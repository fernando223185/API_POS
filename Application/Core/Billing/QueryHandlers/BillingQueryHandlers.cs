using Application.Abstractions.Sales;
using Application.Core.Billing.Documents;
using Application.Core.Billing.Queries;
using Application.DTOs.Billing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Core.Billing.QueryHandlers
{
    /// <summary>
    /// Handler para obtener ventas pendientes de timbrar con filtros avanzados
    /// </summary>
    public class GetPendingInvoiceSalesQueryHandler : IRequestHandler<GetPendingInvoiceSalesQuery, PendingInvoiceSalesResponseDto>
    {
        private readonly ISaleRepository _saleRepository;

        public GetPendingInvoiceSalesQueryHandler(ISaleRepository saleRepository)
        {
            _saleRepository = saleRepository;
        }

        public async Task<PendingInvoiceSalesResponseDto> Handle(
            GetPendingInvoiceSalesQuery request, 
            CancellationToken cancellationToken)
        {
            Console.WriteLine($"?? Obteniendo ventas pendientes de timbrar - P�gina: {request.Page}, Tama�o: {request.PageSize}");

            // Construir filtros
            var fromDate = request.FromDate ?? DateTime.UtcNow.Date.AddDays(-30);
            var toDate = request.ToDate ?? DateTime.UtcNow.Date.AddDays(1);

            // Obtener ventas con filtros
            var (sales, totalCount) = await _saleRepository.GetPendingInvoiceSalesAsync(
                request.Page,
                request.PageSize,
                request.OnlyRequiresInvoice,
                request.WarehouseId,
                request.BranchId,
                request.CompanyId,
                fromDate,
                toDate
            );

            var salesList = sales.ToList();

            // Calcular estad�sticas
            var salesRequiresInvoice = salesList.Count(s => s.RequiresInvoice);
            var salesNotRequiresInvoice = salesList.Count(s => !s.RequiresInvoice);
            var totalAmount = salesList.Sum(s => s.Total);
            var avgAmount = salesList.Any() ? totalAmount / salesList.Count : 0;
            var avgDaysPending = salesList.Any() 
                ? (int)salesList.Average(s => (DateTime.UtcNow - s.CreatedAt).TotalDays)
                : 0;

            var result = new PendingInvoiceSalesResponseDto
            {
                Message = "Ventas pendientes de timbrar obtenidas exitosamente",
                Error = 0,
                Data = salesList.Select(s => new PendingInvoiceSaleDto
                {
                    Id = s.Id,
                    Code = s.Code,
                    SaleDate = s.SaleDate,
                    CustomerId = s.CustomerId,
                    CustomerName = s.CustomerName ?? "Público General",
                    CustomerRfc = s.Customer?.TaxId, // CORREGIDO: TaxId es el RFC
                    CustomerEmail = s.Customer?.Email,
                    WarehouseId = s.WarehouseId,
                    WarehouseName = s.Warehouse?.Name,
                    BranchId = s.BranchId,
                    BranchName = s.Branch?.Name,
                    CompanyId = s.CompanyId,
                    CompanyName = s.Company?.LegalName,
                    SubTotal = s.SubTotal,
                    TaxAmount = s.TaxAmount,
                    Total = s.Total,
                    RequiresInvoice = s.RequiresInvoice,
                    IsPaid = s.IsPaid,
                    Status = s.Status,
                    CreatedAt = s.CreatedAt,
                    DaysPending = (int)(DateTime.UtcNow - s.CreatedAt).TotalDays,
                    Notes = s.Notes
                }).ToList(),
                Page = request.Page,
                PageSize = request.PageSize,
                TotalRecords = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                Summary = new PendingInvoiceSummaryDto
                {
                    TotalSales = totalCount,
                    SalesRequiresInvoice = salesRequiresInvoice,
                    SalesNotRequiresInvoice = salesNotRequiresInvoice,
                    TotalAmount = totalAmount,
                    AverageAmount = avgAmount,
                    AverageDaysPending = avgDaysPending
                }
            };

            Console.WriteLine($"? Obtenidas {salesList.Count} ventas de {totalCount} totales");

            return result;
        }
    }

    /// <summary>
    /// Handler para obtener una venta individual completa para facturaci�n
    /// Incluye todos los datos necesarios para generar CFDI
    /// </summary>
    public class GetSaleForInvoicingQueryHandler : IRequestHandler<GetSaleForInvoicingQuery, SaleForInvoicingDto>
    {
        private readonly ISaleRepository _saleRepository;

        public GetSaleForInvoicingQueryHandler(ISaleRepository saleRepository)
        {
            _saleRepository = saleRepository;
        }

        public async Task<SaleForInvoicingDto> Handle(
            GetSaleForInvoicingQuery request,
            CancellationToken cancellationToken)
        {
            Console.WriteLine($"?? Obteniendo venta {request.SaleId} para facturaci�n...");

            var sale = await _saleRepository.GetSaleForInvoicingAsync(request.SaleId);

            if (sale == null)
            {
                throw new KeyNotFoundException($"Venta con ID {request.SaleId} no encontrada");
            }

            // Validar que la venta est� completada y pagada
            if (sale.Status != "Completed")
            {
                throw new InvalidOperationException($"La venta debe estar completada para facturar. Estado actual: {sale.Status}");
            }

            if (!sale.IsPaid)
            {
                throw new InvalidOperationException("La venta debe estar pagada para facturar");
            }

            // Validar que tenga empresa asignada
            if (sale.CompanyId == null || sale.Company == null)
            {
                throw new InvalidOperationException("La venta debe tener una empresa emisora asignada");
            }

            var result = new SaleForInvoicingDto
            {
                Id = sale.Id,
                Code = sale.Code,
                SaleDate = sale.SaleDate,

                // Empresa emisora
                Company = new CompanyForInvoicingDto
                {
                    Id = sale.Company.Id,
                    LegalName = sale.Company.LegalName,
                    Rfc = sale.Company.TaxId, // CORREGIDO: TaxId es el RFC
                    FiscalRegime = sale.Company.SatTaxRegime, // CORREGIDO: SatTaxRegime
                    PostalCode = sale.Company.FiscalZipCode, // CORREGIDO: FiscalZipCode
                    Email = sale.Company.Email,
                    Serie = sale.Company.InvoiceSeries ?? "A", // CORREGIDO: InvoiceSeries
                    NextFolio = sale.Company.InvoiceCurrentFolio  // CORREGIDO: InvoiceCurrentFolio
                },

                // Sucursal
                Branch = sale.Branch != null ? new BranchForInvoicingDto
                {
                    Id = sale.Branch.Id,
                    Name = sale.Branch.Name,
                    Address = sale.Branch.Address,
                    Phone = sale.Branch.Phone
                } : null,

                // Cliente receptor
                Customer = new CustomerForInvoicingDto
                {
                    Id = sale.CustomerId,
                    Name = sale.CustomerName ?? "PUBLICO EN GENERAL",
                    Rfc = sale.Customer?.TaxId ?? "XAXX010101000", // CORREGIDO: TaxId
                    FiscalRegime = sale.Customer?.SatTaxRegime, // CORREGIDO: SatTaxRegime
                    PostalCode = sale.Customer?.ZipCode ?? "00000", // CORREGIDO: ZipCode
                    Email = sale.Customer?.Email,
                    Address = sale.Customer?.Address,
                    CfdiUse = sale.Customer?.SatCfdiUse ?? "G03" // CORREGIDO: SatCfdiUse (Gastos en general)
                },

                // Montos
                SubTotal = sale.SubTotal,
                DiscountAmount = sale.DiscountAmount,
                DiscountPercentage = sale.DiscountPercentage,
                TaxAmount = sale.TaxAmount,
                Total = sale.Total,

                // Estado
                IsPaid = sale.IsPaid,
                RequiresInvoice = sale.RequiresInvoice,
                Status = sale.Status,
                InvoiceUuid = sale.InvoiceUuid,

                // Detalles
                Details = sale.Details.Select(d => new SaleDetailForInvoicingDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductCode = d.ProductCode,
                    ProductName = d.ProductName,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    DiscountAmount = d.DiscountAmount,
                    TaxAmount = d.TaxAmount,
                    SubTotal = d.SubTotal,
                    Total = d.Total,
                    SatProductKey = "01010101", // TODO: Obtener de Product cuando est� disponible
                    SatUnitKey = "H87", // TODO: Obtener de Product cuando est� disponible
                    Notes = d.Notes
                }).ToList()

                // Formas de pago
                , Payments = sale.Payments.Select(p => new PaymentMethodForInvoicingDto
                {
                    Id = p.Id,
                    PaymentMethod = p.PaymentMethod,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    CardNumber = p.CardNumber,
                    TransactionReference = p.TransactionReference,
                    BankName = p.BankName
                }).ToList(),

                Notes = sale.Notes,
                CreatedAt = sale.CreatedAt
            };

            Console.WriteLine($"? Venta {sale.Code} lista para facturaci�n");
            Console.WriteLine($"   Empresa: {result.Company.LegalName} (RFC: {result.Company.Rfc})");
            Console.WriteLine($"   Cliente: {result.Customer.Name} (RFC: {result.Customer.Rfc})");
            Console.WriteLine($"   Total: ${result.Total:N2} ({result.Details.Count} productos)");

            return result;
        }
    }

    /// <summary>
    /// Handler para obtener facturas con filtros opcionales (para dashboard/listados)
    /// </summary>
    public class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, InvoicesPagedResponseDto>
    {
        private readonly Application.Abstractions.Billing.IInvoiceRepository _invoiceRepository;

        public GetInvoicesQueryHandler(Application.Abstractions.Billing.IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<InvoicesPagedResponseDto> Handle(
            GetInvoicesQuery request,
            CancellationToken cancellationToken)
        {
            Console.WriteLine($"📋 Obteniendo facturas - Página: {request.Page}, Tamaño: {request.PageSize}");
            if (request.Status != null)
                Console.WriteLine($"   Filtro Status: {request.Status}");
            if (request.CompanyId.HasValue)
                Console.WriteLine($"   Filtro CompanyId: {request.CompanyId}");

            // Obtener facturas desde el repositorio
            var (invoices, totalCount) = await _invoiceRepository.GetPagedAsync(
                request.Page,
                request.PageSize,
                request.CompanyId,
                request.CustomerId,
                request.Status,
                request.FromDate,
                request.ToDate,
                request.Serie,
                request.Rfc
            );

            var invoiceList = invoices.ToList();

            // Mapear a DTOs
            var data = invoiceList.Select(i => new InvoiceListItemDto
            {
                Id = i.Id,
                Serie = i.Serie,
                Folio = i.Folio,
                InvoiceDate = i.InvoiceDate,
                Status = i.Status,
                Uuid = i.Uuid,
                EmisorRfc = i.EmisorRfc,
                EmisorNombre = i.EmisorNombre,
                ReceptorRfc = i.ReceptorRfc,
                ReceptorNombre = i.ReceptorNombre,
                Total = i.Total,
                Moneda = i.Moneda,
                TimbradoAt = i.TimbradoAt,
                CancelledAt = i.CancelledAt,
                SaleId = i.SaleId,
                SaleCode = i.Sale?.Code,
                CreatedAt = i.CreatedAt
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            Console.WriteLine($"✓ {invoiceList.Count} facturas encontradas de {totalCount} totales");

            return new InvoicesPagedResponseDto
            {
                Message = "Facturas obtenidas exitosamente",
                Error = 0,
                Data = data,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalRecords = totalCount,
                TotalPages = totalPages
            };
        }
    }

    /// <summary>
    /// Handler para obtener una factura por ID con todos sus detalles
    /// </summary>
    public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceResponseDto>
    {
        private readonly Application.Abstractions.Billing.IInvoiceRepository _invoiceRepository;

        public GetInvoiceByIdQueryHandler(Application.Abstractions.Billing.IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<InvoiceResponseDto> Handle(
            GetInvoiceByIdQuery request,
            CancellationToken cancellationToken)
        {
            Console.WriteLine($"📄 Obteniendo factura con ID: {request.InvoiceId}");

            var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId);

            if (invoice == null)
            {
                throw new KeyNotFoundException($"Factura con ID {request.InvoiceId} no encontrada");
            }

            // Mapear a DTO completo
            var invoiceDto = new InvoiceDto
            {
                Id = invoice.Id,
                
                // Referencia
                SaleId = invoice.SaleId,
                SaleCode = invoice.Sale?.Code,
                
                // Comprobante
                Serie = invoice.Serie,
                Folio = invoice.Folio,
                InvoiceDate = invoice.InvoiceDate,
                FormaPago = invoice.FormaPago,
                MetodoPago = invoice.MetodoPago,
                CondicionesDePago = invoice.CondicionesDePago,
                TipoDeComprobante = invoice.TipoDeComprobante,
                LugarExpedicion = invoice.LugarExpedicion,
                
                // Emisor
                CompanyId = invoice.CompanyId,
                EmisorRfc = invoice.EmisorRfc,
                EmisorNombre = invoice.EmisorNombre,
                EmisorRegimenFiscal = invoice.EmisorRegimenFiscal,
                
                // Receptor
                CustomerId = invoice.CustomerId,
                ReceptorRfc = invoice.ReceptorRfc,
                ReceptorNombre = invoice.ReceptorNombre,
                ReceptorDomicilioFiscal = invoice.ReceptorDomicilioFiscal,
                ReceptorRegimenFiscal = invoice.ReceptorRegimenFiscal,
                ReceptorUsoCfdi = invoice.ReceptorUsoCfdi,
                
                // Montos
                SubTotal = invoice.SubTotal,
                DiscountAmount = invoice.DiscountAmount,
                TaxAmount = invoice.TaxAmount,
                Total = invoice.Total,
                Moneda = invoice.Moneda,
                TipoCambio = invoice.TipoCambio,
                
                // Estado
                Status = invoice.Status,
                Uuid = invoice.Uuid,
                TimbradoAt = invoice.TimbradoAt,
                
                // Timbrado (solo si Status = "Timbrada")
                XmlCfdi = invoice.Status == "Timbrada" ? invoice.XmlCfdi : null,
                CadenaOriginalSat = invoice.Status == "Timbrada" ? invoice.CadenaOriginalSat : null,
                SelloCfdi = invoice.Status == "Timbrada" ? invoice.SelloCfdi : null,
                SelloSat = invoice.Status == "Timbrada" ? invoice.SelloSat : null,
                NoCertificadoCfdi = invoice.Status == "Timbrada" ? invoice.NoCertificadoCfdi : null,
                NoCertificadoSat = invoice.Status == "Timbrada" ? invoice.NoCertificadoSat : null,
                QrCode = invoice.Status == "Timbrada" ? invoice.QrCode : null,
                
                // Cancelación (solo si Status = "Cancelada")
                CancelledAt = invoice.CancelledAt,
                CancellationReason = invoice.CancellationReason,
                CancelledByUserId = invoice.CancelledByUserId,
                CancelledByUserName = invoice.CancelledBy?.Name,
                
                // Detalles
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
                    
                    // Impuestos - Traslados
                    TieneTraslados = d.TieneTraslados,
                    TrasladoBase = d.TrasladoBase,
                    TrasladoImpuesto = d.TrasladoImpuesto,
                    TrasladoTipoFactor = d.TrasladoTipoFactor,
                    TrasladoTasaOCuota = d.TrasladoTasaOCuota,
                    TrasladoImporte = d.TrasladoImporte,
                    
                    // Impuestos - Retenciones
                    TieneRetenciones = d.TieneRetenciones,
                    RetencionBase = d.RetencionBase,
                    RetencionImpuesto = d.RetencionImpuesto,
                    RetencionTipoFactor = d.RetencionTipoFactor,
                    RetencionTasaOCuota = d.RetencionTasaOCuota,
                    RetencionImporte = d.RetencionImporte,
                    
                    Notes = d.Notes
                }).ToList(),
                
                // Audit
                Notes = invoice.Notes,
                CreatedAt = invoice.CreatedAt,
                CreatedByUserId = invoice.CreatedByUserId,
                CreatedByUserName = invoice.CreatedBy?.Name,
                UpdatedAt = invoice.UpdatedAt
            };

            Console.WriteLine($"✓ Factura {invoice.Serie}-{invoice.Folio} obtenida exitosamente");
            Console.WriteLine($"   Estado: {invoice.Status}");
            Console.WriteLine($"   Detalles: {invoice.Details.Count} conceptos");
            Console.WriteLine($"   Total: ${invoice.Total:N2} {invoice.Moneda}");

            return new InvoiceResponseDto
            {
                Message = "Factura obtenida exitosamente",
                Error = 0,
                Data = invoiceDto
            };
        }
    }

    // ============================================================
    // XML
    // ============================================================
    public class GetInvoiceXmlQueryHandler : IRequestHandler<GetInvoiceXmlQuery, (byte[] Bytes, string FileName)>
    {
        private readonly Application.Abstractions.Billing.IInvoiceRepository _invoiceRepository;

        public GetInvoiceXmlQueryHandler(Application.Abstractions.Billing.IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<(byte[] Bytes, string FileName)> Handle(
            GetInvoiceXmlQuery request,
            CancellationToken cancellationToken)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId);

            if (invoice == null)
                throw new KeyNotFoundException($"Factura con ID {request.InvoiceId} no encontrada");

            if (invoice.Status != "Timbrada" || string.IsNullOrEmpty(invoice.XmlCfdi))
                throw new InvalidOperationException("La factura no tiene XML timbrado disponible");

            var bytes = System.Text.Encoding.UTF8.GetBytes(invoice.XmlCfdi);
            var fileName = $"{invoice.Serie}-{invoice.Folio}_{invoice.Uuid}.xml";
            return (bytes, fileName);
        }
    }

    // ============================================================
    // PDF
    // ============================================================
    public class GetInvoicePdfQueryHandler : IRequestHandler<GetInvoicePdfQuery, (byte[] Bytes, string FileName)>
    {
        private readonly Application.Abstractions.Billing.IInvoiceRepository _invoiceRepository;
        private readonly Application.Abstractions.Reports.IReportTemplateRepository _templateRepository;

        public GetInvoicePdfQueryHandler(
            Application.Abstractions.Billing.IInvoiceRepository invoiceRepository,
            Application.Abstractions.Reports.IReportTemplateRepository templateRepository)
        {
            _invoiceRepository = invoiceRepository;
            _templateRepository = templateRepository;
        }

        public async Task<(byte[] Bytes, string FileName)> Handle(
            GetInvoicePdfQuery request,
            CancellationToken cancellationToken)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId)
                ?? throw new KeyNotFoundException($"Factura con ID {request.InvoiceId} no encontrada");

            if (invoice.Status != "Timbrada")
                throw new InvalidOperationException("Solo se puede generar PDF de facturas timbradas");

            var template = await _templateRepository.GetDefaultByTypeAsync("Invoice", invoice.CompanyId)
                ?? await _templateRepository.GetDefaultByTypeAsync("Invoice", null)
                ?? throw new InvalidOperationException("No hay una plantilla por defecto para Facturas. Crea una desde el Report Builder.");

            var sections = Reports.QueryHandlers.GetReportTemplateByIdQueryHandler
                .EnsureInvoiceVisualSections(
                    Reports.QueryHandlers.GetReportTemplateByIdQueryHandler
                        .DeserializeSections(template.SectionsJson));

            var bytes = Reports.Engine.ReportPdfEngine.Generate(
                sections,
                new() { Reports.Engine.ReportDataProvider.FromInvoice(invoice) },
                Reports.Engine.ReportDataProvider.FromInvoiceDetails(invoice),
                template.Name);

            var fileName = $"{invoice.Serie}-{invoice.Folio}_{invoice.Uuid}.pdf";
            return (bytes, fileName);
        }
    }

    // ============================================================
    // RESUMEN DE FACTURACIÓN
    // ============================================================
    public class GetBillingSummaryQueryHandler : IRequestHandler<GetBillingSummaryQuery, BillingSummaryResponseDto>
    {
        private readonly Application.Abstractions.Billing.IInvoiceRepository _invoiceRepository;

        public GetBillingSummaryQueryHandler(Application.Abstractions.Billing.IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<BillingSummaryResponseDto> Handle(
            GetBillingSummaryQuery request,
            CancellationToken cancellationToken)
        {
            Console.WriteLine($"📊 Obteniendo resumen de facturación - Año: {request.Year}, Mes: {request.Month}");

            var (totalInvoices, totalAmount, stampedInvoices, pendingInvoices, cancelledInvoices) = 
                await _invoiceRepository.GetSummaryAsync(request.Year, request.Month);

            var averageInvoiceAmount = totalInvoices > 0 ? totalAmount / totalInvoices : 0;

            var result = new BillingSummaryResponseDto
            {
                Message = "Billing summary retrieved successfully",
                Error = 0,
                Period = $"{request.Year}-{request.Month:D2}",
                Data = new BillingSummaryDataDto
                {
                    TotalInvoices = totalInvoices,
                    TotalAmount = totalAmount,
                    StampedInvoices = stampedInvoices,
                    PendingInvoices = pendingInvoices,
                    CancelledInvoices = cancelledInvoices,
                    AverageInvoiceAmount = averageInvoiceAmount
                }
            };

            Console.WriteLine($"✓ Resumen obtenido - Total: {totalInvoices} facturas, Monto: ${totalAmount:N2}");

            return result;
        }
    }
}
