using Application.DTOs.Inventory;
using MediatR;

namespace Application.Core.WarehouseTransfer.Commands
{
    /// <summary>Crea una orden de traspaso en estado Draft.</summary>
    public record CreateWarehouseTransferCommand(
        CreateWarehouseTransferDto Data,
        int UserId) : IRequest<WarehouseTransferResponseDto>;

    /// <summary>Actualiza una orden en estado Draft.</summary>
    public record UpdateWarehouseTransferCommand(
        int TransferId,
        UpdateWarehouseTransferDto Data,
        int UserId) : IRequest<WarehouseTransferResponseDto>;

    /// <summary>
    /// Confirma la salida de mercancía desde el almacén origen.
    /// Crea movimientos de inventario tipo SALIDA y avanza el estado a Dispatched.
    /// </summary>
    public record DispatchWarehouseTransferCommand(
        int TransferId,
        DispatchWarehouseTransferDto Data,
        int UserId) : IRequest<DispatchWarehouseTransferResponseDto>;

    /// <summary>
    /// Registra una entrada (parcial o completa) en el almacén destino.
    /// Crea movimientos de inventario tipo ENTRADA.
    /// </summary>
    public record CreateWarehouseTransferReceivingCommand(
        int TransferId,
        CreateWarehouseTransferReceivingDto Data,
        int UserId) : IRequest<WarehouseTransferReceivingResponseDto>;

    /// <summary>Cancela una orden en estado Draft.</summary>
    public record CancelWarehouseTransferCommand(
        int TransferId,
        int UserId) : IRequest<bool>;
}
