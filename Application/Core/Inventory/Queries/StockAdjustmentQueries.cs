using Application.DTOs.Inventory;
using MediatR;

namespace Application.Core.Inventory.Queries
{
    /// <summary>
    /// Query para obtener listado paginado de ajustes de inventario
    /// </summary>
    public record GetStockAdjustmentsQuery(
        int? WarehouseId,
        string? Reason,
        DateTime? FromDate,
        DateTime? ToDate,
        int? CompanyId,
        int Page,
        int PageSize
    ) : IRequest<PagedStockAdjustmentsResponseDto>;

    /// <summary>
    /// Query para obtener detalle de un ajuste específico
    /// </summary>
    public record GetStockAdjustmentByIdQuery(int Id) : IRequest<StockAdjustmentResponseDto>;
}
