using Domain.Entities;
using Infrastructure.Persistence;
using Application.Abstractions.CRM;
using Microsoft.EntityFrameworkCore;
using Domain.DTOs;
using Application.Core.CRM.Queries;


namespace Infrastructure.Repositories
{
	public class CustomerRepository : ICustomerRepository
	{
		private readonly POSDbContext _dbcontext;

		public CustomerRepository(POSDbContext dbcontext)
		{
			_dbcontext = dbcontext;
		}

		public async Task<Customer> CreateAsync(Customer customer)
		{
			_dbcontext.Customer.Add(customer);
			await _dbcontext.SaveChangesAsync();
			return customer;

        }

		public async Task<Customer> UpdateAsync(Customer customer)
        {
			var info = await _dbcontext.Customer.FirstOrDefaultAsync(c => c.ID == customer.ID);
			info.Code = customer.Code;
			info.Name = customer.Name;
			info.LastName = customer.LastName;
			info.Phone = customer.Phone;
			info.Email = customer.Email;
			info.Address = customer.Address;
			info.TaxId = customer.TaxId;
			info.ZipCode = customer.ZipCode;
			info.Commentary = customer.Commentary;
			info.CountryId = customer.CountryId;
			info.StateId = customer.StateId;
			info.InteriorNumber = customer.InteriorNumber;
			info.ExteriorNumber = customer.ExteriorNumber;

			await _dbcontext.SaveChangesAsync();

			return info;
		}

		public async Task<PaginatedDto> GetByPageAsync(GetCustomerByPageQuery data)
		{
			int pageSize = 50;
			PaginatedDto paginate = new PaginatedDto();

			IQueryable<Customer> query = _dbcontext.Customer.AsQueryable();

			if (!string.IsNullOrEmpty(data.search))
			{
				query = query.Where( c => c.Code == data.search 
						|| c.Name.Contains(data.search)
						|| c.LastName.Contains(data.search)
				);
			}

			int pagesCount = (int)Math.Ceiling((decimal)await query.CountAsync() / pageSize);

            var customers = await query
                .OrderByDescending(c => c.ID)
                .Skip((data.Page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new
                {
                    s.ID,
                    s.Code,
                    s.Name,
                    s.LastName,
                    s.Phone,
                    s.Email,
                    s.Address,
                    s.TaxId,
                    s.ZipCode,
                    s.Commentary,
                    s.CountryId,
                    s.StateId,
                    s.InteriorNumber,
                    s.ExteriorNumber,
                    s.StatusId
                })
                .ToListAsync();
			
			paginate.sizePage = pagesCount;
			paginate.page = data.Page;
            paginate.totalPages = pagesCount;
            paginate.data = customers;
			
			return paginate;
        }

		public async Task<Customer> GetByIdAsync(int customerID)
		{
			return await _dbcontext.Customer.FirstOrDefaultAsync(c => c.ID == customerID);
		}
	}
}

