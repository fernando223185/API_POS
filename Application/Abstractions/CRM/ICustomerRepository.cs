using Domain.Entities;

namespace Application.Abstractions.CRM
{
    public interface ICustomerRepository
    {
        Task<Customer> CreateAsync(Customer customer);
    }
}
