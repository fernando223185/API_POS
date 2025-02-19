using Domain.Entities;
using Infrastructure.Persistence;
using Application.Abstractions.Catalogue;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly POSDbContext _dbcontext;

		public UserRepository(POSDbContext dbContext)
		{
			_dbcontext = dbContext;
		}

		public async Task<User> CreateAsync(User user)
		{
			_dbcontext.User.Add(user);
            await _dbcontext.SaveChangesAsync();
			return user;
        }

    }
}

