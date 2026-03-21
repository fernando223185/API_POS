using Application.Abstractions.Billing;
using Application.Abstractions.Sales;
using Application.Core.Billing.Commands;
using Application.DTOs.Billing;
using Domain.Entities;
using MediatR;

namespace Application.Core.Billing.CommandHandlers
{
    /// <summary>
    /// Handler para crear una factura (borrador o timbrada)
    /// Puede crear desde una venta existente o manualmente
    /// </summary>
    public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, InvoiceResponseDto>
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ISaleRepository _saleRepository;
        private readonly ISapiensService _sapiensService;

        public CreateInvoiceCommandHandler(
            IInvoiceRepository invoiceRepository,
            ISaleRepository saleRepository,
            ISapiensService sapiensService)
        {
            _invoiceRepository = invoiceRepository;
            _saleRepository = saleRepository;
            _sapiensService = sapiensService;
        }

        public async Task<InvoiceResponseDto> Handle(
            CreateInvoiceCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                var request = command.Request;
                Console.WriteLine($"📄 Iniciando creación de factura");

                Invoice invoice;

                // ========================================
                // CASO 1: Crear factura desde una venta
                // ========================================
                if (request.SaleId.HasValue)
                {
                    Console.WriteLine($"   📦 Creando factura desde venta ID: {request.SaleId}");
                    invoice = await CreateInvoiceFromSaleAsync(request, command.UserId);
                }
                // ========================================
                // CASO 2: Crear factura manualmente
                // ========================================
                else
                {
                    Console.WriteLine($"   ✍️ Creando factura manual");
                    invoice = await CreateManualInvoiceAsync(request, command.UserId);
                }

                // Guardar factura en BD
                Console.WriteLine($"   💾 Guardando factura en BD...");
                var savedInvoice = await _invoiceRepository.CreateAsync(invoice);
                Console.WriteLine($"   ✓ Factura guardada con ID: {savedInvoice.Id}");

                // ========================================
                // CASO: Timbrar inmediatamente
                // ========================================
                if (request.TimbrarInmediatamente)
                {
                    Console.WriteLine($"   🎫 Timbrando factura inmediatamente...");
                    savedInvoice = await TimbrarInvoiceAsync(savedInvoice, request.Version);
                }

                // Recargar con relaciones completas
                var invoiceWithRelations = await _invoiceRepository.GetByIdAsync(savedInvoice.Id);

                if (invoiceWithRelations == null)
                {
                    return new InvoiceResponseDto
                    {
                        Message = "Error al recargar la factura creada",
                        Error = 1
                    };
                }

                // Mapear a DTO
                var invoiceDto = MapToDto(invoiceWithRelations);

                return new InvoiceResponseDto
                {
                    Message = savedInvoice.Status == "Timbrada" 
                        ? $"Factura {savedInvoice.Serie}-{savedInvoice.Folio} timbrada exitosamente con UUID: {savedInvoice.Uuid}" 
                        : $"Factura {savedInvoice.Serie}-{savedInvoice.Folio} guardada como borrador",
                    Error = 0,
                    Data = invoiceDto
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al crear factura: {ex.Message}");
                return new InvoiceResponseDto
                {
                    Message = $"Error al crear factura: {ex.Message}",
                    Error = 1
                };
            }
        }

