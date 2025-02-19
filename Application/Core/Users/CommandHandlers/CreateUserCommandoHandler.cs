using System.Security.Cryptography;
using Application.Abstractions.Catalogue;
using Application.Core.Users.Commands;
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
				PasswordHash = HashPassword(requets.Password),
				Email = requets.Email,
				Phone = requets.Phone,
				RoleId = requets.RoleId,
				Active = true
			};

			return await _repository.CreateAsync(user);
		}

        static byte[] HashPassword(string password)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, 16, 10000, HashAlgorithmName.SHA256))
            {
                byte[] salt = deriveBytes.Salt; 
                byte[] key = deriveBytes.GetBytes(32); 
                byte[] hash = new byte[salt.Length + key.Length];

                Array.Copy(salt, 0, hash, 0, salt.Length);
                Array.Copy(key, 0, hash, salt.Length, key.Length);

                return hash; 
            }
        }

    }
}

