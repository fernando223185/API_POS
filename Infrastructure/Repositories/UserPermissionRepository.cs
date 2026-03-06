using Application.Abstractions.Authorization;
using Application.DTOs.UserPermissions;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserPermissionRepository : IUserPermissionRepository
    {
        private readonly POSDbContext _context;

        public UserPermissionRepository(POSDbContext context)
        {
            _context = context;
        }

        public async Task<GetUserPermissionsResponseDto> GetUserPermissionsAsync(int userId)
        {
            // Obtener usuario
            var user = await _context.User
                .Where(u => u.Id == userId && u.Active)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new Exception($"Usuario con ID {userId} no encontrado o inactivo");
            }

            // Obtener todos los módulos del sistema
            var systemModules = await _context.SystemModules
                .Include(m => m.Submodules.Where(s => s.IsActive))
                .Where(m => m.IsActive)
                .OrderBy(m => m.Order)
                .ToListAsync();

            // Obtener permisos personalizados del usuario
            var userPermissions = await _context.UserModulePermissions
                .Where(up => up.UserId == userId)
                .ToListAsync();

            // Construir respuesta con formato unificado
            var modules = systemModules.Select(module =>
            {
                var modulePermission = userPermissions.FirstOrDefault(up => up.ModuleId == module.Id && up.SubmoduleId == null);

                return new UserModulePermissionItemDto
                {
                    ModuleId = module.Id,
                    ModuleName = module.Name,
                    HasAccess = modulePermission?.HasAccess ?? false,
                    Submodules = module.Submodules.Select(sub =>
                    {
                        var subPermission = userPermissions.FirstOrDefault(up => up.ModuleId == module.Id && up.SubmoduleId == sub.Id);

                        return new UserSubmodulePermissionItemDto
                        {
                            SubmoduleId = sub.Id,
                            SubmoduleName = sub.Name,
                            HasAccess = subPermission?.HasAccess ?? false,
                            CanView = subPermission?.CanView ?? false,
                            CanCreate = subPermission?.CanCreate ?? false,
                            CanEdit = subPermission?.CanEdit ?? false,
                            CanDelete = subPermission?.CanDelete ?? false
                        };
                    }).ToList()
                };
            }).ToList();

            Console.WriteLine($"? Permisos personalizados obtenidos para usuario {user.Name}: {modules.Count} módulos");

            return new GetUserPermissionsResponseDto
            {
                Message = "Permisos obtenidos exitosamente",
                Error = 0,
                UserId = user.Id,
                UserName = user.Name,
                Modules = modules,
                TotalModules = modules.Count,
                TotalSubmodules = modules.Sum(m => m.Submodules.Count)
            };
        }

        public async Task<List<UserModulePermission>> GetUserPermissionsEntitiesAsync(int userId)
        {
            return await _context.UserModulePermissions
                .Where(up => up.UserId == userId)
                .OrderBy(up => up.ModuleId)
                .ThenBy(up => up.SubmoduleId)
                .ToListAsync();
        }

        public async Task<bool> SaveUserPermissionsAsync(int userId, List<UserModulePermission> permissions)
        {
            try
            {
                // Eliminar permisos existentes del usuario
                var existingPermissions = await _context.UserModulePermissions
                    .Where(up => up.UserId == userId)
                    .ToListAsync();

                if (existingPermissions.Any())
                {
                    _context.UserModulePermissions.RemoveRange(existingPermissions);
                }

                // Agregar nuevos permisos
                if (permissions.Any())
                {
                    await _context.UserModulePermissions.AddRangeAsync(permissions);
                }

                await _context.SaveChangesAsync();

                Console.WriteLine($"? Permisos guardados para usuario {userId}: {permissions.Count} registros");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al guardar permisos del usuario {userId}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteUserPermissionsAsync(int userId)
        {
            try
            {
                var permissions = await _context.UserModulePermissions
                    .Where(up => up.UserId == userId)
                    .ToListAsync();

                if (permissions.Any())
                {
                    _context.UserModulePermissions.RemoveRange(permissions);
                    await _context.SaveChangesAsync();
                }

                Console.WriteLine($"? Permisos eliminados para usuario {userId}: {permissions.Count} registros");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al eliminar permisos del usuario {userId}: {ex.Message}");
                return false;
            }
        }

        public async Task<UserModulePermission?> GetSpecificPermissionAsync(int userId, int moduleId, int? submoduleId)
        {
            return await _context.UserModulePermissions
                .FirstOrDefaultAsync(up =>
                    up.UserId == userId &&
                    up.ModuleId == moduleId &&
                    up.SubmoduleId == submoduleId);
        }

        public async Task<bool> HasPermissionAsync(int userId, int moduleId, int? submoduleId, string action)
        {
            var permission = await GetSpecificPermissionAsync(userId, moduleId, submoduleId);

            if (permission == null || !permission.HasAccess)
            {
                return false;
            }

            return action.ToLower() switch
            {
                "view" => permission.CanView,
                "create" => permission.CanCreate,
                "edit" => permission.CanEdit,
                "delete" => permission.CanDelete,
                _ => false
            };
        }

        public async Task<Dictionary<int, List<UserModulePermission>>> GetPermissionsByModuleAsync(int userId)
        {
            var permissions = await _context.UserModulePermissions
                .Where(up => up.UserId == userId)
                .ToListAsync();

            return permissions
                .GroupBy(up => up.ModuleId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }
    }
}
