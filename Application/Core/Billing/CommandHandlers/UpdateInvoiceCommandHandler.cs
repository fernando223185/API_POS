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

            if (!string.IsNullOrWhiteSpace(req.FormaPago))
                invoice.FormaPago = req.FormaPago;

            if (!string.IsNullOrWhiteSpace(req.MetodoPago))
                invoice.MetodoPago = req.MetodoPago;

            if (req.CondicionesDePago != null)
                invoice.CondicionesDePago = req.CondicionesDePago;

            if (!string.IsNullOrWhiteSpace(req.ReceptorRfc))
                invoice.ReceptorRfc = req.ReceptorRfc;

            if (!string.IsNullOrWhiteSpace(req.ReceptorNombre))
                invoice.ReceptorNombre = req.ReceptorNombre;

            if (!string.IsNullOrWhiteSpace(req.ReceptorDomicilioFiscal))
                invoice.ReceptorDomicilioFiscal = req.ReceptorDomicilioFiscal;

            if (req.ReceptorRegimenFiscal != null)
                invoice.ReceptorRegimenFiscal = req.ReceptorRegimenFiscal;

            if (!string.IsNullOrWhiteSpace(req.ReceptorUsoCfdi))
                invoice.ReceptorUsoCfdi = req.ReceptorUsoCfdi;

            if (req.Notes != null)
                invoice.Notes = req.Notes;

            invoice.UpdatedAt = DateTime.UtcNow;

            var updated = await _invoiceRepository.UpdateAsync(invoice);

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
    }
}
