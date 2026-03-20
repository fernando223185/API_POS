using Application.Abstractions.Authorization;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    /// <summary>
    /// Servicio de permisos - SISTEMA SIMPLIFICADO
    /// Busca directamente en la base de datos sin mapeos legacy
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly POSDbContext _context;

        public PermissionService(POSDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Verifica si un usuario tiene permiso para un m�dulo/subm�dulo con una acci�n espec�fica
        /// Busca directamente en la BD usando los nombres exactos y verifica la acci�n granular
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="moduleName">Nombre del m�dulo (ej: "Clientes", "Productos", "Ventas")</param>
        /// <param name="action">Acci�n o submódulo. Puede ser:
        ///   - Nombre del subm�dulo directo (ej: "Directorio", "Nuevo Cliente")
        ///   - Acci�n granular (ej: "View", "Create", "Edit", "Delete")</param>
        public async Task<bool> HasPermissionAsync(int userId, string moduleName, string action)
        {
            // 1. Obtener usuario con su rol
            var user = await _context.User
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId && u.Active);

            if (user == null || !user.Role.IsActive)
                return false;

            // 2. Buscar el m�dulo por nombre
            var module = await _context.Modules
                .FirstOrDefaultAsync(m => m.Name == moduleName && m.IsActive);

            if (module == null)
            {
                Console.WriteLine($"❌  M�dulo '{moduleName}' no encontrado");
                return false;
            }

            // 3. Determinar si es un subm�dulo espec�fico o una acci�n gen�rica
            // Primero intentar encontrar un subm�dulo con ese nombre exacto
            var submodule = await _context.Submodules
                .FirstOrDefaultAsync(s => s.ModuleId == module.Id && 
                                         s.Name == action && 
                                         s.IsActive);

            // Si encontramos un subm�dulo exacto, verificar solo HasAccess
            if (submodule != null)
            {
                return await CheckModuleSubmoduleAccess(userId, user.RoleId, module.Id, submodule.Id, moduleName, action);
            }

            // Si no es un subm�dulo espec�fico, es una acci�n granular (View/Create/Edit/Delete)
            // Verificar la acci�n en todos los subm�dulos del m�dulo
            Console.WriteLine($"🔍 '{action}' no es un subm�dulo directo, verificando como acci�n granular en '{moduleName}'");
            
            return await CheckActionPermission(userId, user.RoleId, module.Id, action, moduleName);
        }

        /// <summary>
        /// Verifica acceso a un m�dulo/subm�dulo espec�fico
        /// </summary>
        private async Task<bool> CheckModuleSubmoduleAccess(int userId, int roleId, int moduleId, int submoduleId, string moduleName, string submoduleName)
        {
            // Verificar permisos PERSONALIZADOS del usuario (PRIORIDAD ALTA)
            var userPermission = await _context.UserModulePermissions
                .FirstOrDefaultAsync(up => up.UserId == userId &&
                                          up.ModuleId == moduleId &&
                                          up.SubmoduleId == submoduleId &&
                                          up.HasAccess);

            if (userPermission != null)
            {
                Console.WriteLine($"✅ Permiso personalizado encontrado para usuario {userId} en {moduleName}/{submoduleName}");
                return true;
            }

            // Verificar permisos del ROL (PRIORIDAD NORMAL)
            var rolePermission = await _context.RoleModulePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId &&
                                          rp.ModuleId == moduleId &&
                                          rp.SubmoduleId == submoduleId &&
                                          rp.HasAccess);

            if (rolePermission != null)
            {
                Console.WriteLine($"✅ Permiso de rol encontrado para rol {roleId} en {moduleName}/{submoduleName}");
                return true;
            }

            Console.WriteLine($"❌ Sin permisos para usuario {userId} (rol {roleId}) en {moduleName}/{submoduleName}");
            return false;
        }

        /// <summary>
        /// Verifica si el usuario tiene una acci�n espec�fica (View/Create/Edit/Delete) en cualquier subm�dulo del m�dulo
        /// </summary>
        private async Task<bool> CheckActionPermission(int userId, int roleId, int moduleId, string action, string moduleName)
        {
            // Mapear acci�n a campo de BD
            var actionField = action.ToLower() switch
            {
                "view" or "viewlist" or "viewcatalog" or "viewreports" => "CanView",
                "create" => "CanCreate",
                "edit" or "update" => "CanEdit",
                "delete" => "CanDelete",
                _ => null
            };

            if (actionField == null)
            {
                Console.WriteLine($"❌ Acci�n '{action}' no reconocida");
                return false;
            }

            // Verificar permisos PERSONALIZADOS del usuario (PRIORIDAD ALTA)
            var userPermissions = await _context.UserModulePermissions
                .Where(up => up.UserId == userId && up.ModuleId == moduleId && up.HasAccess)
                .ToListAsync();

            foreach (var perm in userPermissions)
            {
                if (HasActionPermission(perm, actionField))
                {
                    Console.WriteLine($"✅ Permiso de acci�n '{action}' encontrado para usuario {userId} en '{moduleName}'");
                    return true;
                }
            }

            // Verificar permisos del ROL (PRIORIDAD NORMAL)
            var rolePermissions = await _context.RoleModulePermissions
                .Where(rp => rp.RoleId == roleId && rp.ModuleId == moduleId && rp.HasAccess)
                .ToListAsync();

            foreach (var perm in rolePermissions)
            {
                if (HasActionPermission(perm, actionField))
                {
                    Console.WriteLine($"✅ Permiso de acci�n '{action}' encontrado para rol {roleId} en '{moduleName}'");
                    return true;
                }
            }

            Console.WriteLine($"❌ Sin permisos de acci�n '{action}' para usuario {userId} (rol {roleId}) en '{moduleName}'");
            return false;
        }

        /// <summary>
        /// Verifica si un permiso tiene una acci�n espec�fica habilitada
        /// </summary>
        private bool HasActionPermission(dynamic permission, string actionField)
        {
            return actionField switch
            {
                "CanView" => permission.CanView,
                "CanCreate" => permission.CanCreate,
                "CanEdit" => permission.CanEdit,
                "CanDelete" => permission.CanDelete,
                _ => false
            };
        }

        /// <summary>
        /// Obtiene todos los permisos de un usuario (combinando rol + personalizados)
        /// Retorna lista de m�dulos y subm�dulos a los que tiene acceso
        /// </summary>
        public async Task<List<string>> GetUserPermissionClaimsAsync(int userId)
        {
            var user = await _context.User
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId && u.Active);

            if (user == null || !user.Role.IsActive)
                return new List<string>();

            var claims = new HashSet<string>();

            // 1. Obtener permisos del rol
            var rolePermissions = await _context.RoleModulePermissions
                .Where(rp => rp.RoleId == user.RoleId && rp.HasAccess)
                .Include(rp => rp.Role)
                .ToListAsync();

            foreach (var rp in rolePermissions)
            {
                // Buscar nombre del m�dulo
                var module = await _context.Modules.FindAsync(rp.ModuleId);
                if (module != null)
                {
                    if (rp.SubmoduleId.HasValue)
                    {
                        // Subm�dulo espec�fico
                        var submodule = await _context.Submodules.FindAsync(rp.SubmoduleId.Value);
                        if (submodule != null)
                        {
                            claims.Add($"{module.Name}/{submodule.Name}");
                        }
                    }
                    else
                    {
                        // M�dulo completo
                        claims.Add($"{module.Name}/*");
                    }
                }
            }

            // 2. Agregar permisos personalizados del usuario
            var userPermissions = await _context.UserModulePermissions
                .Where(up => up.UserId == userId && up.HasAccess)
                .ToListAsync();

            foreach (var up in userPermissions)
            {
                var module = await _context.Modules.FindAsync(up.ModuleId);
                if (module != null)
                {
                    if (up.SubmoduleId.HasValue)
                    {
                        var submodule = await _context.Submodules.FindAsync(up.SubmoduleId.Value);
                        if (submodule != null)
                        {
                            claims.Add($"{module.Name}/{submodule.Name}");
                        }
                    }
                    else
                    {
                        claims.Add($"{module.Name}/*");
                    }
                }
            }

            return claims.ToList();
        }
    }
}