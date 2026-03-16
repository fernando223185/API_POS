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
            // Primero buscar el usuario por código
            var userInDb = await _dbcontext.User
                .FirstOrDefaultAsync(u => u.Code == user.Code);

            if (userInDb == null)
            {
                _logger.LogWarning($"Usuario {user.Code} no encontrado");
                return null;
            }

            // Comparar los hashes en memoria (no en SQL)
            if (userInDb.PasswordHash == null || user.PasswordHash == null)
            {
                _logger.LogWarning($"Usuario {user.Code}: PasswordHash es null");
                return null;
            }

            if (userInDb.PasswordHash.Length != user.PasswordHash.Length)
            {
                _logger.LogWarning($"Usuario {user.Code}: Longitud de hash no coincide");
                return null;
            }

            // Comparar byte por byte
            bool passwordMatch = true;
            for (int i = 0; i < userInDb.PasswordHash.Length; i++)
            {
                if (userInDb.PasswordHash[i] != user.PasswordHash[i])
                {
                    passwordMatch = false;
                    break;
                }
            }

            if (!passwordMatch)
            {
                _logger.LogWarning($"Usuario {user.Code}: Contraseña incorrecta");
                return null;
            }

            _logger.LogInformation($"Usuario {user.Code}: Login exitoso");
            return userInDb;
        }

        
    }
}

