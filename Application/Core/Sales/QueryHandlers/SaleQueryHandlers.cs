using Application.Abstractions.Sales;
using Application.Core.Sales.Queries;
using Application.DTOs.Sales;
using MediatR;

namespace Application.Core.Sales.QueryHandlers
{
    /// <summary>
    /// Handler para obtener una venta por ID
    /// </summary>
    public class GetSaleByIdQueryHandler : IRequestHandler<GetSaleByIdQuery, SaleResponseDto>
    {
        private readonly ISaleRepository _saleRepository;

        public GetSaleByIdQueryHandler(ISaleRepository saleRepository)
        {
            _saleRepository = saleRepository;
        }

        public async Task<SaleResponseDto> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
        {
            var sale = await _saleRepository.GetByIdAsync(request.SaleId);

            if (sale == null)
            {
                throw new KeyNotFoundException($"Venta con ID {request.SaleId} no encontrada");
            }

            var totalCost = sale.Details.Sum(d => d.TotalCost ?? 0);
            var grossProfit = sale.Total - totalCost;
            var profitMargin = sale.Total > 0 ? (grossProfit / sale.Total) * 100 : 0;

            return new SaleResponseDto
            {
                Id = sale.Id,
                Code = sale.Code,
                SaleDate = sale.SaleDate,
                CustomerId = sale.CustomerId,
                CustomerName = sale.CustomerName,
                WarehouseId = sale.WarehouseId,
                WarehouseName = sale.Warehouse.Name,
                BranchName = sale.Warehouse.Branch?.Name,
                UserId = sale.UserId,
                UserName = sale.User.Name,
                PriceListId = sale.PriceListId,
                PriceListName = sale.PriceList?.Name,
                SubTotal = sale.SubTotal,
                DiscountAmount = sale.DiscountAmount,
                DiscountPercentage = sale.DiscountPercentage,
                TaxAmount = sale.TaxAmount,
                Total = sale.Total,
                AmountPaid = sale.AmountPaid,
                ChangeAmount = sale.ChangeAmount,
                IsPaid = sale.IsPaid,
                Status = sale.Status,
                IsPostedToInventory = sale.IsPostedToInventory,
                PostedToInventoryDate = sale.PostedToInventoryDate,
                RequiresInvoice = sale.RequiresInvoice,
                InvoiceUuid = sale.InvoiceUuid,
                Notes = sale.Notes,
                CreatedAt = sale.CreatedAt,
                CreatedByName = sale.CreatedBy?.Name,
                CancelledAt = sale.CancelledAt,
                CancellationReason = sale.CancellationReason,
                Details = sale.Details.Select(d => new SaleDetailResponseDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductCode = d.ProductCode,
                    ProductName = d.ProductName,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    DiscountPercentage = d.DiscountPercentage,
                    DiscountAmount = d.DiscountAmount,
                    TaxPercentage = d.TaxPercentage,
                    TaxAmount = d.TaxAmount,
                    SubTotal = d.SubTotal,
                    Total = d.Total,
                    UnitCost = d.UnitCost,
                    TotalCost = d.TotalCost,
                    LineProfit = d.Total - (d.TotalCost ?? 0),
                    Notes = d.Notes,
                    SerialNumber = d.SerialNumber,
                    LotNumber = d.LotNumber
                }).ToList(),
                Payments = sale.Payments.Select(p => new SalePaymentResponseDto
                {
                    Id = p.Id,
                    PaymentMethod = p.PaymentMethod,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    CardNumber = p.CardNumber,
                    CardType = p.CardType,
                    AuthorizationCode = p.AuthorizationCode,
                    TransactionReference = p.TransactionReference,
                    TerminalId = p.TerminalId,
                    BankName = p.BankName,
                    TransferReference = p.TransferReference,
                    CheckNumber = p.CheckNumber,
                    CheckBank = p.CheckBank,
                    Status = p.Status,
                    Notes = p.Notes
                }).ToList(),
                TotalItems = sale.Details.Count,
                TotalQuantity = sale.Details.Sum(d => d.Quantity),
                TotalCost = totalCost,
                GrossProfit = grossProfit,
                ProfitMarginPercentage = profitMargin
            };
        }
    }

    /// <summary>
    /// Handler para obtener ventas paginadas
    /// </summary>
    public class GetSalesPagedQueryHandler : IRequestHandler<GetSalesPagedQuery, SalesPagedResponseDto>
    {
        private readonly ISaleRepository _saleRepository;

        public GetSalesPagedQueryHandler(ISaleRepository saleRepository)
        {
            _saleRepository = saleRepository;
        }

        public async Task<SalesPagedResponseDto> Handle(GetSalesPagedQuery request, CancellationToken cancellationToken)
        {
            var (sales, totalCount) = await _saleRepository.GetPagedAsync(
                request.Page,
                request.PageSize,
                request.WarehouseId,
                request.CustomerId,
                request.UserId,
                request.FromDate,
                request.ToDate,
                request.Status,
                request.IsPaid,
                request.RequiresInvoice
            );

            var salesList = sales.ToList();

            // Obtener estadísticas
            var (total, completed, cancelled, draft, totalRevenue, totalCost) = 
                await _saleRepository.GetStatisticsAsync(
                    request.FromDate,
                    request.ToDate,
                    request.WarehouseId
                );

            var totalProfit = totalRevenue - totalCost;
            var avgSaleAmount = completed > 0 ? totalRevenue / completed : 0;
            var avgProfitMargin = totalRevenue > 0 ? (totalProfit / totalRevenue) * 100 : 0;

            return new SalesPagedResponseDto
            {
                Message = "Ventas obtenidas exitosamente",
                Error = 0,
                Data = salesList.Select(s => new SaleSummaryDto
                {
                    Id = s.Id,
                    Code = s.Code,
                    SaleDate = s.SaleDate,
                    CustomerName = s.CustomerName,
                    WarehouseName = s.Warehouse.Name,
                    Total = s.Total,
                    Status = s.Status,
                    IsPaid = s.IsPaid,
                    RequiresInvoice = s.RequiresInvoice,
                    TotalItems = s.Details.Count,
                    UserName = s.User.Name
                }).ToList(),
                Page = request.Page,
                PageSize = request.PageSize,
                TotalRecords = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                Statistics = new SalesStatisticsDto
                {
                    TotalSales = total,
                    CompletedSales = completed,
                    CancelledSales = cancelled,
                    DraftSales = draft,
                    TotalRevenue = totalRevenue,
                    TotalCost = totalCost,
                    TotalProfit = totalProfit,
                    AverageSaleAmount = avgSaleAmount,
                    AverageProfitMargin = avgProfitMargin
                }
            };
        }
    }

    /// <summary>
    /// Handler para obtener estadísticas de ventas
    /// </summary>
    public class GetSalesStatisticsQueryHandler : IRequestHandler<GetSalesStatisticsQuery, SalesStatisticsDto>
    {
        private readonly ISaleRepository _saleRepository;

        public GetSalesStatisticsQueryHandler(ISaleRepository saleRepository)
        {
            _saleRepository = saleRepository;
        }

        public async Task<SalesStatisticsDto> Handle(GetSalesStatisticsQuery request, CancellationToken cancellationToken)
        {
            var (total, completed, cancelled, draft, totalRevenue, totalCost) = 
                await _saleRepository.GetStatisticsAsync(
                    request.FromDate,
                    request.ToDate,
                    request.WarehouseId
                );

            var totalProfit = totalRevenue - totalCost;
            var avgSaleAmount = completed > 0 ? totalRevenue / completed : 0;
            var avgProfitMargin = totalRevenue > 0 ? (totalProfit / totalRevenue) * 100 : 0;

            return new SalesStatisticsDto
            {
                TotalSales = total,
                CompletedSales = completed,
                CancelledSales = cancelled,
                DraftSales = draft,
                TotalRevenue = totalRevenue,
                TotalCost = totalCost,
                TotalProfit = totalProfit,
                AverageSaleAmount = avgSaleAmount,
                AverageProfitMargin = avgProfitMargin
            };
        }
    }
}
