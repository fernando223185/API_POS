using Application.Abstractions.Config;
using Application.Core.Warehouse.Commands;
using Application.DTOs.Warehouse;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Core.Warehouse.CommandHandlers
{
    /// <summary>
    /// Handler para crear almacén
    /// </summary>
    public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, WarehouseResponseDto>
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public CreateWarehouseCommandHandler(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<WarehouseResponseDto> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
        {
            // Validar que la sucursal existe
            if (!await _warehouseRepository.BranchExistsAsync(request.WarehouseData.BranchId))
            {
                throw new KeyNotFoundException($"Sucursal con ID {request.WarehouseData.BranchId} no encontrada");
            }

            // Generar código automático
            var code = await _warehouseRepository.GenerateNextCodeAsync();

            var warehouse = new Domain.Entities.Warehouse
            {
                Code = code,
                Name = request.WarehouseData.Name,
                Description = request.WarehouseData.Description,
                BranchId = request.WarehouseData.BranchId,
                WarehouseType = request.WarehouseData.WarehouseType,
                PhysicalLocation = request.WarehouseData.PhysicalLocation,
                MaxCapacity = request.WarehouseData.MaxCapacity,
                CurrentCapacity = 0, // Inicia en 0
                ManagerName = request.WarehouseData.ManagerName,
                ManagerEmail = request.WarehouseData.ManagerEmail,
                ManagerPhone = request.WarehouseData.ManagerPhone,
                IsMainWarehouse = request.WarehouseData.IsMainWarehouse,
                AllowsReceiving = request.WarehouseData.AllowsReceiving,
                AllowsShipping = request.WarehouseData.AllowsShipping,
                RequiresTemperatureControl = request.WarehouseData.RequiresTemperatureControl,
                MinTemperature = request.WarehouseData.MinTemperature,
                MaxTemperature = request.WarehouseData.MaxTemperature,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = request.CreatedByUserId
            };

            var createdWarehouse = await _warehouseRepository.CreateAsync(warehouse);

            var availableCapacity = (createdWarehouse.MaxCapacity ?? 0) - (createdWarehouse.CurrentCapacity ?? 0);
            var usagePercentage = createdWarehouse.MaxCapacity > 0
                ? ((createdWarehouse.CurrentCapacity ?? 0) / createdWarehouse.MaxCapacity.Value) * 100
                : 0;

            return new WarehouseResponseDto
            {
                Id = createdWarehouse.Id,
                Code = createdWarehouse.Code,
                Name = createdWarehouse.Name,
                Description = createdWarehouse.Description,
                BranchId = createdWarehouse.BranchId,
                BranchCode = createdWarehouse.Branch.Code,
                BranchName = createdWarehouse.Branch.Name,
                WarehouseType = createdWarehouse.WarehouseType,
                PhysicalLocation = createdWarehouse.PhysicalLocation,
                MaxCapacity = createdWarehouse.MaxCapacity,
                CurrentCapacity = createdWarehouse.CurrentCapacity,
                AvailableCapacity = availableCapacity,
                CapacityUsagePercentage = usagePercentage,
                ManagerName = createdWarehouse.ManagerName,
                ManagerEmail = createdWarehouse.ManagerEmail,
                ManagerPhone = createdWarehouse.ManagerPhone,
                IsMainWarehouse = createdWarehouse.IsMainWarehouse,
                AllowsReceiving = createdWarehouse.AllowsReceiving,
                AllowsShipping = createdWarehouse.AllowsShipping,
                RequiresTemperatureControl = createdWarehouse.RequiresTemperatureControl,
                MinTemperature = createdWarehouse.MinTemperature,
                MaxTemperature = createdWarehouse.MaxTemperature,
                IsActive = createdWarehouse.IsActive,
                CreatedAt = createdWarehouse.CreatedAt
            };
        }
    }

    /// <summary>
    /// Handler para actualizar almacén
    /// </summary>
    public class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand, WarehouseResponseDto>
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public UpdateWarehouseCommandHandler(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<WarehouseResponseDto> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId);
            if (warehouse == null)
            {
                throw new KeyNotFoundException($"Almacén con ID {request.WarehouseId} no encontrado");
            }

            // Validar que la sucursal existe
            if (!await _warehouseRepository.BranchExistsAsync(request.WarehouseData.BranchId))
            {
                throw new KeyNotFoundException($"Sucursal con ID {request.WarehouseData.BranchId} no encontrada");
            }

            // Actualizar campos
            warehouse.Name = request.WarehouseData.Name;
            warehouse.Description = request.WarehouseData.Description;
            warehouse.BranchId = request.WarehouseData.BranchId;
            warehouse.WarehouseType = request.WarehouseData.WarehouseType;
            warehouse.PhysicalLocation = request.WarehouseData.PhysicalLocation;
            warehouse.MaxCapacity = request.WarehouseData.MaxCapacity;
            warehouse.CurrentCapacity = request.WarehouseData.CurrentCapacity;
            warehouse.ManagerName = request.WarehouseData.ManagerName;
            warehouse.ManagerEmail = request.WarehouseData.ManagerEmail;
            warehouse.ManagerPhone = request.WarehouseData.ManagerPhone;
            warehouse.IsMainWarehouse = request.WarehouseData.IsMainWarehouse;
            warehouse.AllowsReceiving = request.WarehouseData.AllowsReceiving;
            warehouse.AllowsShipping = request.WarehouseData.AllowsShipping;
            warehouse.RequiresTemperatureControl = request.WarehouseData.RequiresTemperatureControl;
            warehouse.MinTemperature = request.WarehouseData.MinTemperature;
            warehouse.MaxTemperature = request.WarehouseData.MaxTemperature;
            warehouse.IsActive = request.WarehouseData.IsActive;
            warehouse.UpdatedAt = DateTime.UtcNow;
            warehouse.UpdatedByUserId = request.UpdatedByUserId;

            await _warehouseRepository.UpdateAsync(warehouse);

            // Recargar con navegación
            warehouse = await _warehouseRepository.GetByIdAsync(warehouse.Id);

            var availableCapacity = (warehouse!.MaxCapacity ?? 0) - (warehouse.CurrentCapacity ?? 0);
            var usagePercentage = warehouse.MaxCapacity > 0
                ? ((warehouse.CurrentCapacity ?? 0) / warehouse.MaxCapacity.Value) * 100
                : 0;

            return new WarehouseResponseDto
            {
                Id = warehouse.Id,
                Code = warehouse.Code,
                Name = warehouse.Name,
                Description = warehouse.Description,
                BranchId = warehouse.BranchId,
                BranchCode = warehouse.Branch.Code,
                BranchName = warehouse.Branch.Name,
                WarehouseType = warehouse.WarehouseType,
                PhysicalLocation = warehouse.PhysicalLocation,
                MaxCapacity = warehouse.MaxCapacity,
                CurrentCapacity = warehouse.CurrentCapacity,
                AvailableCapacity = availableCapacity,
                CapacityUsagePercentage = usagePercentage,
                ManagerName = warehouse.ManagerName,
                ManagerEmail = warehouse.ManagerEmail,
                ManagerPhone = warehouse.ManagerPhone,
                IsMainWarehouse = warehouse.IsMainWarehouse,
                AllowsReceiving = warehouse.AllowsReceiving,
                AllowsShipping = warehouse.AllowsShipping,
                RequiresTemperatureControl = warehouse.RequiresTemperatureControl,
                MinTemperature = warehouse.MinTemperature,
                MaxTemperature = warehouse.MaxTemperature,
                IsActive = warehouse.IsActive,
                CreatedAt = warehouse.CreatedAt,
                UpdatedAt = warehouse.UpdatedAt
            };
        }
    }

    /// <summary>
    /// Handler para desactivar almacén
    /// </summary>
    public class DeactivateWarehouseCommandHandler : IRequestHandler<DeactivateWarehouseCommand, bool>
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public DeactivateWarehouseCommandHandler(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<bool> Handle(DeactivateWarehouseCommand request, CancellationToken cancellationToken)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId);
            if (warehouse == null)
            {
                return false;
            }

            warehouse.IsActive = false;
            warehouse.UpdatedAt = DateTime.UtcNow;
            warehouse.UpdatedByUserId = request.UpdatedByUserId;

            await _warehouseRepository.UpdateAsync(warehouse);
            return true;
        }
    }

    /// <summary>
    /// Handler para reactivar almacén
    /// </summary>
    public class ReactivateWarehouseCommandHandler : IRequestHandler<ReactivateWarehouseCommand, bool>
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public ReactivateWarehouseCommandHandler(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<bool> Handle(ReactivateWarehouseCommand request, CancellationToken cancellationToken)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId);
            if (warehouse == null)
            {
                return false;
            }

            warehouse.IsActive = true;
            warehouse.UpdatedAt = DateTime.UtcNow;
            warehouse.UpdatedByUserId = request.UpdatedByUserId;

            await _warehouseRepository.UpdateAsync(warehouse);
            return true;
        }
    }
}