        /// <summary>
        /// Crea una factura desde una venta existente
        /// </summary>
        private async Task<Invoice> CreateInvoiceFromSaleAsync(
            CreateInvoiceRequestDto request,
            int userId)
        {
            // Cargar venta completa con relaciones
            var sale = await _saleRepository.GetSaleForInvoicingAsync(request.SaleId!.Value);

            if (sale == null)
            {
                throw new Exception($"Venta con ID {request.SaleId} no encontrada");
            }

            // Validaciones
            if (sale.Status != "Completed")
            {
                throw new Exception($"La venta debe estar completada. Estado actual: {sale.Status}");
            }

            if (!sale.IsPaid)
            {
                throw new Exception("La venta debe estar pagada para facturar");
            }

            if (sale.Company == null)
            {
                throw new Exception("La venta no tiene empresa emisora asignada");
            }

            if (sale.Customer == null)
            {
                throw new Exception("La venta no tiene cliente asignado");
            }

            Console.WriteLine($"   ✓ Venta validada");
            Console.WriteLine($"      Empresa: {sale.Company.LegalName}");
            Console.WriteLine($"      Cliente: {sale.Customer.Name}");
            Console.WriteLine($"      Total: ${sale.Total:N2}");

            // Determinar Serie y Folio
            var serie = request.Serie ?? sale.Company.InvoiceSeries ?? "A";
            var folio = await _invoiceRepository.GetNextFolioAsync(sale.Company.Id, serie);

            // Crear Invoice
            var invoice = new Invoice
            {
                // Referencia
                SaleId = sale.Id,

                // Comprobante
                Serie = serie,
                Folio = folio,
                InvoiceDate = DateTime.UtcNow,
                FormaPago = request.FormaPago,
                MetodoPago = request.MetodoPago,
                CondicionesDePago = request.CondicionesDePago,
                TipoDeComprobante = request.TipoDeComprobante,
                LugarExpedicion = sale.Company.FiscalZipCode ?? string.Empty,

                // Emisor
                CompanyId = sale.Company.Id,
                EmisorRfc = sale.Company.TaxId ?? string.Empty,
                EmisorNombre = sale.Company.LegalName ?? string.Empty,
                EmisorRegimenFiscal = sale.Company.SatTaxRegime,

                // Receptor (priorizar datos de request.Receptor si están presentes)
                CustomerId = request.Receptor?.CustomerId ?? sale.Customer.ID,
                ReceptorRfc = request.Receptor?.Rfc ?? sale.Customer.TaxId ?? "XAXX010101000",
                ReceptorNombre = request.Receptor?.Nombre ?? sale.Customer.Name ?? string.Empty,
                ReceptorDomicilioFiscal = request.Receptor?.DomicilioFiscal ?? sale.Customer.ZipCode,
                ReceptorRegimenFiscal = request.Receptor?.RegimenFiscal ?? sale.Customer.SatTaxRegime,
                ReceptorUsoCfdi = request.Receptor?.UsoCfdi ?? request.UsoCfdi ?? sale.Customer.SatCfdiUse ?? "G03",

                // Montos
                SubTotal = sale.SubTotal,
                DiscountAmount = sale.DiscountAmount,
                TaxAmount = sale.TaxAmount,
                Total = sale.Total,
                Moneda = "MXN",
                TipoCambio = 1.0m,

                // Status
                Status = "Borrador",

                // Audit
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId,

                // Details
                Details = new List<InvoiceDetail>()
            };

            // Crear InvoiceDetails desde SaleDetails
            foreach (var saleDetail in sale.Details ?? new List<Domain.Entities.SaleDetail>())
            {
                var invoiceDetail = new InvoiceDetail
                {
                    ProductId = saleDetail.ProductId,
                    ClaveProdServ = saleDetail.Product?.SatCode ?? "01010101",
                    NoIdentificacion = saleDetail.ProductCode,
                    Cantidad = saleDetail.Quantity,
                    ClaveUnidad = saleDetail.Product?.SatUnit ?? "H87",
                    Unidad = saleDetail.Product?.Unit,
                    Descripcion = saleDetail.ProductName ?? string.Empty,
                    ValorUnitario = saleDetail.UnitPrice,
                    Importe = saleDetail.Quantity * saleDetail.UnitPrice,
                    Descuento = saleDetail.DiscountAmount,
                    ObjetoImp = "02", // Sí objeto de impuesto

                    // Traslados (IVA)
                    TieneTraslados = saleDetail.TaxAmount > 0,
                    TrasladoBase = saleDetail.TaxAmount > 0 ? (saleDetail.Quantity * saleDetail.UnitPrice - saleDetail.DiscountAmount) : null,
                    TrasladoImpuesto = saleDetail.TaxAmount > 0 ? "002" : null, // 002 = IVA
                    TrasladoTipoFactor = saleDetail.TaxAmount > 0 ? "Tasa" : null,
                    TrasladoTasaOCuota = saleDetail.TaxAmount > 0 ? 0.160000m : null,
                    TrasladoImporte = saleDetail.TaxAmount,

                    // Retenciones (ninguna en este caso)
                    TieneRetenciones = false,

                    SaleDetailId = saleDetail.Id,
                    CreatedAt = DateTime.UtcNow
                };

                invoice.Details.Add(invoiceDetail);
            }

            Console.WriteLine($"   ✓ Factura construida con {invoice.Details.Count} conceptos");

            return invoice;
        }

