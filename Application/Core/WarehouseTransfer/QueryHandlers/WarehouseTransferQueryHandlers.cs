using Application.Abstractions.Inventory;
using Application.Core.WarehouseTransfer.CommandHandlers;
using Application.Core.WarehouseTransfer.Queries;
using Application.DTOs.Inventory;
using MediatR;

namespace Application.Core.WarehouseTransfer.QueryHandlers
{
    public class GetWarehouseTransfersQueryHandler
        : IRequestHandler<GetWarehouseTransfersQuery, PagedWarehouseTransferResponseDto>
    {
        private readonly IWarehouseTransferRepository _repo;

        public GetWarehouseTransfersQueryHandler(IWarehouseTransferRepository repo)
        {
            _repo = repo;
        }

        public async Task<PagedWarehouseTransferResponseDto> Handle(
            GetWarehouseTransfersQuery request, CancellationToken cancellationToken)
        {
            var (transfers, total) = await _repo.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                request.SourceWarehouseId,
                request.DestinationWarehouseId,
                request.Status,
                request.CompanyId);

            var items = transfers.Select(t => new WarehouseTransferListItemDto
            {
                Id = t.Id,
                Code = t.Code,
                Status = t.Status,
                SourceWarehouseName = t.SourceWarehouse?.Name ?? string.Empty,
                DestinationWarehouseName = t.DestinationWarehouse?.Name ?? string.Empty,
                TransferDate = t.TransferDate,
                DispatchedAt = t.DispatchedAt,
                CreatedAt = t.CreatedAt,
                CreatedByUserName = t.CreatedBy?.Name,
                TotalProducts = t.Details.Count,
                TotalQuantityRequested = t.Details.Sum(d => d.QuantityRequested),
                TotalQuantityReceived = t.Details.Sum(d => d.QuantityReceived)
            }).ToList();

            return new PagedWarehouseTransferResponseDto
            {
                Items = items,
                TotalRecords = total,
                Page = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)total / request.PageSize)
            };
        }
    }

    public class GetWarehouseTransferByIdQueryHandler
        : IRequestHandler<GetWarehouseTransferByIdQuery, WarehouseTransferResponseDto>
    {
        private readonly IWarehouseTransferRepository _repo;

        public GetWarehouseTransferByIdQueryHandler(IWarehouseTransferRepository repo)
        {
            _repo = repo;
        }

        public async Task<WarehouseTransferResponseDto> Handle(
            GetWarehouseTransferByIdQuery request, CancellationToken cancellationToken)
        {
            var transfer = await _repo.GetByIdAsync(request.TransferId);
            if (transfer == null)
                throw new KeyNotFoundException($"Orden de traspaso con ID {request.TransferId} no encontrada.");

            return WarehouseTransferMapper.ToResponseDto(transfer);
        }
    }

    public class GetWarehouseTransferReceivingsQueryHandler
        : IRequestHandler<GetWarehouseTransferReceivingsQuery, List<WarehouseTransferReceivingResponseDto>>
    {
        private readonly IWarehouseTransferRepository _repo;

        public GetWarehouseTransferReceivingsQueryHandler(IWarehouseTransferRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<WarehouseTransferReceivingResponseDto>> Handle(
            GetWarehouseTransferReceivingsQuery request, CancellationToken cancellationToken)
        {
            var transfer = await _repo.GetByIdAsync(request.TransferId);
            if (transfer == null)
                throw new KeyNotFoundException($"Orden de traspaso con ID {request.TransferId} no encontrada.");

            return transfer.Receivings.Select(WarehouseTransferReceivingMapper.MapReceiving).ToList();
        }
    }

    public class GetWarehouseTransferReceivingByIdQueryHandler
        : IRequestHandler<GetWarehouseTransferReceivingByIdQuery, WarehouseTransferReceivingResponseDto>
    {
        private readonly IWarehouseTransferRepository _repo;

        public GetWarehouseTransferReceivingByIdQueryHandler(IWarehouseTransferRepository repo)
        {
            _repo = repo;
        }

        public async Task<WarehouseTransferReceivingResponseDto> Handle(
            GetWarehouseTransferReceivingByIdQuery request, CancellationToken cancellationToken)
        {
            var receiving = await _repo.GetReceivingByIdAsync(request.ReceivingId);
            if (receiving == null)
                throw new KeyNotFoundException($"Entrada con ID {request.ReceivingId} no encontrada.");

            return WarehouseTransferReceivingMapper.MapReceiving(receiving);
        }
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────────

    internal static class WarehouseTransferReceivingMapper
    {
        internal static WarehouseTransferReceivingResponseDto MapReceiving(
            Domain.Entities.WarehouseTransferReceiving r) => new()
        {
            Id = r.Id,
            Code = r.Code,
            WarehouseTransferId = r.WarehouseTransferId,
            WarehouseTransferCode = r.WarehouseTransfer?.Code ?? string.Empty,
            DestinationWarehouseId = r.DestinationWarehouseId,
            DestinationWarehouseName = r.DestinationWarehouse?.Name ?? string.Empty,
            ReceivingDate = r.ReceivingDate,
            ReceivingType = r.ReceivingType,
            Notes = r.Notes,
            CreatedAt = r.CreatedAt,
            CreatedByUserName = r.CreatedBy?.Name,
            Details = r.Details.Select(d => new WarehouseTransferReceivingDetailResponseDto
            {
                Id = d.Id,
                WarehouseTransferDetailId = d.WarehouseTransferDetailId,
                ProductId = d.ProductId,
                ProductCode = d.Product?.code ?? string.Empty,
                ProductName = d.Product?.name ?? string.Empty,
                QuantityReceived = d.QuantityReceived,
                Notes = d.Notes
            }).ToList(),
            Movements = new List<WarehouseTransferReceivingMovementDto>(),
            TotalProducts = r.Details.Count,
            TotalQuantityReceived = r.Details.Sum(d => d.QuantityReceived)
        };
    }
}
