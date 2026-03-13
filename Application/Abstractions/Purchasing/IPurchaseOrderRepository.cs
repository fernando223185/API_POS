using Domain.Entities;

namespace Application.Abstractions.Purchasing
{
    public interface IPurchaseOrderRepository
    {
        // Consultas básicas
        Task<PurchaseOrder?> GetByIdAsync(int id);
        Task<PurchaseOrder?> GetByCodeAsync(string code);
        Task<List<PurchaseOrder>> GetAllAsync(bool includeInactive = false);
        
        // Consultas por filtros
        Task<List<PurchaseOrder>> GetBySupplierAsync(int supplierId, bool includeInactive = false);
        Task<List<PurchaseOrder>> GetByWarehouseAsync(int warehouseId, bool includeInactive = false);
        Task<List<PurchaseOrder>> GetByStatusAsync(string status);
        Task<List<PurchaseOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        
        // Consultas especiales
        Task<List<PurchaseOrder>> GetPendingToReceiveAsync();
        Task<List<PurchaseOrder>> GetPendingApprovalAsync();
        
        // Paginación
        Task<(List<PurchaseOrder> orders, int totalRecords)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            bool includeInactive = false,
            string? searchTerm = null,
            string? status = null,
            int? supplierId = null,
            int? warehouseId = null);
        
        // Operaciones CRUD
        Task<PurchaseOrder> CreateAsync(PurchaseOrder purchaseOrder);
        Task UpdateAsync(PurchaseOrder purchaseOrder);
        Task<bool> ExistsAsync(int id);
        Task<bool> CodeExistsAsync(string code, int? excludeId = null);
        
        // Validaciones
        Task<bool> SupplierExistsAsync(int supplierId);
        Task<bool> WarehouseExistsAsync(int warehouseId);
        Task<bool> ProductExistsAsync(int productId);
        
        // Estadísticas
        Task<int> GetTotalCountAsync();
        Task<int> GetCountByStatusAsync(string status);
        Task<decimal> GetTotalAmountBySupplierAsync(int supplierId, DateTime? startDate = null);
    }
}
