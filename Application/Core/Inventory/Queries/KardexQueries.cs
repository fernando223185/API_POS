using Application.DTOs.Inventory;
using MediatR;

namespace Application.Core.Inventory.Queries
{
    /// <summary>
    /// Query para obtener el kardex de inventario
    /// </summary>
    public class GetKardexQuery : IRequest<KardexResponseDto>
    {
        public int? ProductId { get; set; }
        public string? ProductSearch { get; set; }
        public int? WarehouseId { get; set; }
        public string? MovementType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public GetKardexQuery(GetKardexRequestDto request)
        {
            ProductId = request.ProductId;
            ProductSearch = request.ProductSearch;
            WarehouseId = request.WarehouseId;
            MovementType = request.MovementType;
            FromDate = request.FromDate;
            ToDate = request.ToDate;
            Page = request.Page;
            PageSize = request.PageSize;
        }
    }

    /// <summary>
    /// Query para obtener estadísticas del kardex
    /// </summary>
    public class GetKardexStatisticsQuery : IRequest<KardexStatisticsDto>
    {
        public int? ProductId { get; set; }
        public int? WarehouseId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public GetKardexStatisticsQuery(int? productId, int? warehouseId, DateTime? fromDate, DateTime? toDate)
        {
            ProductId = productId;
            WarehouseId = warehouseId;
            FromDate = fromDate;
            ToDate = toDate;
        }
    }
}
