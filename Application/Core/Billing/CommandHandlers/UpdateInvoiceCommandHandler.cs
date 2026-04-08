using Application.Abstractions.Billing;
using Application.Core.Billing.Commands;
using Application.DTOs.Billing;
using Domain.Entities;
using MediatR;

namespace Application.Core.Billing.CommandHandlers
{
    /// <summary>
    /// Handler para actualizar una factura en estado Borrador
    /// </summary>
    public class UpdateInvoiceCommandHandler : IRequestHandler<UpdateInvoiceCommand, InvoiceResponseDto>
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public UpdateInvoiceCommandHandler(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<InvoiceResponseDto> Handle(UpdateInvoiceCommand command, CancellationToken cancellationToken)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(command.InvoiceId);

            if (invoice == null)
            {
                return new InvoiceResponseDto
                {
                    Message = $"Factura con ID {command.InvoiceId} no encontrada",
                    Error = 1
                };
            }

            if (invoice.Status != "Borrador")
            {
                return new InvoiceResponseDto
                {
                    Message = $"Solo se pueden modificar facturas en estado Borrador. Estado actual: {invoice.Status}",
                    Error = 1
                };
            }

            var req = command.Request;

            // Datos del receptor
            if (!string.IsNullOrWhiteSpace(req.ClientRFC))
                invoice.ReceptorRfc = req.ClientRFC;

            if (!string.IsNullOrWhiteSpace(req.ClientName))
                invoice.ReceptorNombre = req.ClientName;

            if (!string.IsNullOrWhiteSpace(req.ClientPostalCode))
                invoice.ReceptorDomicilioFiscal = req.ClientPostalCode;

            if (!string.IsNullOrWhiteSpace(req.ClientRegimenFiscal))
                invoice.ReceptorRegimenFiscal = req.ClientRegimenFiscal;

            if (!string.IsNullOrWhiteSpace(req.ClientUsoCFDI))
                invoice.ReceptorUsoCfdi = req.ClientUsoCFDI;

            // Datos del comprobante
            if (req.InvoiceDate.HasValue)
                invoice.InvoiceDate = req.InvoiceDate.Value;

            if (!string.IsNullOrWhiteSpace(req.PaymentForm))
                invoice.FormaPago = req.PaymentForm;

            if (!string.IsNullOrWhiteSpace(req.PaymentMethod))
                invoice.MetodoPago = req.PaymentMethod;

            if (req.CondicionesDePago != null)
                invoice.CondicionesDePago = req.CondicionesDePago;

            if (!string.IsNullOrWhiteSpace(req.Currency))
                invoice.Moneda = req.Currency;

            if (req.ExchangeRate.HasValue)
                invoice.TipoCambio = req.ExchangeRate.Value;

            if (req.Notes != null)
                invoice.Notes = req.Notes;

            // Montos
            if (req.Subtotal.HasValue)
                invoice.SubTotal = req.Subtotal.Value;

            if (req.TotalDiscount.HasValue)
                invoice.DiscountAmount = req.TotalDiscount.Value;

            if (req.TotalTax.HasValue)
                invoice.TaxAmount = req.TotalTax.Value;

            if (req.Total.HasValue)
                invoice.Total = req.Total.Value;

            invoice.UpdatedAt = DateTime.UtcNow;

            // Guardar lista de detalles existentes ANTES de actualizar
            // Se busca por ID, luego por código de producto, luego por descripción
            var existingDetails = invoice.Details.ToList();

            var updated = await _invoiceRepository.UpdateAsync(invoice);

            // Actualizar ítems si se enviaron
            if (req.Items != null && req.Items.Count > 0)
            {
                var details = req.Items.Select(item =>
                {
                    // id > 0 = detalle existente, id = 0 o null = nuevo ítem
                    var existing = (item.Id.HasValue && item.Id.Value > 0)
                        ? existingDetails.FirstOrDefault(d => d.Id == item.Id.Value)
                        : null;

                    // TaxRate puede venir como 16 (porcentaje) o 0.16 (decimal)
                    decimal taxRateNorm = item.TaxRate > 1 ? item.TaxRate / 100m : item.TaxRate;
                    bool tieneIva = taxRateNorm > 0 || item.TaxAmount > 0;
                    decimal tasaOCuota = tieneIva ? (taxRateNorm > 0 ? Math.Round(taxRateNorm, 6) : 0.160000m) : 0m;
                    decimal taxImporte = item.TaxAmount > 0
                        ? Math.Round(item.TaxAmount, 2)
                        : Math.Round(item.Amount * tasaOCuota, 2);

                    // ClaveProdServ: del item > del detalle existente > del producto cargado
                    var claveProdServ = !string.IsNullOrWhiteSpace(item.ClaveProdServ)
                        ? item.ClaveProdServ
                        : existing?.ClaveProdServ
                          ?? existing?.Product?.SatCode
                          ?? "01010101";

                    return new InvoiceDetail
                    {
                        InvoiceId = updated.Id,
                        ProductId = (item.ProductId.HasValue && item.ProductId.Value > 0) ? item.ProductId : existing?.ProductId,
                        NoIdentificacion = item.ProductCode ?? existing?.NoIdentificacion,
                        ClaveProdServ = claveProdServ,
                        Cantidad = item.Quantity,
                        ClaveUnidad = item.Unit ?? existing?.ClaveUnidad ?? "H87",
                        Unidad = ResolveUnidad(item.Unit ?? existing?.ClaveUnidad) ?? existing?.Unidad,
                        Descripcion = item.Description ?? existing?.Descripcion ?? string.Empty,
                        ValorUnitario = item.UnitPrice,
                        Descuento = item.Discount,
                        Importe = item.Amount,
                        ObjetoImp = tieneIva ? "02" : "01",

                        // Impuestos trasladados (IVA)
                        TieneTraslados = tieneIva,
                        TrasladoBase = tieneIva ? item.Amount : null,
                        TrasladoImpuesto = tieneIva ? "002" : null,
                        TrasladoTipoFactor = tieneIva ? "Tasa" : null,
                        TrasladoTasaOCuota = tieneIva ? tasaOCuota : null,
                        TrasladoImporte = tieneIva ? taxImporte : null
                    };
                }).ToList();

                await _invoiceRepository.ReplaceDetailsAsync(updated.Id, details);
            }

            var invoiceWithRelations = await _invoiceRepository.GetByIdAsync(updated.Id);

            var dto = MapToDto(invoiceWithRelations!);

            return new InvoiceResponseDto
            {
                Message = $"Factura {updated.Serie}-{updated.Folio} actualizada exitosamente",
                Error = 0,
                Data = dto
            };
        }

