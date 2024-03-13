<<<<<<< HEAD
﻿using Application.Core.CRM.Queries;
using Domain.DTOs;
using Domain.Entities;
=======
﻿using Domain.Entities;
>>>>>>> d399b8080a32d22d35314462b39a24df68edce47

namespace Application.Abstractions.CRM
{
    public interface ICustomerRepository
    {
        Task<Customer> CreateAsync(Customer customer);
<<<<<<< HEAD
        Task<Customer> UpdateAsync(Customer customer);
        Task<PaginatedDto> GetByPageAsync(GetCustomerByPageQuery data);

=======

        Task<Customer> UpdateAsync(Customer customer);
>>>>>>> d399b8080a32d22d35314462b39a24df68edce47
    }
}
