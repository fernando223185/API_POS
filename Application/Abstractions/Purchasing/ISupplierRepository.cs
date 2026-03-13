using Domain.Entities;

namespace Application.Abstractions.Purchasing
{
    public interface ISupplierRepository
    {
        // Consultas básicas
        Task<Supplier?> GetByIdAsync(int id);
        Task<Supplier?> GetByCodeAsync(string code);
        Task<List<Supplier>> GetAllAsync(bool includeInactive = false);
        
        // Paginación
        Task<(List<Supplier> suppliers, int totalRecords)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            bool includeInactive = false,
            string? searchTerm = null);
        
        // Operaciones CRUD
        Task<Supplier> CreateAsync(Supplier supplier);
        Task UpdateAsync(Supplier supplier);
        Task<bool> ExistsAsync(int id);
        Task<bool> CodeExistsAsync(string code, int? excludeId = null);
        Task<bool> TaxIdExistsAsync(string taxId, int? excludeId = null);
        
        // Estadísticas
        Task<int> GetTotalCountAsync();
        Task<int> GetActiveCountAsync();
        Task<decimal> GetTotalPurchasedAsync(int supplierId, DateTime? startDate = null);
        Task<int> GetPurchaseOrderCountAsync(int supplierId);
    }
}
