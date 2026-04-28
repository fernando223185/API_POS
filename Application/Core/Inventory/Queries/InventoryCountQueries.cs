using Application.DTOs.Inventory;
using MediatR;

namespace Application.Core.Inventory.Queries
{
    /// <summary>
    /// Query para obtener listado paginado de conteos con filtros
    /// </summary>
    public record GetInventoryCountsQuery(
        int? WarehouseId,
        string? CountType,
        string? Status,
        DateTime? FromDate,
        DateTime? ToDate,
        int? AssignedToUserId,
        int? CompanyId,
        int Page,
        int PageSize
    ) : IRequest<PagedInventoryCountsResponseDto>;

    /// <summary>
    /// Query para obtener el detalle completo de un conteo específico
    /// </summary>
    public record GetInventoryCountByIdQuery(
        int CountId
    ) : IRequest<InventoryCountResponseDto>;

    /// <summary>
    /// Query para obtener productos pendientes de contar en una sesión
    /// </summary>
    public record GetPendingCountDetailsQuery(
        int CountId
    ) : IRequest<List<InventoryCountDetailResponseDto>>;

    /// <summary>
    /// Query para obtener productos con discrepancias (variance != 0)
    /// </summary>
    public record GetCountDetailsWithVarianceQuery(
        int CountId
    ) : IRequest<List<InventoryCountDetailResponseDto>>;

    /// <summary>
    /// Query para obtener estadísticas de un conteo
    /// </summary>
    public record GetCountStatisticsQuery(
        int CountId
    ) : IRequest<CountStatisticsDto>;
}

namespace Application.DTOs.Inventory
{
    /// <summary>
    /// DTO con estadísticas de un conteo
    /// </summary>
    public class CountStatisticsDto
    {
        public int TotalProducts { get; set; }
        public int CountedProducts { get; set; }
        public int PendingProducts { get; set; }
        public decimal ProgressPercentage { get; set; }
        public int ProductsWithPositiveVariance { get; set; }
        public int ProductsWithNegativeVariance { get; set; }
        public int ProductsWithoutVariance { get; set; }
        public decimal TotalPositiveVarianceCost { get; set; }
        public decimal TotalNegativeVarianceCost { get; set; }
        public decimal NetVarianceCost { get; set; }
        public decimal AverageVariancePercentage { get; set; }
        public decimal InventoryAccuracyPercentage { get; set; }
    }
}
