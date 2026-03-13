using Application.DTOs.Warehouse;
using MediatR;

namespace Application.Core.Warehouse.Commands
{
    /// <summary>
    /// Comando para crear un nuevo almacén
    /// </summary>
    public class CreateWarehouseCommand : IRequest<WarehouseResponseDto>
    {
        public CreateWarehouseDto WarehouseData { get; set; }
        public int CreatedByUserId { get; set; }

        public CreateWarehouseCommand(CreateWarehouseDto warehouseData, int createdByUserId)
        {
            WarehouseData = warehouseData;
            CreatedByUserId = createdByUserId;
        }
    }

    /// <summary>
    /// Comando para actualizar un almacén existente
    /// </summary>
    public class UpdateWarehouseCommand : IRequest<WarehouseResponseDto>
    {
        public int WarehouseId { get; set; }
        public UpdateWarehouseDto WarehouseData { get; set; }
        public int UpdatedByUserId { get; set; }

        public UpdateWarehouseCommand(int warehouseId, UpdateWarehouseDto warehouseData, int updatedByUserId)
        {
            WarehouseId = warehouseId;
            WarehouseData = warehouseData;
            UpdatedByUserId = updatedByUserId;
        }
    }

    /// <summary>
    /// Comando para dar de baja lógica un almacén
    /// </summary>
    public class DeactivateWarehouseCommand : IRequest<bool>
    {
        public int WarehouseId { get; set; }
        public int UpdatedByUserId { get; set; }

        public DeactivateWarehouseCommand(int warehouseId, int updatedByUserId)
        {
            WarehouseId = warehouseId;
            UpdatedByUserId = updatedByUserId;
        }
    }

    /// <summary>
    /// Comando para reactivar un almacén
    /// </summary>
    public class ReactivateWarehouseCommand : IRequest<bool>
    {
        public int WarehouseId { get; set; }
        public int UpdatedByUserId { get; set; }

        public ReactivateWarehouseCommand(int warehouseId, int updatedByUserId)
        {
            WarehouseId = warehouseId;
            UpdatedByUserId = updatedByUserId;
        }
    }
}
