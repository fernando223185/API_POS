using Application.Core.CRM.Queries;
using Domain.Entities;

namespace Application.Abstractions.CRM
{
    public interface ICustomerRepository
    {
        Task<Customer> CreateAsync(Customer customer);
        Task<Customer?> UpdateAsync(Customer customer);
        Task<bool> DeleteAsync(int customerId);
        Task<Customer?> GetByIdAsync(int customerId);
        Task<Customer?> GetByCodeAsync(string code); // Nuevo método para validar código único
        Task<IEnumerable<Customer>> GetByPageAsync(GetCustomerByPageQuery query);
        Task<string?> GetLastCodeByPrefixAsync(string prefix); // Nuevo método para obtener el último código por prefijo
        Task<int> GetNextSequentialNumberAsync(); // Nuevo método para obtener el siguiente número secuencial
    }
}
