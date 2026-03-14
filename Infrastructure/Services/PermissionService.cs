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
        /// Verifica si un usuario tiene permiso para un módulo/submódulo
        /// Busca directamente en la BD usando los nombres exactos
        /// </summary>
        public async Task<bool> HasPermissionAsync(int userId, string moduleName, string submoduleName)
        {
            // 1. Obtener usuario con su rol
            var user = await _context.User
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId && u.Active);

            if (user == null || !user.Role.IsActive)
                return false;

            // 2. Buscar el módulo por nombre
            var module = await _context.Modules
                .FirstOrDefaultAsync(m => m.Name == moduleName && m.IsActive);

            if (module == null)
            {
                Console.WriteLine($"??  Módulo '{moduleName}' no encontrado");
                return false;
            }

            // 3. Buscar el submódulo por nombre dentro del módulo
            var submodule = await _context.Submodules
                .FirstOrDefaultAsync(s => s.ModuleId == module.Id && 
                                         s.Name == submoduleName && 
                                         s.IsActive);

            if (submodule == null)
            {
                Console.WriteLine($"??  Submódulo '{submoduleName}' no encontrado en módulo '{moduleName}'");
                return false;
            }

            // 4. Verificar permisos PERSONALIZADOS del usuario (PRIORIDAD ALTA)
            var userPermission = await _context.UserModulePermissions
                .FirstOrDefaultAsync(up => up.UserId == userId &&
                                          up.ModuleId == module.Id &&
                                          up.SubmoduleId == submodule.Id &&
                                          up.HasAccess);

            if (userPermission != null)
            {
                Console.WriteLine($"? Permiso personalizado encontrado para usuario {userId} en {moduleName}/{submoduleName}");
                return true; // Usuario tiene permisos personalizados
            }

            // 5. Verificar permisos del ROL (PRIORIDAD NORMAL)
            var rolePermission = await _context.RoleModulePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == user.RoleId &&
                                          rp.ModuleId == module.Id &&
                                          rp.SubmoduleId == submodule.Id &&
                                          rp.HasAccess);

            if (rolePermission != null)
            {
                Console.WriteLine($"? Permiso de rol encontrado para rol {user.RoleId} en {moduleName}/{submoduleName}");
                return true; // Rol tiene permisos
            }

            Console.WriteLine($"? Sin permisos para usuario {userId} (rol {user.RoleId}) en {moduleName}/{submoduleName}");
            return false;
        }

        /// <summary>
        /// Obtiene todos los permisos de un usuario (combinando rol + personalizados)
        /// Retorna lista de módulos y submódulos a los que tiene acceso
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
                // Buscar nombre del módulo
                var module = await _context.Modules.FindAsync(rp.ModuleId);
                if (module != null)
                {
                    if (rp.SubmoduleId.HasValue)
                    {
                        // Submódulo específico
                        var submodule = await _context.Submodules.FindAsync(rp.SubmoduleId.Value);
                        if (submodule != null)
                        {
                            claims.Add($"{module.Name}/{submodule.Name}");
                        }
                    }
                    else
                    {
                        // Módulo completo
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