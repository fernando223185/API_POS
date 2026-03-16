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
                System.Console.WriteLine($"🔍 Intentando login para usuario: {request.Code}");
                
                // Método 1: Texto plano en bytes (para testing)
                System.Console.WriteLine("   Método 1: Texto plano en bytes...");
                var passwordBytes = Encoding.UTF8.GetBytes(request.Password);
                System.Console.WriteLine($"   Password bytes length: {passwordBytes.Length}");
                System.Console.WriteLine($"   Password bytes: {BitConverter.ToString(passwordBytes)}");
                
                var user1 = new User
                {
                    Code = request.Code,
                    PasswordHash = passwordBytes
                };
                
                var result1 = await _loginRepository.Login(user1);
                if (result1 != null)
                {
                    System.Console.WriteLine("   ✅ Login exitoso con Método 1 (texto plano)");
                    return result1;
                }

                // Método 2: PasswordHasher (SHA256)
                System.Console.WriteLine("   Método 2: PasswordHasher (SHA256)...");
                try
                {
                    var passwordHash = PasswordHasher.HashPassword(request.Password);
                    System.Console.WriteLine($"   SHA256 hash length: {passwordHash.Length}");
                    System.Console.WriteLine($"   SHA256 hash: {BitConverter.ToString(passwordHash).Replace("-", "").Substring(0, 32)}...");
                    
                    var user2 = new User
                    {
                        Code = request.Code,
                        PasswordHash = passwordHash
                    };
                    
                    var result2 = await _loginRepository.Login(user2);
                    if (result2 != null)
                    {
                        System.Console.WriteLine("   ✅ Login exitoso con Método 2 (SHA256)");
                        return result2;
                    }
                }
                catch (Exception ex2)
                {
                    System.Console.WriteLine($"   ❌ Error en Método 2: {ex2.Message}");
                }

                // Método 3: SHA256 directo
                System.Console.WriteLine("   Método 3: SHA256 directo...");
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
                        System.Console.WriteLine("   ✅ Login exitoso con Método 3 (SHA256 directo)");
                        return result3;
                    }
                }

                System.Console.WriteLine("   ❌ Todos los métodos fallaron");
                return null;
            }
            catch (Exception ex)
            {
                // Log del error para debugging
                System.Console.WriteLine($"❌ Login error: {ex.Message}");
                System.Console.WriteLine($"   Stack: {ex.StackTrace}");
                return null;
            }
        }
    }
}
