using Domain.Entities;

namespace Application.Abstractions.CRM
{
    public interface ICustomerRepository
    {
        Task<Customer> CreateAsync(Customer customer);
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer?> GetByCodeAsync(string code);
        Task<Customer> UpdateAsync(Customer customer);
        Task<bool> DeleteAsync(Customer customer);
        Task<IEnumerable<Customer>> GetPagedAsync(int page, int pageSize);
        Task<int> GetTotalCountAsync();
        Task<IEnumerable<Customer>> SearchAsync(string searchTerm, int page, int pageSize);
        
        // ✅ Para generación automática de códigos
        Task<string?> GetLastCodeByPrefixAsync(string prefix);
        Task<int> GetNextSequentialNumberAsync();
        
        // ✅ Para paginación avanzada con filtros y ordenamiento
        Task<(IEnumerable<Customer> customers, int totalCount)> GetPagedWithCountAsync(
            int page,
            int pageSize,
            string? searchTerm = null,
            string? sortBy = "name",
            string? sortDirection = "asc",
            bool? isActive = null,
            int? statusId = null,
            int? priceListId = null
        );
    }
}