        private InvoiceDto MapToDto(Invoice invoice)
        {
            return new InvoiceDto
            {
                Id = invoice.Id,
                SaleId = invoice.SaleId,
                SaleCode = invoice.Sale?.Code,

                Serie = invoice.Serie,
                Folio = invoice.Folio,
                InvoiceDate = invoice.InvoiceDate,
                FormaPago = invoice.FormaPago,
                MetodoPago = invoice.MetodoPago,
                CondicionesDePago = invoice.CondicionesDePago,
                TipoDeComprobante = invoice.TipoDeComprobante,
                LugarExpedicion = invoice.LugarExpedicion,

                CompanyId = invoice.CompanyId,
                EmisorRfc = invoice.EmisorRfc,
                EmisorNombre = invoice.EmisorNombre,
                EmisorRegimenFiscal = invoice.EmisorRegimenFiscal,

                CustomerId = invoice.CustomerId,
                ReceptorRfc = invoice.ReceptorRfc,
                ReceptorNombre = invoice.ReceptorNombre,
                ReceptorDomicilioFiscal = invoice.ReceptorDomicilioFiscal,
                ReceptorRegimenFiscal = invoice.ReceptorRegimenFiscal,
                ReceptorUsoCfdi = invoice.ReceptorUsoCfdi,

                SubTotal = invoice.SubTotal,
                DiscountAmount = invoice.DiscountAmount,
                TaxAmount = invoice.TaxAmount,
                Total = invoice.Total,
                Moneda = invoice.Moneda,
                TipoCambio = invoice.TipoCambio,

                Status = invoice.Status,
                Uuid = invoice.Uuid,
                TimbradoAt = invoice.TimbradoAt,

                XmlCfdi = invoice.XmlCfdi,
                CadenaOriginalSat = invoice.CadenaOriginalSat,
                SelloCfdi = invoice.SelloCfdi,
                SelloSat = invoice.SelloSat,
                NoCertificadoCfdi = invoice.NoCertificadoCfdi,
                NoCertificadoSat = invoice.NoCertificadoSat,
                QrCode = invoice.QrCode,

                CancelledAt = invoice.CancelledAt,
                CancellationReason = invoice.CancellationReason,
                CancelledByUserId = invoice.CancelledByUserId,
                CancelledByUserName = invoice.CancelledBy?.Name,

                Notes = invoice.Notes,
                CreatedAt = invoice.CreatedAt,
                UpdatedAt = invoice.UpdatedAt,

                Details = invoice.Details.Select(d => new InvoiceDetailDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ClaveProdServ = d.ClaveProdServ,
                    NoIdentificacion = d.NoIdentificacion,
                    Cantidad = d.Cantidad,
                    ClaveUnidad = d.ClaveUnidad,
                    Unidad = d.Unidad,
                    Descripcion = d.Descripcion,
                    ValorUnitario = d.ValorUnitario,
                    Descuento = d.Descuento,
                    Importe = d.Importe,
                    ObjetoImp = d.ObjetoImp
                }).ToList()
            };
        }

        private static string? ResolveUnidad(string? claveUnidad) => claveUnidad switch
        {
            "H87" => "Pieza",
            "KGM" => "Kilogramo",
            "LTR" => "Litro",
            "MTR" => "Metro",
            "MTK" => "Metro cuadrado",
            "MTQ" => "Metro cúbico",
            "E48" => "Unidad de servicio",
            "ACT" => "Actividad",
            "MON" => "Mes",
            "DAY" => "Día",
            "HUR" => "Hora",
            _ => claveUnidad
        };
    }
}
