using Application.Abstractions.Inventory;
using Application.Core.StockTransfer.Queries;
using Application.DTOs.Inventory;
using Domain.Entities;
using MediatR;

namespace Application.Core.StockTransfer.QueryHandlers
{
    public class GetStockTransfersQueryHandler : IRequestHandler<GetStockTransfersQuery, PagedStockTransferResponseDto>
    {
        private readonly IStockTransferRepository _transferRepo;

        public GetStockTransfersQueryHandler(IStockTransferRepository transferRepo)
        {
            _transferRepo = transferRepo;
        }

        public async Task<PagedStockTransferResponseDto> Handle(GetStockTransfersQuery request, CancellationToken cancellationToken)
        {
            var (transfers, total) = await _transferRepo.GetPagedAsync(
                request.Page,
                request.PageSize,
                request.SearchTerm,
                request.SourceWarehouseId,
                request.DestinationWarehouseId,
                request.Status,
                request.CompanyId);

            return new PagedStockTransferResponseDto
            {
                Items = transfers.Select(MapToListItem).ToList(),
                TotalRecords = total,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
            };
        }

        private static StockTransferListItemDto MapToListItem(Domain.Entities.StockTransfer t) => new()
        {
            Id = t.Id,
            Code = t.Code,
            Status = t.Status,
            SourceWarehouseName = t.SourceWarehouse?.Name ?? string.Empty,
            DestinationWarehouseName = t.DestinationWarehouse?.Name ?? string.Empty,
            TransferDate = t.TransferDate,
            IsApplied = t.IsApplied,
            AppliedAt = t.AppliedAt,
            CreatedAt = t.CreatedAt,
            CreatedByUserName = t.CreatedBy?.Name,
            TotalProducts = t.Details.Count,
            TotalQuantity = t.Details.Sum(d => d.Quantity)
        };
    }

    public class GetStockTransferByIdQueryHandler : IRequestHandler<GetStockTransferByIdQuery, StockTransferResponseDto>
    {
        private readonly IStockTransferRepository _transferRepo;

        public GetStockTransferByIdQueryHandler(IStockTransferRepository transferRepo)
        {
            _transferRepo = transferRepo;
        }

        public async Task<StockTransferResponseDto> Handle(GetStockTransferByIdQuery request, CancellationToken cancellationToken)
        {
            var transfer = await _transferRepo.GetByIdAsync(request.Id);
            if (transfer == null)
                throw new KeyNotFoundException($"Traspaso con ID {request.Id} no encontrado.");

            return MapToResponse(transfer);
        }

        private static StockTransferResponseDto MapToResponse(Domain.Entities.StockTransfer t) => new()
        {
            Id = t.Id,
            Code = t.Code,
            Status = t.Status,
            SourceWarehouseId = t.SourceWarehouseId,
            SourceWarehouseName = t.SourceWarehouse?.Name ?? string.Empty,
            SourceWarehouseCode = t.SourceWarehouse?.Code ?? string.Empty,
            DestinationWarehouseId = t.DestinationWarehouseId,
            DestinationWarehouseName = t.DestinationWarehouse?.Name ?? string.Empty,
            DestinationWarehouseCode = t.DestinationWarehouse?.Code ?? string.Empty,
            CompanyId = t.CompanyId,
            TransferDate = t.TransferDate,
            Notes = t.Notes,
            IsApplied = t.IsApplied,
            AppliedAt = t.AppliedAt,
            AppliedByUserName = t.AppliedBy?.Name,
            CreatedAt = t.CreatedAt,
            CreatedByUserName = t.CreatedBy?.Name,
            Details = t.Details.Select(d => new StockTransferDetailResponseDto
            {
                Id = d.Id,
                ProductId = d.ProductId,
                ProductCode = d.Product?.code ?? string.Empty,
                ProductName = d.Product?.name ?? string.Empty,
                Quantity = d.Quantity,
                UnitCost = d.UnitCost,
                Notes = d.Notes
            }).ToList(),
            TotalProducts = t.Details.Count,
            TotalQuantity = t.Details.Sum(d => d.Quantity)
        };
    }
}
