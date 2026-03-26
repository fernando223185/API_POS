using Application.Abstractions.Catalogue;
using Application.Core.Users.Commands;
using Application.Common.Security;
using MediatR;

namespace Application.Core.Users.CommandHandlers
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, bool>
    {
        private readonly IUserRepository _repository;

        public UpdateUserCommandHandler(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            // Obtener el usuario existente
            var user = await _repository.GetByIdAsync(request.Id);
            if (user == null)
            {
                throw new InvalidOperationException($"Usuario con ID {request.Id} no encontrado");
            }

            // Actualizar solo los campos que se proporcionaron (no null)
            if (!string.IsNullOrWhiteSpace(request.Code))
                user.Code = request.Code;

            if (!string.IsNullOrWhiteSpace(request.Name))
                user.Name = request.Name;

            if (!string.IsNullOrWhiteSpace(request.Email))
                user.Email = request.Email;

            if (request.Phone != null)
                user.Phone = request.Phone;

            if (request.RoleId.HasValue)
                user.RoleId = request.RoleId.Value;

            // Actualizar contraseña solo si se proporcionó
            if (!string.IsNullOrWhiteSpace(request.NewPassword))
            {
                user.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
            }

            // Actualizar CompanyId y BranchId
            if (request.CompanyId.HasValue)
                user.CompanyId = request.CompanyId;

            if (request.BranchId.HasValue)
                user.BranchId = request.BranchId;

            // Actualizar almacén por defecto
            if (request.DefaultWarehouseId.HasValue)
            {
                // Validar que el almacén exista si se especifica un valor mayor a 0
                if (request.DefaultWarehouseId.Value > 0)
                {
                    var warehouseExists = await _repository.WarehouseExistsAsync(request.DefaultWarehouseId.Value);
                    if (!warehouseExists)
                    {
                        throw new InvalidOperationException($"El almacén con ID {request.DefaultWarehouseId.Value} no existe o está inactivo");
                    }
                }
                user.DefaultWarehouseId = request.DefaultWarehouseId;
            }

            if (request.CanSellFromMultipleWarehouses.HasValue)
                user.CanSellFromMultipleWarehouses = request.CanSellFromMultipleWarehouses.Value;

            if (request.Active.HasValue)
                user.Active = request.Active.Value;

            user.UpdatedAt = DateTime.UtcNow;

            var result = await _repository.UpdateAsync(user);
            return result != null;
        }
    }
}
