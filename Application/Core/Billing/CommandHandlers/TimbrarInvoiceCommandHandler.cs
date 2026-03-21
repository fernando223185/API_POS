using Application.Abstractions.Billing;
using Application.Core.Billing.Commands;
using Application.DTOs.Billing;
using Domain.Entities;
using MediatR;

namespace Application.Core.Billing.CommandHandlers
{
    /// <summary>
    /// Handler para timbrar una factura borrador existente
    /// </summary>
    public class TimbrarInvoiceCommandHandler : IRequestHandler<TimbrarInvoiceCommand, InvoiceResponseDto>
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ISapiensService _sapiensService;

        public TimbrarInvoiceCommandHandler(
            IInvoiceRepository invoiceRepository,
            ISapiensService sapiensService)
        {
            _invoiceRepository = invoiceRepository;
            _sapiensService = sapiensService;
        }

        public async Task<InvoiceResponseDto> Handle(
            TimbrarInvoiceCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                var request = command.Request;
                Console.WriteLine($"🎫 Iniciando timbrado de factura ID: {request.InvoiceId}");

                // 1. Cargar factura con detalles
                var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId);

                if (invoice == null)
                {
                    return new InvoiceResponseDto
                    {
                        Message = $"Factura con ID {request.InvoiceId} no encontrada",
                        Error = 1
                    };
                }

                // 2. Validaciones
                if (invoice.Status != "Borrador")
                {
                    return new InvoiceResponseDto
                    {
                        Message = $"Solo se pueden timbrar facturas en estado Borrador. Estado actual: {invoice.Status}",
                        Error = 1
                    };
                }

                if (!string.IsNullOrEmpty(invoice.Uuid))
                {
                    return new InvoiceResponseDto
                    {
                        Message = $"La factura ya tiene UUID asignado: {invoice.Uuid}",
                        Error = 1
                    };
                }

                if (!invoice.Details.Any())
                {
                    return new InvoiceResponseDto
                    {
                        Message = "La factura no tiene conceptos/detalles para timbrar",
                        Error = 1
                    };
                }

                Console.WriteLine($"   ✓ Validaciones pasadas");
                Console.WriteLine($"   Factura: {invoice.Serie}-{invoice.Folio}");
                Console.WriteLine($"   Emisor: {invoice.EmisorNombre} (RFC: {invoice.EmisorRfc})");
                Console.WriteLine($"   Receptor: {invoice.ReceptorNombre} (RFC: {invoice.ReceptorRfc})");
                Console.WriteLine($"   Total: ${invoice.Total:N2}");
                Console.WriteLine($"   Conceptos: {invoice.Details.Count}");

                // 3. Construir objeto CFDI 4.0 para Sapiens
                var cfdiData = BuildCfdiObject(invoice);
                Console.WriteLine($"   📋 CFDI construido");

                // 4. Timbrar con Sapiens
                Console.WriteLine($"   🔄 Enviando a Sapiens para timbrado...");
                var timbradoResponse = await _sapiensService.TimbrarFacturaAsync(
                    cfdiData,
                    request.Version
                );

                if (timbradoResponse == null || timbradoResponse.status != "success")
                {
                    Console.WriteLine($"   ❌ Error en respuesta de Sapiens: {timbradoResponse?.status}");
                    return new InvoiceResponseDto
                    {
                        Message = $"Error al timbrar con Sapiens: {timbradoResponse?.status ?? "Sin respuesta"}",
                        Error = 1
                    };
                }

                var sapienData = timbradoResponse.data;
                if (sapienData == null)
                {
                    return new InvoiceResponseDto
                    {
                        Message = "Sapiens no devolvió datos de timbrado",
                        Error = 1
                    };
                }

                Console.WriteLine($"   ✓ Timbrado exitoso - UUID: {sapienData.uuid}");

                // 5. Actualizar factura con datos del timbrado
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

                // 6. Guardar cambios
                Console.WriteLine($"   💾 Actualizando factura...");
                var updatedInvoice = await _invoiceRepository.UpdateAsync(invoice);

                // 7. Mapear a DTO
                var invoiceDto = MapToDto(updatedInvoice);

                Console.WriteLine($"   ✓ Factura timbrada exitosamente");

