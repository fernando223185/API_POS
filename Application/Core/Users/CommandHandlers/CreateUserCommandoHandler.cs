using Application.Abstractions.Catalogue;
using Application.Core.Users.Commands;
using Application.Common.Security;
using Domain.Entities;
using MediatR;

namespace Application.Core.Users.CommandHandlers
{
	public class CreateUserCommandoHandler : IRequestHandler<CreateUserCommand, User>
	{
		private readonly IUserRepository _repository;

		public CreateUserCommandoHandler(IUserRepository repository)
		{
			_repository = repository;
		}

		public async Task<User> Handle(CreateUserCommand requets, CancellationToken cancellationToken)
		{
			var user = new User
			{
				Code = requets.Code,
				Name = requets.Name,
				PasswordHash = PasswordHasher.HashPassword(requets.Password),
				Email = requets.Email,
				Phone = requets.Phone,
				RoleId = requets.RoleId,
				Active = true
			};

			return await _repository.CreateAsync(user);
		}
    }
}

