using System;

namespace Application.DTOs.Warehouse
{
    /// <summary>
    /// DTO para crear un nuevo almacén
    /// </summary>
    public class CreateWarehouseDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int BranchId { get; set; }
        public string WarehouseType { get; set; } = "General";
        public string? PhysicalLocation { get; set; }
        public decimal? MaxCapacity { get; set; }
        public string? ManagerName { get; set; }
        public string? ManagerEmail { get; set; }
        public string? ManagerPhone { get; set; }
        public bool IsMainWarehouse { get; set; } = false;
        public bool AllowsReceiving { get; set; } = true;
        public bool AllowsShipping { get; set; } = true;
        public bool RequiresTemperatureControl { get; set; } = false;
        public decimal? MinTemperature { get; set; }
        public decimal? MaxTemperature { get; set; }
    }

    /// <summary>
    /// DTO para actualizar un almacén existente
    /// </summary>
    public class UpdateWarehouseDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int BranchId { get; set; }
        public string WarehouseType { get; set; } = "General";
        public string? PhysicalLocation { get; set; }
        public decimal? MaxCapacity { get; set; }
        public decimal? CurrentCapacity { get; set; }
        public string? ManagerName { get; set; }
        public string? ManagerEmail { get; set; }
        public string? ManagerPhone { get; set; }
        public bool IsMainWarehouse { get; set; }
        public bool AllowsReceiving { get; set; }
        public bool AllowsShipping { get; set; }
        public bool RequiresTemperatureControl { get; set; }
        public decimal? MinTemperature { get; set; }
        public decimal? MaxTemperature { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO de respuesta para almacén
    /// </summary>
    public class WarehouseResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int BranchId { get; set; }
        public string BranchCode { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string WarehouseType { get; set; } = string.Empty;
        public string? PhysicalLocation { get; set; }
        public decimal? MaxCapacity { get; set; }
        public decimal? CurrentCapacity { get; set; }
        public decimal? AvailableCapacity { get; set; }
        public decimal? CapacityUsagePercentage { get; set; }
        public string? ManagerName { get; set; }
        public string? ManagerEmail { get; set; }
        public string? ManagerPhone { get; set; }
        public bool IsMainWarehouse { get; set; }
        public bool AllowsReceiving { get; set; }
        public bool AllowsShipping { get; set; }
        public bool RequiresTemperatureControl { get; set; }
        public decimal? MinTemperature { get; set; }
        public decimal? MaxTemperature { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedByUserName { get; set; }
        public string? UpdatedByUserName { get; set; }
    }

    /// <summary>
    /// DTO de respuesta para lista de almacenes
    /// </summary>
    public class WarehousesListResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public List<WarehouseResponseDto> Warehouses { get; set; } = new();
        public int TotalWarehouses { get; set; }
        public int ActiveWarehouses { get; set; }
        public int InactiveWarehouses { get; set; }
    }

    /// <summary>
    /// DTO para respuesta paginada de almacenes
    /// </summary>
    public class WarehousesPagedResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public List<WarehouseResponseDto> Data { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    /// <summary>
    /// DTO para obtener almacenes por sucursal
    /// </summary>
    public class WarehousesByBranchResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public List<WarehouseResponseDto> Warehouses { get; set; } = new();
        public int TotalWarehouses { get; set; }
    }
}
