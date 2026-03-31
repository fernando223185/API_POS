using Application.Abstractions.Billing;
using Application.Abstractions.Config;
using Application.Abstractions.Sales;
using Application.Core.Billing.Commands;
using Application.DTOs.Billing;
using Domain.Entities;
using MediatR;

namespace Application.Core.Billing.CommandHandlers
{
    public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, InvoiceResponseDto>
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ISaleRepository _saleRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ISapiensService _sapiensService;

        public CreateInvoiceCommandHandler(
            IInvoiceRepository invoiceRepository,
            ISaleRepository saleRepository,
            ICompanyRepository companyRepository,
            ISapiensService sapiensService)
        {
            _invoiceRepository = invoiceRepository;
            _saleRepository = saleRepository;
            _companyRepository = companyRepository;
            _sapiensService = sapiensService;
        }

        public async Task<InvoiceResponseDto> Handle(
            CreateInvoiceCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                var req = command.Request;

                // Validaciones básicas
                if (req.Receptor == null)
                    return Error("Debe proporcionar los datos del receptor");

                if (req.Items == null || req.Items.Count == 0)
                    return Error("Debe proporcionar al menos un concepto (items)");

                // Cargar empresa emisora
                var company = await _companyRepository.GetByIdAsync(req.CompanyId);
                if (company == null)
                    return Error($"Empresa con ID {req.CompanyId} no encontrada");

                // Serie y folio
                var serie = req.Serie ?? company.InvoiceSeries ?? "A";
                var folio = await _invoiceRepository.GetNextFolioAsync(company.Id, serie);

                // Construir Invoice
                var invoice = new Invoice
                {
                    // Solo se guarda la primera venta en SaleId para compatibilidad con el modelo
                    SaleId = req.SaleIds?.FirstOrDefault() > 0 ? req.SaleIds.First() : null,

                    Serie = serie,
                    Folio = folio,
                    InvoiceDate = req.InvoiceDate ?? DateTime.UtcNow,
                    FormaPago = req.FormaPago,
                    MetodoPago = req.MetodoPago,
                    CondicionesDePago = req.CondicionesDePago,
                    TipoDeComprobante = "I",
                    LugarExpedicion = company.FiscalZipCode ?? string.Empty,

                    CompanyId = company.Id,
                    EmisorRfc = company.TaxId ?? string.Empty,
                    EmisorNombre = company.LegalName ?? string.Empty,
                    EmisorRegimenFiscal = company.SatTaxRegime,

                    CustomerId = req.Receptor.CustomerId,
                    ReceptorRfc = req.Receptor.Rfc,
                    ReceptorNombre = req.Receptor.Nombre,
                    ReceptorDomicilioFiscal = req.Receptor.DomicilioFiscal,
                    ReceptorRegimenFiscal = req.Receptor.RegimenFiscal,
                    ReceptorUsoCfdi = req.Receptor.UsoCfdi,

                    SubTotal = req.Subtotal,
                    DiscountAmount = req.TotalDiscount,
                    TaxAmount = req.TotalTax,
                    Total = req.Total,
                    Moneda = req.Currency,
                    TipoCambio = req.ExchangeRate,

                    Status = "Borrador",
                    Notes = req.Notes,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = command.UserId,

                    Details = new List<InvoiceDetail>()
                };

                BuildDetailsFromItems(req.Items, invoice);

                // Guardar
                var saved = await _invoiceRepository.CreateAsync(invoice);

                // Vincular todas las ventas al nuevo invoice
                if (req.SaleIds?.Any() == true)
                    await _saleRepository.SetInvoiceIdBulkAsync(req.SaleIds, saved.Id);

                // Timbrar inmediatamente si se solicitó
                if (req.TimbrarInmediatamente)
                    saved = await TimbrarInvoiceAsync(saved, req.Version);

                var invoiceWithRelations = await _invoiceRepository.GetByIdAsync(saved.Id);
                if (invoiceWithRelations == null)
                    return Error("Error al recargar la factura creada");

                return new InvoiceResponseDto
                {
                    Message = saved.Status == "Timbrada"
                        ? $"Factura {saved.Serie}-{saved.Folio} timbrada exitosamente con UUID: {saved.Uuid}"
                        : $"Factura {saved.Serie}-{saved.Folio} guardada como borrador",
                    Error = 0,
                    Data = MapToDto(invoiceWithRelations)
                };
            }
            catch (Exception ex)
            {
                return Error($"Error al crear factura: {ex.Message}");
            }
        }

        private static InvoiceResponseDto Error(string message) =>
            new() { Message = message, Error = 1 };

        /// <summary>
        /// Convierte la lista de items (formato simplificado de UpdateInvoiceItemDto) a InvoiceDetails
        /// </summary>
        private static void BuildDetailsFromItems(List<UpdateInvoiceItemDto> items, Invoice invoice)
        {
            foreach (var item in items)
            {
                decimal taxRateNorm = item.TaxRate > 1 ? item.TaxRate / 100m : item.TaxRate;
                bool tieneIva = taxRateNorm > 0 || item.TaxAmount > 0;
                decimal tasaOCuota = tieneIva ? (taxRateNorm > 0 ? Math.Round(taxRateNorm, 6) : 0.160000m) : 0m;
                decimal taxImporte = item.TaxAmount > 0
                    ? Math.Round(item.TaxAmount, 2)
                    : Math.Round(item.Amount * tasaOCuota, 2);

                invoice.Details.Add(new InvoiceDetail
                {
                    ProductId = item.ProductId,
                    ClaveProdServ = !string.IsNullOrWhiteSpace(item.ClaveProdServ) ? item.ClaveProdServ : "01010101",
                    NoIdentificacion = item.ProductCode,
                    Cantidad = item.Quantity,
                    ClaveUnidad = item.Unit ?? "H87",
                    Unidad = (item.Unit) switch { "H87" => "Pieza", "KGM" => "Kilogramo", "LTR" => "Litro", "MTR" => "Metro", "E48" => "Unidad de servicio", _ => item.Unit },
                    Descripcion = item.Description ?? string.Empty,
                    ValorUnitario = item.UnitPrice,
                    Importe = item.Amount,
                    Descuento = item.Discount,
                    ObjetoImp = tieneIva ? "02" : "01",
                    TieneTraslados = tieneIva,
                    TrasladoBase = tieneIva ? item.Amount : null,
                    TrasladoImpuesto = tieneIva ? "002" : null,
                    TrasladoTipoFactor = tieneIva ? "Tasa" : null,
                    TrasladoTasaOCuota = tieneIva ? tasaOCuota : null,
                    TrasladoImporte = tieneIva ? taxImporte : null,
                    TieneRetenciones = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Timbra una factura borrador con Sapiens
        /// </summary>
        private async Task<Invoice> TimbrarInvoiceAsync(Invoice invoice, string version)
        {
            // Construir objeto CFDI 4.0 para Sapiens
            var cfdiData = BuildCfdiObject(invoice);

            Console.WriteLine($"      📋 CFDI construido - Serie: {invoice.Serie}, Folio: {invoice.Folio}");

            // Timbrar con Sapiens
            Console.WriteLine($"      🔄 Enviando a Sapiens para timbrado...");
            var timbradoResponse = await _sapiensService.TimbrarFacturaAsync(cfdiData, version);

            if (timbradoResponse == null || timbradoResponse.status != "success")
            {
                throw new Exception($"Error al timbrar con Sapiens: {timbradoResponse?.status}");
            }

            var sapienData = timbradoResponse.data;
            if (sapienData == null)
            {
                throw new Exception("Sapiens no devolvió datos de timbrado");
            }

            Console.WriteLine($"      ✓ Timbrado exitoso - UUID: {sapienData.uuid}");

            // Actualizar Invoice con datos del timbrado
            invoice.Status = "Timbrada";
            invoice.Uuid = sapienData.uuid;
            invoice.TimbradoAt = DateTime.UtcNow;
            invoice.XmlCfdi = sapienData.cfdi;
            invoice.CadenaOriginalSat = sapienData.cadenaOriginalSAT;
            invoice.SelloCfdi = sapienData.selloCFDI;
            invoice.SelloSat = sapienData.selloSAT;
            invoice.NoCertificadoCfdi = sapienData.noCertificadoCFDI;
            invoice.NoCertificadoSat = sapienData.noCertificadoSAT;
            invoice.QrCode = sapienData.qrCode;
            invoice.UpdatedAt = DateTime.UtcNow;

            // Inicializar campos de cuentas por cobrar para facturas PPD
            if (invoice.MetodoPago == "PPD")
            {
                invoice.DueDate = (invoice.TimbradoAt ?? invoice.InvoiceDate).AddDays(30);
                invoice.PaidAmount = 0;
                invoice.BalanceAmount = invoice.Total;
                invoice.NextPartialityNumber = 1;
                invoice.TotalPartialities = 0;
                invoice.DaysOverdue = 0;
                invoice.LastPaymentDate = null;
                invoice.PaymentStatus = "Pending";
            }

            return invoice;
        }

        /// <summary>
        /// Construye el objeto CFDI 4.0 para enviar a Sapiens
        /// </summary>
        private object BuildCfdiObject(Invoice invoice)
        {
            // Construir array de conceptos
            var conceptos = invoice.Details.Select(d => new
            {
                ClaveProdServ = d.ClaveProdServ,
                NoIdentificacion = d.NoIdentificacion,
                Cantidad = d.Cantidad,
                ClaveUnidad = d.ClaveUnidad,
                Unidad = d.Unidad,
                Descripcion = d.Descripcion,
                ValorUnitario = d.ValorUnitario,
                Importe = d.Importe,
                Descuento = d.Descuento > 0 ? d.Descuento : (decimal?)null,
                ObjetoImp = d.ObjetoImp,
                Impuestos = BuildImpuestosConcepto(d)
            }).ToList();

            // Construir objeto completo CFDI
            var cfdi = new
            {
                Version = "4.0",
                Serie = invoice.Serie,
                Folio = invoice.Folio,
                Fecha = invoice.InvoiceDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                FormaPago = invoice.FormaPago,
                MetodoPago = invoice.MetodoPago,
                CondicionesDePago = invoice.CondicionesDePago,
                SubTotal = invoice.SubTotal,
                Descuento = invoice.DiscountAmount > 0 ? invoice.DiscountAmount : (decimal?)null,
                Moneda = invoice.Moneda,
                TipoCambio = invoice.TipoCambio,
                Total = invoice.Total,
                TipoDeComprobante = invoice.TipoDeComprobante,
                LugarExpedicion = invoice.LugarExpedicion,

                Emisor = new
                {
                    Rfc = invoice.EmisorRfc,
                    Nombre = invoice.EmisorNombre,
                    RegimenFiscal = invoice.EmisorRegimenFiscal
                },

                Receptor = new
                {
                    Rfc = invoice.ReceptorRfc,
                    Nombre = invoice.ReceptorNombre,
                    DomicilioFiscalReceptor = invoice.ReceptorDomicilioFiscal,
                    RegimenFiscalReceptor = invoice.ReceptorRegimenFiscal,
                    UsoCFDI = invoice.ReceptorUsoCfdi
                },

                Conceptos = conceptos,

                Impuestos = BuildImpuestosGlobales(invoice)
            };

            return cfdi;
        }

        /// <summary>
        /// Construye impuestos de un concepto individual
        /// </summary>
        private object? BuildImpuestosConcepto(InvoiceDetail detail)
        {
            var impuestos = new
            {
                Traslados = detail.TieneTraslados ? new[]
                {
                    new
                    {
                        Base = detail.TrasladoBase,
                        Impuesto = detail.TrasladoImpuesto,
                        TipoFactor = detail.TrasladoTipoFactor,
                        TasaOCuota = detail.TrasladoTasaOCuota,
                        Importe = detail.TrasladoImporte
                    }
                } : null,

                Retenciones = detail.TieneRetenciones ? new[]
                {
                    new
                    {
                        Base = detail.RetencionBase,
                        Impuesto = detail.RetencionImpuesto,
                        TipoFactor = detail.RetencionTipoFactor,
                        TasaOCuota = detail.RetencionTasaOCuota,
                        Importe = detail.RetencionImporte
                    }
                } : null
            };

            // Si no hay impuestos, retornar null
            if (impuestos.Traslados == null && impuestos.Retenciones == null)
            {
                return null;
            }

            return impuestos;
        }

        /// <summary>
        /// Construye impuestos globales del comprobante
        /// </summary>
        private object? BuildImpuestosGlobales(Invoice invoice)
        {
            // Calcular totales de traslados y retenciones
            var totalTraslados = invoice.Details
                .Where(d => d.TieneTraslados)
                .Sum(d => d.TrasladoImporte ?? 0);

            var totalRetenciones = invoice.Details
                .Where(d => d.TieneRetenciones)
                .Sum(d => d.RetencionImporte ?? 0);

            if (totalTraslados == 0 && totalRetenciones == 0)
            {
                return null;
            }

            var ic = System.Globalization.CultureInfo.InvariantCulture;

            var trasladosGlobales = invoice.Details
                .Where(d => d.TieneTraslados)
                .GroupBy(d => new { d.TrasladoImpuesto, d.TrasladoTipoFactor, d.TrasladoTasaOCuota })
                .Select(g => new
                {
                    Base = g.Sum(d => d.TrasladoBase)?.ToString("0.00", ic),
                    Impuesto = g.Key.TrasladoImpuesto,
                    TipoFactor = g.Key.TrasladoTipoFactor,
                    TasaOCuota = g.Key.TrasladoTasaOCuota?.ToString("0.000000", ic),
                    Importe = g.Sum(d => d.TrasladoImporte)?.ToString("0.00", ic)
                })
                .ToList();

            var retencionesGlobales = invoice.Details
                .Where(d => d.TieneRetenciones)
                .GroupBy(d => new { d.RetencionImpuesto })
                .Select(g => new
                {
                    Impuesto = g.Key.RetencionImpuesto,
                    Importe = g.Sum(d => d.RetencionImporte)?.ToString("0.00", ic)
                })
                .ToList();

            var impuestos = new
            {
                TotalImpuestosTrasladados = totalTraslados > 0 ? totalTraslados.ToString("0.00", ic) : null,
                TotalImpuestosRetenidos = totalRetenciones > 0 ? totalRetenciones.ToString("0.00", ic) : null,
                Retenciones = retencionesGlobales.Count > 0 ? retencionesGlobales : null,
                Traslados = trasladosGlobales.Count > 0 ? trasladosGlobales : null
            };

            return impuestos;
        }

        /// <summary>
        /// Mapea Invoice entity a InvoiceDto
        /// </summary>
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
                    Importe = d.Importe,
                    Descuento = d.Descuento,
                    ObjetoImp = d.ObjetoImp,

                    TieneTraslados = d.TieneTraslados,
                    TrasladoBase = d.TrasladoBase,
                    TrasladoImpuesto = d.TrasladoImpuesto,
                    TrasladoTipoFactor = d.TrasladoTipoFactor,
                    TrasladoTasaOCuota = d.TrasladoTasaOCuota,
                    TrasladoImporte = d.TrasladoImporte,

                    TieneRetenciones = d.TieneRetenciones,
                    RetencionBase = d.RetencionBase,
                    RetencionImpuesto = d.RetencionImpuesto,
                    RetencionTipoFactor = d.RetencionTipoFactor,
                    RetencionTasaOCuota = d.RetencionTasaOCuota,
                    RetencionImporte = d.RetencionImporte,

                    Notes = d.Notes
                }).ToList(),

                Notes = invoice.Notes,
                CreatedAt = invoice.CreatedAt,
                CreatedByUserId = invoice.CreatedByUserId ?? 0,
                CreatedByUserName = invoice.CreatedBy?.Name,
                UpdatedAt = invoice.UpdatedAt
            };
        }
    }
}
