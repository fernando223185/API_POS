using Domain.Entities;
using Infrastructure.Persistence;
using Application.Abstractions.Login;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;



namespace Infrastructure.Repositories
{
	public class LoginRepository : ILoginRepository
	{
		private readonly POSDbContext _dbcontext;
        private readonly ILogger<LoginRepository> _logger; 

        public LoginRepository(POSDbContext _context, ILogger<LoginRepository> logger)
		{
			_dbcontext = _context;
            _logger = logger;
        }

        public async Task<User> Login(User user)
        {
            var data = await _dbcontext.User
                .Where(u => u.Code == user.Code && u.PasswordHash == user.PasswordHash)
                .ToListAsync();

            if (!data.Any())
            {
                return null;
            }

            return data.First();
        }

        
    }
}

