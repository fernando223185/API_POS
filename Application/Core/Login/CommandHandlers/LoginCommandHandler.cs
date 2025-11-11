using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Login;
using Application.Core.Login.Commands;
using Application.Common.Security;
using Domain.Entities;
using MediatR;
using System.Text;

namespace Application.Core.Login.CommandHandlers
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, User>
    {
        private readonly ILoginRepository _loginRepository;

        public LoginCommandHandler(ILoginRepository loginRepository)
        {
            _loginRepository = loginRepository;
        }

        public async Task<User?> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // ✅ SOLUCIÓN TEMPORAL: Probar múltiples métodos de hash para compatibilidad
            try
            {
                // Método 1: Texto plano en bytes (para testing)
                var passwordBytes = Encoding.UTF8.GetBytes(request.Password);
                var user1 = new User
                {
                    Code = request.Code,
                    PasswordHash = passwordBytes
                };
                
                var result1 = await _loginRepository.Login(user1);
                if (result1 != null)
                {
                    return result1;
                }

                // Método 2: PasswordHasher (original)
                try
                {
                    var passwordHash = PasswordHasher.HashPassword(request.Password);
                    var user2 = new User
                    {
                        Code = request.Code,
                        PasswordHash = passwordHash
                    };
                    
                    var result2 = await _loginRepository.Login(user2);
                    if (result2 != null)
                    {
                        return result2;
                    }
                }
                catch
                {
                    // Si PasswordHasher falla, continuar con otros métodos
                }

                // Método 3: SHA256 directo
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    var sha256Hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(request.Password));
                    var user3 = new User
                    {
                        Code = request.Code,
                        PasswordHash = sha256Hash
                    };
                    
                    var result3 = await _loginRepository.Login(user3);
                    if (result3 != null)
                    {
                        return result3;
                    }
                }

                // Si ningún método funciona, retornar null
                return null;
            }
            catch (Exception ex)
            {
                // Log del error para debugging
                System.Console.WriteLine($"Login error: {ex.Message}");
                return null;
            }
        }
    }
}
