using Domain.Entities;
using System.Threading.Tasks;

namespace Application.Abstractions.Config
{
    public interface IBranchRepository
    {
        Task<Branch?> GetByIdAsync(int id);
        Task<Branch?> GetByCodeAsync(string code);
        Task<List<Branch>> GetAllAsync(bool includeInactive = false);
        Task<(List<Branch> branches, int totalRecords)> GetPagedAsync(int pageNumber, int pageSize, bool includeInactive = false, string? searchTerm = null);
        Task<Branch> CreateAsync(Branch branch);
        Task UpdateAsync(Branch branch);
        Task<bool> ExistsAsync(int id);
        Task<bool> CodeExistsAsync(string code, int? excludeId = null);
        Task<string> GenerateNextCodeAsync();
        Task<int> GetTotalCountAsync();
        Task<int> GetActiveCountAsync();
    }
}
