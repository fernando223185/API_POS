using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Login;
using Application.Core.Login.Commands;
using Domain.Entities;
using MediatR;

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
            Console.WriteLine(request);

            var passwordHash = HashPassword(request.pass);

            var user = new User
            {
                Code = request.Code,
                PasswordHash = passwordHash  
            };

            var result = await _loginRepository.Login(user);
            return result;
        }

        private static byte[] HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }
    }
}
