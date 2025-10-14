using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Login;
using Application.Core.Login.Commands;
using Application.Common.Security;
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
            var passwordHash = PasswordHasher.HashPassword(request.Password);

            var user = new User
            {
                Code = request.Code,
                PasswordHash = passwordHash  
            };

            var result = await _loginRepository.Login(user);
            return result;
        }
    }
}
