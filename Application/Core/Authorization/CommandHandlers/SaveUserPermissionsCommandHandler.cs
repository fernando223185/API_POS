using Application.Abstractions.Authorization;
using Application.Core.Authorization.Commands;
using Application.DTOs.UserPermissions;
using Domain.Entities;
using MediatR;

namespace Application.Core.Authorization.CommandHandlers
{
    public class SaveUserPermissionsCommandHandler : IRequestHandler<SaveUserPermissionsCommand, UserPermissionsResponseDto>
    {
        private readonly IUserPermissionRepository _userPermissionRepository;

        public SaveUserPermissionsCommandHandler(IUserPermissionRepository userPermissionRepository)
        {
            _userPermissionRepository = userPermissionRepository;
        }

        public async Task<UserPermissionsResponseDto> Handle(SaveUserPermissionsCommand request, CancellationToken cancellationToken)
        {
            var permissionsToSave = new List<UserModulePermission>();
            int totalCount = 0;

            Console.WriteLine($"?? Guardando permisos para usuario {request.PermissionsData.UserId}");

            // Procesar módulos y submódulos (FORMATO UNIFICADO)
            foreach (var module in request.PermissionsData.Modules)
            {
                // Guardar permiso del módulo principal (si tiene acceso)
                if (module.HasAccess)
                {
                    permissionsToSave.Add(new UserModulePermission
                    {
                        UserId = request.PermissionsData.UserId,
                        ModuleId = module.ModuleId,
                        SubmoduleId = null,
                        Name = module.ModuleName,
                        Path = string.Empty, // No necesario para módulos
                        Icon = string.Empty,
                        Order = 0,
                        HasAccess = module.HasAccess,
                        CanView = false,  // A nivel módulo no hay acciones
                        CanCreate = false,
                        CanEdit = false,
                        CanDelete = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedByUserId = request.RequestingUserId
                    });
                    totalCount++;

                    Console.WriteLine($"  ?? Módulo: {module.ModuleName} - Acceso: {module.HasAccess}");
                }

                // Guardar permisos de submódulos
                if (module.Submodules != null && module.Submodules.Any())
                {
                    foreach (var submodule in module.Submodules)
                    {
                        if (submodule.HasAccess)
                        {
                            permissionsToSave.Add(new UserModulePermission
                            {
                                UserId = request.PermissionsData.UserId,
                                ModuleId = module.ModuleId,
                                SubmoduleId = submodule.SubmoduleId,
                                Name = submodule.SubmoduleName,
                                Path = string.Empty,
                                Icon = string.Empty,
                                Order = 0,
                                HasAccess = submodule.HasAccess,
                                CanView = submodule.CanView,
                                CanCreate = submodule.CanCreate,
                                CanEdit = submodule.CanEdit,
                                CanDelete = submodule.CanDelete,
                                CreatedAt = DateTime.UtcNow,
                                CreatedByUserId = request.RequestingUserId
                            });
                            totalCount++;

                            Console.WriteLine($"    ?? Submódulo: {submodule.SubmoduleName} - View:{submodule.CanView} Create:{submodule.CanCreate} Edit:{submodule.CanEdit} Delete:{submodule.CanDelete}");
                        }
                    }
                }
            }

            // Guardar en la base de datos
            var saved = await _userPermissionRepository.SaveUserPermissionsAsync(
                request.PermissionsData.UserId, 
                permissionsToSave
            );

            if (!saved)
            {
                throw new InvalidOperationException("Error al guardar permisos en la base de datos");
            }

            Console.WriteLine($"? Se guardaron {totalCount} permisos exitosamente para usuario {request.PermissionsData.UserId}");

            return new UserPermissionsResponseDto
            {
                Message = "Permisos guardados exitosamente",
                Error = 0,
                UserId = request.PermissionsData.UserId,
                TotalPermissionsSaved = totalCount,
                SavedAt = DateTime.UtcNow
            };
        }
    }
}
