using Application.Abstractions.Config;
using Application.Core.Warehouse.Queries;
using Application.DTOs.Warehouse;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Core.Warehouse.QueryHandlers
{
    /// <summary>
    /// Handler para obtener todos los almacenes
    /// </summary>
    public class GetAllWarehousesQueryHandler : IRequestHandler<GetAllWarehousesQuery, WarehousesListResponseDto>
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public GetAllWarehousesQueryHandler(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<WarehousesListResponseDto> Handle(GetAllWarehousesQuery request, CancellationToken cancellationToken)
        {
            var warehouses = await _warehouseRepository.GetAllAsync(request.IncludeInactive);

            var warehouseDtos = warehouses.Select(w =>
            {
                var availableCapacity = (w.MaxCapacity ?? 0) - (w.CurrentCapacity ?? 0);
                var usagePercentage = w.MaxCapacity > 0
                    ? ((w.CurrentCapacity ?? 0) / w.MaxCapacity.Value) * 100
                    : 0;

                return new WarehouseResponseDto
                {
                    Id = w.Id,
                    Code = w.Code,
                    Name = w.Name,
                    Description = w.Description,
                    BranchId = w.BranchId,
                    BranchCode = w.Branch.Code,
                    BranchName = w.Branch.Name,
                    WarehouseType = w.WarehouseType,
                    PhysicalLocation = w.PhysicalLocation,
                    MaxCapacity = w.MaxCapacity,
                    CurrentCapacity = w.CurrentCapacity,
                    AvailableCapacity = availableCapacity,
                    CapacityUsagePercentage = usagePercentage,
                    ManagerName = w.ManagerName,
                    ManagerEmail = w.ManagerEmail,
                    ManagerPhone = w.ManagerPhone,
                    IsMainWarehouse = w.IsMainWarehouse,
                    AllowsReceiving = w.AllowsReceiving,
                    AllowsShipping = w.AllowsShipping,
                    RequiresTemperatureControl = w.RequiresTemperatureControl,
                    MinTemperature = w.MinTemperature,
                    MaxTemperature = w.MaxTemperature,
                    IsActive = w.IsActive,
                    CreatedAt = w.CreatedAt,
                    UpdatedAt = w.UpdatedAt,
                    CreatedByUserName = w.CreatedBy?.Name,
                    UpdatedByUserName = w.UpdatedBy?.Name
                };
            }).ToList();

            var totalWarehouses = await _warehouseRepository.GetTotalCountAsync();
            var activeWarehouses = await _warehouseRepository.GetActiveCountAsync();

            return new WarehousesListResponseDto
            {
                Message = "Almacenes obtenidos exitosamente",
                Error = 0,
                Warehouses = warehouseDtos,
                TotalWarehouses = totalWarehouses,
                ActiveWarehouses = activeWarehouses,
                InactiveWarehouses = totalWarehouses - activeWarehouses
            };
        }
    }

    /// <summary>
    /// Handler para obtener almacenes paginados
    /// </summary>
    public class GetWarehousesPagedQueryHandler : IRequestHandler<GetWarehousesPagedQuery, WarehousesPagedResponseDto>
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public GetWarehousesPagedQueryHandler(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<WarehousesPagedResponseDto> Handle(GetWarehousesPagedQuery request, CancellationToken cancellationToken)
        {
            var (warehouses, totalRecords) = await _warehouseRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                request.IncludeInactive,
                request.SearchTerm,
                request.BranchId
            );

            var warehouseDtos = warehouses.Select(w =>
            {
                var availableCapacity = (w.MaxCapacity ?? 0) - (w.CurrentCapacity ?? 0);
                var usagePercentage = w.MaxCapacity > 0
                    ? ((w.CurrentCapacity ?? 0) / w.MaxCapacity.Value) * 100
                    : 0;

                return new WarehouseResponseDto
                {
                    Id = w.Id,
                    Code = w.Code,
                    Name = w.Name,
                    Description = w.Description,
                    BranchId = w.BranchId,
                    BranchCode = w.Branch.Code,
                    BranchName = w.Branch.Name,
                    WarehouseType = w.WarehouseType,
                    PhysicalLocation = w.PhysicalLocation,
                    MaxCapacity = w.MaxCapacity,
                    CurrentCapacity = w.CurrentCapacity,
                    AvailableCapacity = availableCapacity,
                    CapacityUsagePercentage = usagePercentage,
                    ManagerName = w.ManagerName,
                    ManagerEmail = w.ManagerEmail,
                    ManagerPhone = w.ManagerPhone,
                    IsMainWarehouse = w.IsMainWarehouse,
                    AllowsReceiving = w.AllowsReceiving,
                    AllowsShipping = w.AllowsShipping,
                    RequiresTemperatureControl = w.RequiresTemperatureControl,
                    MinTemperature = w.MinTemperature,
                    MaxTemperature = w.MaxTemperature,
                    IsActive = w.IsActive,
                    CreatedAt = w.CreatedAt,
                    UpdatedAt = w.UpdatedAt,
                    CreatedByUserName = w.CreatedBy?.Name,
                    UpdatedByUserName = w.UpdatedBy?.Name
                };
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

            return new WarehousesPagedResponseDto
            {
                Message = "Almacenes obtenidos exitosamente",
                Error = 0,
                Data = warehouseDtos,
                CurrentPage = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                TotalRecords = totalRecords,
                HasPreviousPage = request.PageNumber > 1,
                HasNextPage = request.PageNumber < totalPages
            };
        }
    }

    /// <summary>
    /// Handler para obtener almacén por ID
    /// </summary>
    public class GetWarehouseByIdQueryHandler : IRequestHandler<GetWarehouseByIdQuery, WarehouseResponseDto?>
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public GetWarehouseByIdQueryHandler(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<WarehouseResponseDto?> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId);
            if (warehouse == null)
            {
                return null;
            }

            var availableCapacity = (warehouse.MaxCapacity ?? 0) - (warehouse.CurrentCapacity ?? 0);
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
                UpdatedAt = warehouse.UpdatedAt,
                CreatedByUserName = warehouse.CreatedBy?.Name,
                UpdatedByUserName = warehouse.UpdatedBy?.Name
            };
        }
    }

    /// <summary>
    /// Handler para obtener almacén por código
    /// </summary>
    public class GetWarehouseByCodeQueryHandler : IRequestHandler<GetWarehouseByCodeQuery, WarehouseResponseDto?>
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public GetWarehouseByCodeQueryHandler(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<WarehouseResponseDto?> Handle(GetWarehouseByCodeQuery request, CancellationToken cancellationToken)
        {
            var warehouse = await _warehouseRepository.GetByCodeAsync(request.Code);
            if (warehouse == null)
            {
                return null;
            }

            var availableCapacity = (warehouse.MaxCapacity ?? 0) - (warehouse.CurrentCapacity ?? 0);
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
                UpdatedAt = warehouse.UpdatedAt,
                CreatedByUserName = warehouse.CreatedBy?.Name,
                UpdatedByUserName = warehouse.UpdatedBy?.Name
            };
        }
    }

    /// <summary>
    /// Handler para obtener almacenes por sucursal
    /// </summary>
    public class GetWarehousesByBranchQueryHandler : IRequestHandler<GetWarehousesByBranchQuery, WarehousesByBranchResponseDto>
    {
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IBranchRepository _branchRepository;

        public GetWarehousesByBranchQueryHandler(
            IWarehouseRepository warehouseRepository,
            IBranchRepository branchRepository)
        {
            _warehouseRepository = warehouseRepository;
            _branchRepository = branchRepository;
        }

        public async Task<WarehousesByBranchResponseDto> Handle(GetWarehousesByBranchQuery request, CancellationToken cancellationToken)
        {
            var branch = await _branchRepository.GetByIdAsync(request.BranchId);
            if (branch == null)
            {
                throw new KeyNotFoundException($"Sucursal con ID {request.BranchId} no encontrada");
            }

            var warehouses = await _warehouseRepository.GetByBranchIdAsync(request.BranchId, request.IncludeInactive);

            var warehouseDtos = warehouses.Select(w =>
            {
                var availableCapacity = (w.MaxCapacity ?? 0) - (w.CurrentCapacity ?? 0);
                var usagePercentage = w.MaxCapacity > 0
                    ? ((w.CurrentCapacity ?? 0) / w.MaxCapacity.Value) * 100
                    : 0;

                return new WarehouseResponseDto
                {
                    Id = w.Id,
                    Code = w.Code,
                    Name = w.Name,
                    Description = w.Description,
                    BranchId = w.BranchId,
                    BranchCode = w.Branch.Code,
                    BranchName = w.Branch.Name,
                    WarehouseType = w.WarehouseType,
                    PhysicalLocation = w.PhysicalLocation,
                    MaxCapacity = w.MaxCapacity,
                    CurrentCapacity = w.CurrentCapacity,
                    AvailableCapacity = availableCapacity,
                    CapacityUsagePercentage = usagePercentage,
                    ManagerName = w.ManagerName,
                    ManagerEmail = w.ManagerEmail,
                    ManagerPhone = w.ManagerPhone,
                    IsMainWarehouse = w.IsMainWarehouse,
                    AllowsReceiving = w.AllowsReceiving,
                    AllowsShipping = w.AllowsShipping,
                    RequiresTemperatureControl = w.RequiresTemperatureControl,
                    MinTemperature = w.MinTemperature,
                    MaxTemperature = w.MaxTemperature,
                    IsActive = w.IsActive,
                    CreatedAt = w.CreatedAt,
                    UpdatedAt = w.UpdatedAt,
                    CreatedByUserName = w.CreatedBy?.Name,
                    UpdatedByUserName = w.UpdatedBy?.Name
                };
            }).ToList();

            return new WarehousesByBranchResponseDto
            {
                Message = "Almacenes de la sucursal obtenidos exitosamente",
                Error = 0,
                BranchId = branch.Id,
                BranchName = branch.Name,
                Warehouses = warehouseDtos,
                TotalWarehouses = warehouseDtos.Count
            };
        }
    }
}
