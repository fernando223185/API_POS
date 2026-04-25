using Application.DTOs.Inventory;
using MediatR;

namespace Application.Core.WarehouseTransfer.Queries
{
    /// <summary>Lista paginada de órdenes de traspaso.</summary>
    public record GetWarehouseTransfersQuery(
        int PageNumber,
        int PageSize,
        string? SearchTerm,
        int? SourceWarehouseId,
        int? DestinationWarehouseId,
        string? Status,
        int? CompanyId) : IRequest<PagedWarehouseTransferResponseDto>;

    /// <summary>Obtiene una orden de traspaso por ID con todos sus detalles y entradas.</summary>
    public record GetWarehouseTransferByIdQuery(int TransferId)
        : IRequest<WarehouseTransferResponseDto>;

    /// <summary>Obtiene todas las entradas registradas de una orden de traspaso.</summary>
    public record GetWarehouseTransferReceivingsQuery(int TransferId)
        : IRequest<List<WarehouseTransferReceivingResponseDto>>;

    /// <summary>Obtiene una entrada específica por ID.</summary>
    public record GetWarehouseTransferReceivingByIdQuery(int ReceivingId)
        : IRequest<WarehouseTransferReceivingResponseDto>;
}
