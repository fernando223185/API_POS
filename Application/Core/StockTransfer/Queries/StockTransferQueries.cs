using Application.DTOs.Inventory;
using MediatR;

namespace Application.Core.StockTransfer.Queries
{
    public class GetStockTransfersQuery : IRequest<PagedStockTransferResponseDto>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; }
        public int? SourceWarehouseId { get; set; }
        public int? DestinationWarehouseId { get; set; }
        public string? Status { get; set; }
        public int? CompanyId { get; set; }
    }

    public class GetStockTransferByIdQuery : IRequest<StockTransferResponseDto>
    {
        public int Id { get; set; }

        public GetStockTransferByIdQuery(int id) => Id = id;
    }
}
