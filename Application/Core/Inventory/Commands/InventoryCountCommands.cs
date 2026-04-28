using Application.DTOs.Inventory;
using MediatR;

namespace Application.Core.Inventory.Commands
{
    /// <summary>
    /// Comando para crear una nueva sesión de conteo de inventario
    /// </summary>
    public record CreateInventoryCountCommand(
        CreateInventoryCountDto Dto,
        int CreatedByUserId,
        int? CompanyId
    ) : IRequest<InventoryCountResponseDto>;

    /// <summary>
    /// Comando para actualizar una sesión de conteo existente (solo en estado Draft)
    /// </summary>
    public record UpdateInventoryCountCommand(
        int CountId,
        UpdateInventoryCountDto Dto,
        int UserId
    ) : IRequest<InventoryCountResponseDto>;

    /// <summary>
    /// Comando para iniciar un conteo (cambiar de Draft a InProgress)
    /// </summary>
    public record StartInventoryCountCommand(
        int CountId,
        StartInventoryCountDto Dto,
        int UserId
    ) : IRequest<InventoryCountResponseDto>;

    /// <summary>
    /// Comando para actualizar la cantidad física de un producto durante el conteo
    /// </summary>
    public record UpdateCountDetailCommand(
        int CountId,
        int DetailId,
        UpdateCountDetailDto Dto,
        int CountedByUserId
    ) : IRequest<InventoryCountDetailResponseDto>;

    /// <summary>
    /// Comando para solicitar reconteo de un producto con discrepancia alta
    /// </summary>
    public record RequestRecountCommand(
        int CountId,
        int DetailId,
        RequestRecountDto Dto,
        int RequestedByUserId
    ) : IRequest<InventoryCountDetailResponseDto>;

    /// <summary>
    /// Comando para completar y aprobar un conteo.
    /// Genera ajustes de inventario automáticamente para todos los productos con variación.
    /// </summary>
    public record CompleteInventoryCountCommand(
        int CountId,
        CompleteInventoryCountDto Dto,
        int ApprovedByUserId
    ) : IRequest<InventoryCountResponseDto>;

    /// <summary>
    /// Comando para cancelar un conteo
    /// </summary>
    public record CancelInventoryCountCommand(
        int CountId,
        string? Reason,
        int CancelledByUserId
    ) : IRequest<Unit>;
}
