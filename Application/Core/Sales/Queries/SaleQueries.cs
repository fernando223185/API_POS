using Application.DTOs.Sales;
using MediatR;

namespace Application.Core.Sales.Queries
{
    /// <summary>
    /// Query para obtener una venta por ID
    /// </summary>
    public record GetSaleByIdQuery(int SaleId) : IRequest<SaleResponseDto>;

    /// <summary>
    /// Query para obtener ventas paginadas
    /// </summary>
    public record GetSalesPagedQuery(
        int Page = 1,
        int PageSize = 20,
        int? WarehouseId = null,
        int? CustomerId = null,
        int? UserId = null,
        DateTime? FromDate = null,
        DateTime? ToDate = null,
        string? Status = null,
        bool? IsPaid = null,
        bool? RequiresInvoice = null
    ) : IRequest<SalesPagedResponseDto>;

    /// <summary>
    /// Query para obtener estadísticas de ventas
    /// </summary>
    public record GetSalesStatisticsQuery(
        DateTime? FromDate = null,
        DateTime? ToDate = null,
        int? WarehouseId = null
    ) : IRequest<SalesStatisticsDto>;
}
