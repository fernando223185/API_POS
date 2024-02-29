using Domain.Entities;

namespace Application.Abstractions.CRM
{
    public interface ICustomerRepository
    {
        Task<Customer> CreateAsync(Customer customer);

        Task<Customer> UpdateAsync(Customer customer);
    }
}