        /// <summary>
        /// Crea una factura manual (sin venta origen)
        /// </summary>
        private async Task<Invoice> CreateManualInvoiceAsync(
            CreateInvoiceRequestDto request,
            int userId)
        {
            if (request.Receptor == null)
            {
                throw new Exception("Debe proporcionar datos del receptor para factura manual");
            }

            if (request.Details == null || !request.Details.Any())
            {
                throw new Exception("Debe proporcionar al menos un concepto para factura manual");
            }

            // TODO: Obtener CompanyId del usuario
            int companyId = 1; // Por ahora hardcoded, debe obtenerse del contexto del usuario

            // Determinar Serie y Folio
            var serie = request.Serie ?? "A";
            var folio = await _invoiceRepository.GetNextFolioAsync(companyId, serie);

            // Calcular montos
            decimal subTotal = 0;
            decimal descuentoTotal = 0;
            decimal impuestosTotal = 0;

            foreach (var detail in request.Details)
            {
                var importe = detail.Cantidad * detail.ValorUnitario;
                subTotal += importe;
                descuentoTotal += detail.Descuento;

                if (detail.Impuestos?.Traslados != null)
                {
                    foreach (var traslado in detail.Impuestos.Traslados)
                    {
                        impuestosTotal += traslado.Base * traslado.TasaOCuota;
                    }
                }
            }

            var total = subTotal - descuentoTotal + impuestosTotal;

            // Crear Invoice
            var invoice = new Invoice
            {
                // Sin referencia a venta
                SaleId = null,

                // Comprobante
                Serie = serie,
                Folio = folio,
                InvoiceDate = DateTime.UtcNow,
                FormaPago = request.FormaPago,
                MetodoPago = request.MetodoPago,
                CondicionesDePago = request.CondicionesDePago,
                TipoDeComprobante = request.TipoDeComprobante,
                LugarExpedicion = "00000", // TODO: Obtener del Company

                // Emisor (TODO: cargar desde Company)
                CompanyId = companyId,
                EmisorRfc = "EKU9003173C9", // TODO: Obtener del Company
                EmisorNombre = "ESCUELA KEMPER URGATE", // TODO: Obtener del Company
                EmisorRegimenFiscal = "601", // TODO: Obtener del Company

                // Receptor
                CustomerId = request.Receptor.CustomerId,
                ReceptorRfc = request.Receptor.Rfc,
                ReceptorNombre = request.Receptor.Nombre,
                ReceptorDomicilioFiscal = request.Receptor.DomicilioFiscal,
                ReceptorRegimenFiscal = request.Receptor.RegimenFiscal,
                ReceptorUsoCfdi = request.Receptor.UsoCfdi,

                // Montos
                SubTotal = subTotal,
                DiscountAmount = descuentoTotal,
                TaxAmount = impuestosTotal,
                Total = total,
                Moneda = "MXN",
                TipoCambio = 1.0m,

                // Status
                Status = "Borrador",

                // Audit
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId,

                // Details
                Details = new List<InvoiceDetail>()
            };

            // Crear InvoiceDetails desde request
            foreach (var detailDto in request.Details)
            {
                var importe = detailDto.Cantidad * detailDto.ValorUnitario;

                var invoiceDetail = new InvoiceDetail
                {
                    ProductId = detailDto.ProductId,
                    ClaveProdServ = detailDto.ClaveProdServ,
                    NoIdentificacion = detailDto.NoIdentificacion,
                    Cantidad = detailDto.Cantidad,
                    ClaveUnidad = detailDto.ClaveUnidad,
                    Unidad = detailDto.Unidad,
                    Descripcion = detailDto.Descripcion,
                    ValorUnitario = detailDto.ValorUnitario,
                    Importe = importe,
                    Descuento = detailDto.Descuento,
                    ObjetoImp = detailDto.ObjetoImp,

                    // Traslados
                    TieneTraslados = detailDto.Impuestos?.Traslados?.Any() ?? false,
                    TrasladoBase = detailDto.Impuestos?.Traslados?.FirstOrDefault()?.Base,
                    TrasladoImpuesto = detailDto.Impuestos?.Traslados?.FirstOrDefault()?.Impuesto,
                    TrasladoTipoFactor = detailDto.Impuestos?.Traslados?.FirstOrDefault()?.TipoFactor,
                    TrasladoTasaOCuota = detailDto.Impuestos?.Traslados?.FirstOrDefault()?.TasaOCuota,
                    TrasladoImporte = detailDto.Impuestos?.Traslados?.FirstOrDefault() != null
                        ? detailDto.Impuestos.Traslados.First().Base * detailDto.Impuestos.Traslados.First().TasaOCuota
                        : null,

                    // Retenciones
                    TieneRetenciones = detailDto.Impuestos?.Retenciones?.Any() ?? false,
                    RetencionBase = detailDto.Impuestos?.Retenciones?.FirstOrDefault()?.Base,
                    RetencionImpuesto = detailDto.Impuestos?.Retenciones?.FirstOrDefault()?.Impuesto,
                    RetencionTipoFactor = detailDto.Impuestos?.Retenciones?.FirstOrDefault()?.TipoFactor,
                    RetencionTasaOCuota = detailDto.Impuestos?.Retenciones?.FirstOrDefault()?.TasaOCuota,
                    RetencionImporte = detailDto.Impuestos?.Retenciones?.FirstOrDefault() != null
                        ? detailDto.Impuestos.Retenciones.First().Base * detailDto.Impuestos.Retenciones.First().TasaOCuota
                        : null,

                    CreatedAt = DateTime.UtcNow
                };

                invoice.Details.Add(invoiceDetail);
            }

            Console.WriteLine($"   ✓ Factura manual construida con {invoice.Details.Count} conceptos");

            return invoice;
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

            var impuestos = new
            {
                TotalImpuestosTrasladados = totalTraslados > 0 ? totalTraslados : (decimal?)null,
                TotalImpuestosRetenidos = totalRetenciones > 0 ? totalRetenciones : (decimal?)null,

                Traslados = invoice.Details
                    .Where(d => d.TieneTraslados)
                    .GroupBy(d => new { d.TrasladoImpuesto, d.TrasladoTipoFactor, d.TrasladoTasaOCuota })
                    .Select(g => new
                    {
                        Base = g.Sum(d => d.TrasladoBase),
                        Impuesto = g.Key.TrasladoImpuesto,
                        TipoFactor = g.Key.TrasladoTipoFactor,
                        TasaOCuota = g.Key.TrasladoTasaOCuota,
                        Importe = g.Sum(d => d.TrasladoImporte)
                    })
                    .ToList(),

                Retenciones = invoice.Details
                    .Where(d => d.TieneRetenciones)
                    .GroupBy(d => new { d.RetencionImpuesto })
                    .Select(g => new
                    {
                        Impuesto = g.Key.RetencionImpuesto,
                        Importe = g.Sum(d => d.RetencionImporte)
                    })
                    .ToList()
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
