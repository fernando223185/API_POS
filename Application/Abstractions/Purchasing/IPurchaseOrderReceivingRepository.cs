using Domain.Entities;

namespace Application.Abstractions.Purchasing
{
    public interface IPurchaseOrderReceivingRepository
    {
        // Consultas básicas
        Task<PurchaseOrderReceiving?> GetByIdAsync(int id);
        Task<PurchaseOrderReceiving?> GetByCodeAsync(string code);
        Task<List<PurchaseOrderReceiving>> GetAllAsync(bool includePosted = true);
        
        // Consultas por filtros
        Task<List<PurchaseOrderReceiving>> GetByPurchaseOrderAsync(int purchaseOrderId);
        Task<List<PurchaseOrderReceiving>> GetByWarehouseAsync(int warehouseId);
        Task<List<PurchaseOrderReceiving>> GetPendingToPostAsync();
        
        // Paginación
        Task<(List<PurchaseOrderReceiving> receivings, int totalRecords)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            int? purchaseOrderId = null,
            int? warehouseId = null,
            string? status = null,
            bool? onlyPendingToPost = null);
        
        // Operaciones CRUD
        Task<PurchaseOrderReceiving> CreateAsync(PurchaseOrderReceiving receiving);
        Task UpdateAsync(PurchaseOrderReceiving receiving);
        Task<bool> ExistsAsync(int id);
        
        // Validaciones
        Task<bool> PurchaseOrderExistsAsync(int purchaseOrderId);
        Task<bool> CanReceiveAsync(int purchaseOrderId);
        
        // Estadísticas
        Task<int> GetTotalCountAsync();
        Task<int> GetPendingToPostCountAsync();
    }
}
