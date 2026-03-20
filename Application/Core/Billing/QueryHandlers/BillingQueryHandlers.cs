using Application.Abstractions.Sales;
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
                    CustomerName = s.CustomerName ?? "P�blico General",
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
}