                return new InvoiceResponseDto
                {
                    Message = $"Factura {updatedInvoice.Serie}-{updatedInvoice.Folio} timbrada exitosamente con UUID: {updatedInvoice.Uuid}",
                    Error = 0,
                    Data = invoiceDto
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al timbrar factura: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                return new InvoiceResponseDto
                {
                    Message = $"Error al timbrar factura: {ex.Message}",
                    Error = 1
                };
            }
        }

        /// <summary>
        /// Construye el objeto CFDI 4.0 para enviar a Sapiens
        /// </summary>
        private object BuildCfdiObject(Invoice invoice)
        {
            var ic = System.Globalization.CultureInfo.InvariantCulture;

            // Construir array de conceptos
            var conceptos = invoice.Details.Select(d => new
            {
                ClaveProdServ = d.ClaveProdServ,
                NoIdentificacion = d.NoIdentificacion,
                Cantidad = d.Cantidad.ToString("0.######", ic),
                ClaveUnidad = d.ClaveUnidad,
                Unidad = d.Unidad,
                Descripcion = d.Descripcion,
                ValorUnitario = d.ValorUnitario.ToString("0.00####", ic),
                Importe = d.Importe.ToString("0.00####", ic),
                Descuento = d.Descuento.ToString("0.00", ic),
                ObjetoImp = d.ObjetoImp,
                Impuestos = BuildImpuestosConcepto(d)
            }).ToList();

            // Construir objeto completo CFDI
            var cfdi = new
            {
                Version = "4.0",
                FormaPago = invoice.FormaPago,
                Serie = invoice.Serie,
                Folio = invoice.Folio,
                Fecha = invoice.InvoiceDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                MetodoPago = invoice.MetodoPago,
                Sello = "",
                NoCertificado = "",
                Certificado = "",
                CondicionesDePago = invoice.CondicionesDePago,
                SubTotal = invoice.SubTotal.ToString("0.00", ic),
                Descuento = invoice.DiscountAmount.ToString("0.00", ic),
                Moneda = invoice.Moneda,
                TipoCambio = invoice.TipoCambio.ToString("0.######", ic),
                Total = invoice.Total.ToString("0.00", ic),
                TipoDeComprobante = invoice.TipoDeComprobante,
                Exportacion = "01",
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
                    UsoCfdi = invoice.ReceptorUsoCfdi
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
            var ic = System.Globalization.CultureInfo.InvariantCulture;
            var impuestos = new
            {
                Traslados = detail.TieneTraslados ? new[]
                {
                    new
                    {
                        Base = detail.TrasladoBase?.ToString("0.00####", ic),
                        Impuesto = detail.TrasladoImpuesto,
                        TipoFactor = detail.TrasladoTipoFactor,
                        TasaOCuota = detail.TrasladoTasaOCuota?.ToString("0.000000", ic),
                        Importe = detail.TrasladoImporte?.ToString("0.00", ic)
                    }
                } : null,

                Retenciones = detail.TieneRetenciones ? new[]
                {
                    new
                    {
                        Base = detail.RetencionBase?.ToString("0.00####", ic),
                        Impuesto = detail.RetencionImpuesto,
                        TipoFactor = detail.RetencionTipoFactor,
                        TasaOCuota = detail.RetencionTasaOCuota?.ToString("0.000000", ic),
                        Importe = detail.RetencionImporte?.ToString("0.00", ic)
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

            var retencionesGlobales = invoice.Details
                .Where(d => d.TieneRetenciones)
                .GroupBy(d => new { d.RetencionImpuesto })
                .Select(g => new
                {
                    Importe = g.Sum(d => d.RetencionImporte)?.ToString("0.00", ic),
                    Impuesto = g.Key.RetencionImpuesto
                })
                .ToList();

            var trasladosGlobales = invoice.Details
                .Where(d => d.TieneTraslados)
                .GroupBy(d => new { d.TrasladoImpuesto, d.TrasladoTipoFactor, d.TrasladoTasaOCuota })
                .Select(g => new
                {
                    Base = g.Sum(d => d.TrasladoBase)?.ToString("0.00", ic),
                    Importe = g.Sum(d => d.TrasladoImporte)?.ToString("0.00", ic),
                    Impuesto = g.Key.TrasladoImpuesto,
                    TasaOCuota = g.Key.TrasladoTasaOCuota?.ToString("0.000000", ic),
                    TipoFactor = g.Key.TrasladoTipoFactor
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
