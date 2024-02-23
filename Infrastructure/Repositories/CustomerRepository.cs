using Domain.Entities;
using Infrastructure.Persistence;
using Application.Abstractions.CRM;
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
	}
}

