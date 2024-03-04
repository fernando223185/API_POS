using Domain.Entities;
using Infrastructure.Persistence;
using Application.Abstractions.CRM;
using Microsoft.EntityFrameworkCore;

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
	}
}

