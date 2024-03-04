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
        private readonly ILogger<LoginRepository> _logger; // Agrega esto

        public LoginRepository(POSDbContext _context, ILogger<LoginRepository> logger)
		{
			_dbcontext = _context;
            _logger = logger;
        }

        public async Task<Users> Login(Users user)
        {
            var data = await _dbcontext.Users
                .Where(u => u.nameUser == user.nameUser && u.pass == user.pass)
                .ToListAsync();

            if (!data.Any())
            {
                return null;
            }

            return data.First();
        }
    }
}

