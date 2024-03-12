using Application.Abstractions.Login;
using Application.Core.Login.Commands;
using Domain.Entities;
using MediatR;
namespace Application.Core.Login.CommandHandlers
{
	public class LoginCommandHandler : IRequestHandler<LoginCommand, Users>
	{
        private readonly ILoginRepository _loginRepository;

        public LoginCommandHandler(ILoginRepository loginRepository)
        {
            _loginRepository = loginRepository;
        }
        public async Task<Users?> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine(request);
            var user = new Users
            {
                nameUser = request.nameUser,
                pass = request.pass
            };

            var result = await _loginRepository.Login(user);
            if (result == null)
            {
                return null;
            }
            return result;
        }

    }
}

