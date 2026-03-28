using Application.Abstractions.Billing;
using Application.Abstractions.Sales;
using Application.Core.Billing.Commands;
using Application.DTOs.Billing;
using MediatR;
using System.Globalization;

namespace Application.Core.Billing.CommandHandlers
{
    /// <summary>
    /// Handler para timbrar un CFDI con Sapiens
    /// </summary>
    public class TimbrarCfdiCommandHandler : IRequestHandler<TimbrarCfdiCommand, TimbrarCfdiResponseDto>
    {
        private readonly ISapiensService _sapiensService;
        private readonly ISaleRepository _saleRepository;

        public TimbrarCfdiCommandHandler(
            ISapiensService sapiensService,
            ISaleRepository saleRepository)
        {
            _sapiensService = sapiensService;
            _saleRepository = saleRepository;
        }

        public async Task<TimbrarCfdiResponseDto> Handle(
            TimbrarCfdiCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"📄 Iniciando timbrado CFDI para venta ID: {request.RequestData.SaleId}");

                // 1. Obtener la venta completa con todas sus relaciones
                var sale = await _saleRepository.GetSaleForInvoicingAsync(request.RequestData.SaleId);

                if (sale == null)
                {
                    return new TimbrarCfdiResponseDto
                    {
                        Message = $"Venta con ID {request.RequestData.SaleId} no encontrada",
                        Error = 1
                    };
                }

                // 2. Validaciones
                if (sale.Status != "Completed")
                {
                    return new TimbrarCfdiResponseDto
                    {
                        Message = $"La venta debe estar completada. Estado actual: {sale.Status}",
                        Error = 1
                    };
                }

                if (!sale.IsPaid)
                {
                    return new TimbrarCfdiResponseDto
                    {
                        Message = "La venta debe estar pagada para timbrar",
                        Error = 1
                    };
                }

                if (!string.IsNullOrEmpty(sale.InvoiceUuid))
                {
                    return new TimbrarCfdiResponseDto
                    {
                        Message = $"La venta ya está timbrada con UUID: {sale.InvoiceUuid}",
                        Error = 1
                    };
                }

                if (sale.Company == null)
                {
                    return new TimbrarCfdiResponseDto
                    {
                        Message = "La venta no tiene empresa emisora asignada",
                        Error = 1
                    };
                }

                Console.WriteLine($"   ✓ Validaciones pasadas");
                Console.WriteLine($"   Empresa: {sale.Company.LegalName} (RFC: {sale.Company.TaxId})");
                Console.WriteLine($"   Cliente: {sale.CustomerName ?? "Público General"}");
                Console.WriteLine($"   Total: ${sale.Total:N2}");

                // 3. Determinar Serie y Folio
                var serie = request.RequestData.Serie ?? sale.Company.InvoiceSeries ?? "A";
                var folio = request.RequestData.Folio ?? sale.Company.InvoiceCurrentFolio.ToString();

                // 4. Construir objeto CFDI 4.0 para Sapiens
                var cfdiData = BuildCfdiObject(sale, request.RequestData, serie, folio);

                Console.WriteLine($"   📋 CFDI construido - Serie: {serie}, Folio: {folio}");

                // 5. Timbrar con Sapiens
                Console.WriteLine($"   🔄 Enviando a Sapiens para timbrado...");
                var timbradoResponse = await _sapiensService.TimbrarFacturaAsync(
                    cfdiData,
                    request.RequestData.Version
                );

                if (timbradoResponse.status != "success" || timbradoResponse.data == null)
                {
                    return new TimbrarCfdiResponseDto
                    {
                        Message = "Error al timbrar con Sapiens",
                        Error = 2
                    };
                }

                Console.WriteLine($"   ✅ Timbrado exitoso - UUID: {timbradoResponse.data.uuid}");

                // 6. Actualizar la venta con el UUID
                sale.InvoiceUuid = timbradoResponse.data.uuid;
                sale.InvoiceSeries = serie;
                sale.InvoiceFolio = folio;
                sale.InvoiceDate = DateTime.UtcNow;
                sale.InvoiceXml = timbradoResponse.data.cfdi;
                sale.UpdatedAt = DateTime.UtcNow;

                await _saleRepository.UpdateAsync(sale);

                // 7. Incrementar el folio de la empresa
                sale.Company.InvoiceCurrentFolio += 1;
                // TODO: Actualizar el folio en el repositorio de empresa si existe

                Console.WriteLine($"   💾 Venta actualizada con UUID");

                // 8. Construir respuesta
                var response = new TimbrarCfdiResponseDto
                {
                    Message = "CFDI timbrado exitosamente",
                    Error = 0,
                    Data = new TimbradoDataDto
                    {
                        SaleId = sale.Id,
                        SaleCode = sale.Code,
                        Uuid = timbradoResponse.data.uuid,
                        Serie = serie,
                        Folio = folio,
                        FechaTimbrado = timbradoResponse.data.fechaTimbrado,
                        NoCertificadoSat = timbradoResponse.data.noCertificadoSAT,
                        NoCertificadoCfdi = timbradoResponse.data.noCertificadoCFDI,
                        SelloSat = timbradoResponse.data.selloSAT,
                        SelloCfdi = timbradoResponse.data.selloCFDI,
                        CadenaOriginalSat = timbradoResponse.data.cadenaOriginalSAT,
                        QrCode = timbradoResponse.data.qrCode,
                        XmlCfdi = timbradoResponse.data.cfdi,
                        Total = sale.Total,
                        RfcEmisor = sale.Company.TaxId,
                        RfcReceptor = sale.Customer?.TaxId ?? "XAXX010101000",
                        TimbradoAt = DateTime.UtcNow
                    }
                };

