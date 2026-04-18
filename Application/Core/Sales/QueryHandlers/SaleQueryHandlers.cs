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
                throw new KeyNotFoundException($"Venta con ID {request.SaleId} no encontrada");

            return SaleMapper.ToResponseDto(sale);
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

            // Obtener estad�sticas
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
                    BranchName = s.Branch?.Name ?? s.Warehouse.Branch?.Name,                      // ? NUEVO
                    CompanyName = s.Company?.LegalName ?? s.Warehouse.Branch?.Company?.LegalName, // ? NUEVO
                    Total = s.Total,
                    Status = s.Status,
                    IsPaid = s.IsPaid,
                    RequiresInvoice = s.RequiresInvoice,
                    SaleType = s.SaleType,
                    DeliveryAddress = s.DeliveryAddress,
                    ScheduledDeliveryDate = s.ScheduledDeliveryDate,
                    DeliveredAt = s.DeliveredAt,
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
    /// Handler para obtener estad�sticas de ventas
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
