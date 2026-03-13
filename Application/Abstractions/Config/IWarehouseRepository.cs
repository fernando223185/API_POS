using Domain.Entities;
using System.Threading.Tasks;

namespace Application.Abstractions.Config
{
    public interface IWarehouseRepository
    {
        Task<Warehouse?> GetByIdAsync(int id);
        Task<Warehouse?> GetByCodeAsync(string code);
        Task<List<Warehouse>> GetAllAsync(bool includeInactive = false);
        Task<List<Warehouse>> GetByBranchIdAsync(int branchId, bool includeInactive = false);
        Task<(List<Warehouse> warehouses, int totalRecords)> GetPagedAsync(int pageNumber, int pageSize, bool includeInactive = false, string? searchTerm = null, int? branchId = null);
        Task<Warehouse> CreateAsync(Warehouse warehouse);
        Task UpdateAsync(Warehouse warehouse);
        Task<bool> ExistsAsync(int id);
        Task<bool> CodeExistsAsync(string code, int? excludeId = null);
        Task<bool> BranchExistsAsync(int branchId);
        Task<string> GenerateNextCodeAsync();
        Task<int> GetTotalCountAsync();
        Task<int> GetActiveCountAsync();
        Task<int> GetCountByBranchAsync(int branchId);
    }
}