                Console.WriteLine($"🎉 Proceso de timbrado completado exitosamente");

                return response;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"❌ Error de validación: {ex.Message}");
                return new TimbrarCfdiResponseDto
                {
                    Message = ex.Message,
                    Error = 1
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al timbrar CFDI: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                return new TimbrarCfdiResponseDto
                {
                    Message = $"Error al timbrar CFDI: {ex.Message}",
                    Error = 2
                };
            }
        }

        /// <summary>
        /// Construye el objeto CFDI 4.0 en el formato requerido por Sapiens.
        /// </summary>
        private object BuildCfdiObject(
            Domain.Entities.Sale sale,
            TimbrarCfdiRequestDto request,
            string serie,
            string folio)
        {
            var ic = System.Globalization.CultureInfo.InvariantCulture;

            // Calcular totales de impuestos
            var totalImpuestosTrasladados = sale.Details
                .Where(d => d.TaxAmount > 0)
                .Sum(d => d.TaxAmount);

            // Construir conceptos (productos)
            var conceptos = sale.Details.Select(detail => new
            {
                ClaveProdServ = "01010101", // TODO: Obtener del producto
                NoIdentificacion = detail.ProductCode,
                Cantidad = detail.Quantity.ToString("0.0", ic),
                ClaveUnidad = "H87", // TODO: Obtener del producto (Pieza)
                Unidad = "Pieza",
                Descripcion = detail.ProductName,
                ValorUnitario = detail.UnitPrice.ToString("0.0000", ic),
                Importe = (detail.Quantity * detail.UnitPrice).ToString("0.0000", ic),
                Descuento = detail.DiscountAmount.ToString("0.00", ic),
                ObjetoImp = detail.TaxAmount > 0 ? "02" : "01",
                Impuestos = detail.TaxAmount > 0 ? new
                {
                    Traslados = new[]
                    {
                        new
                        {
                            Base = (detail.Quantity * detail.UnitPrice - detail.DiscountAmount).ToString("0.0000", ic),
                            Importe = detail.TaxAmount.ToString("0.0000", ic),
                            Impuesto = "002",
                            TasaOCuota = "0.160000",
                            TipoFactor = "Tasa"
                        }
                    }
                } : null
            }).ToArray();

            // Construir objeto CFDI completo
            var cfdi = new
            {
                Version = "4.0",
                FormaPago = request.FormaPago,
                Serie = serie,
                Folio = folio,
                Fecha = ObtenerFechaCfdi(),
                Sello = "",
                NoCertificado = "",
                Certificado = "",
                CondicionesDePago = request.CondicionesDePago ?? "",
                SubTotal = sale.SubTotal.ToString("0.00", ic),
                Descuento = sale.DiscountAmount.ToString("0.00", ic),
                Moneda = "MXN",
                TipoCambio = "1",
                Total = sale.Total.ToString("0.00", ic),
                TipoDeComprobante = "I",
                Exportacion = "01",
                MetodoPago = request.MetodoPago,
                LugarExpedicion = sale.Company.FiscalZipCode,

                Emisor = new
                {
                    Rfc = sale.Company.TaxId,
                    Nombre = sale.Company.LegalName,
                    RegimenFiscal = sale.Company.SatTaxRegime ?? "601"
                },

                Receptor = new
                {
                    Rfc = sale.Customer?.TaxId ?? "XAXX010101000",
                    Nombre = sale.CustomerName ?? "PUBLICO EN GENERAL",
                    DomicilioFiscalReceptor = sale.Customer?.ZipCode ?? "00000",
                    RegimenFiscalReceptor = sale.Customer?.SatTaxRegime ?? "616",
                    UsoCFDI = request.UsoCfdi ?? sale.Customer?.SatCfdiUse ?? "G03"
                },

                Conceptos = conceptos,

                Impuestos = totalImpuestosTrasladados > 0 ? new
                {
                    TotalImpuestosTrasladados = totalImpuestosTrasladados.ToString("0.00", ic),
                    Traslados = new[]
                    {
                        new
                        {
                            Base = (sale.SubTotal - sale.DiscountAmount).ToString("0.00", ic),
                            Importe = totalImpuestosTrasladados.ToString("0.00", ic),
                            Impuesto = "002",
                            TasaOCuota = "0.160000",
                            TipoFactor = "Tasa"
                        }
                    }
                } : null
            };

            return cfdi;
        }

        private static string ObtenerFechaCfdi()
        {
            TimeZoneInfo tz;
            try { tz = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)"); }
            catch { tz = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City"); }
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz)
                .ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
        }
    }
}
