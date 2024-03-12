using Application.Core.CRM.Queries;
using Domain.DTOs;
using Domain.Entities;

namespace Application.Abstractions.CRM
{
    public interface ICustomerRepository
    {
        Task<Customer> CreateAsync(Customer customer);
        Task<Customer> UpdateAsync(Customer customer);
        Task<PaginatedDto> GetByPageAsync(GetCustomerByPageQuery data);

    }
}
