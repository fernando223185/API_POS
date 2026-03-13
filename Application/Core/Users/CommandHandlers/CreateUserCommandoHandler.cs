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

		public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
		{
			// ✅ Validar que el almacén exista si se especifica
			if (request.DefaultWarehouseId.HasValue && request.DefaultWarehouseId.Value > 0)
			{
				var warehouseExists = await _repository.WarehouseExistsAsync(request.DefaultWarehouseId.Value);
				if (!warehouseExists)
				{
					throw new InvalidOperationException($"El almacén con ID {request.DefaultWarehouseId.Value} no existe o está inactivo");
				}
			}

			var user = new User
			{
				Code = request.Code,
				Name = request.Name,
				PasswordHash = PasswordHasher.HashPassword(request.Password),
				Email = request.Email,
				Phone = request.Phone,
				RoleId = request.RoleId,
				Active = true,
				// ✅ NUEVO: Asignar almacén y permisos multi-almacén
				DefaultWarehouseId = request.DefaultWarehouseId,
				CanSellFromMultipleWarehouses = request.CanSellFromMultipleWarehouses
			};

			return await _repository.CreateAsync(user);
		}
    }
}

