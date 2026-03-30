using Application.DTOs.Inventory;
using MediatR;

namespace Application.Core.Inventory.Queries
{
    public class GetProductStockQuery : IRequest<PagedProductStockResponseDto>
    {
        public int? ProductId { get; set; }
        public string? ProductSearch { get; set; }
        public int? WarehouseId { get; set; }
        public int? CompanyId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class GetProductStockByWarehouseQuery : IRequest<List<ProductStockDto>>
    {
        public int WarehouseId { get; set; }

        public GetProductStockByWarehouseQuery(int warehouseId) => WarehouseId = warehouseId;
    }
}
