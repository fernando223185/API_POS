using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Config
{
    /// <summary>
    /// Controlador para obtener menú del usuario basado en permisos
    /// Sistema UNIFICADO - Usa RoleModulePermissions + UserModulePermissions
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserMenuController : ControllerBase
    {
        private readonly POSDbContext _context;

        public UserMenuController(POSDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// ?? Obtener menú del usuario basado en sus permisos
        /// Combina permisos del ROL + permisos personalizados del USUARIO
        /// </summary>
        [HttpGet("{userId}")]
        [RequireAuthentication]
        public async Task<IActionResult> GetUserMenu(int userId)
        {
            try
            {
                // Obtener el userId real del token JWT
                var currentUserId = HttpContext.Items["UserId"] as int? ?? 0;
                
                // Solo permitir ver el propio menú o si es administrador
                if (currentUserId != userId && !await IsAdministrator())
                {
                    return Forbid();
                }

                // Obtener usuario con su rol
                var user = await _context.User
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == userId && u.Active);

                if (user == null || !user.Role.IsActive)
                {
                    return NotFound(new
                    {
                        message = "Usuario no encontrado o inactivo",
                        error = 1
                    });
                }

                // 1. Obtener permisos del ROL
                var rolePermissions = await _context.RoleModulePermissions
                    .Where(rp => rp.RoleId == user.RoleId && rp.HasAccess)
                    .ToListAsync();

                // 2. Obtener permisos PERSONALIZADOS del usuario
                var userPermissions = await _context.UserModulePermissions
                    .Where(up => up.UserId == userId && up.HasAccess)
                    .ToListAsync();

                // 3. Obtener todos los módulos y submódulos
                var modules = await _context.Modules
                    .Include(m => m.Submodules.Where(s => s.IsActive))
                    .Where(m => m.IsActive)
                    .OrderBy(m => m.Order)
                    .ToListAsync();

                // 4. Construir menú combinando permisos del rol + usuario
                var menuItems = modules.Select(module =>
                {
                    // Verificar si tiene acceso al módulo (rol o usuario)
                    var hasModuleAccessFromRole = rolePermissions.Any(rp => 
                        rp.ModuleId == module.Id && rp.SubmoduleId == null);
                    
                    var hasModuleAccessFromUser = userPermissions.Any(up => 
                        up.ModuleId == module.Id && up.SubmoduleId == null);

                    var hasModuleAccess = hasModuleAccessFromRole || hasModuleAccessFromUser;

                    // Construir submódulos con permisos combinados
                    var submodules = module.Submodules.Select(sub =>
                    {
                        // Permisos del rol para este submódulo
                        var roleSubPermission = rolePermissions.FirstOrDefault(rp => 
                            rp.ModuleId == module.Id && rp.SubmoduleId == sub.Id);

                        // Permisos personalizados del usuario para este submódulo
                        var userSubPermission = userPermissions.FirstOrDefault(up => 
                            up.ModuleId == module.Id && up.SubmoduleId == sub.Id);

                        // Combinar permisos (usuario tiene prioridad)
                        bool canView = userSubPermission?.CanView ?? roleSubPermission?.CanView ?? false;
                        bool canCreate = userSubPermission?.CanCreate ?? roleSubPermission?.CanCreate ?? false;
                        bool canEdit = userSubPermission?.CanEdit ?? roleSubPermission?.CanEdit ?? false;
                        bool canDelete = userSubPermission?.CanDelete ?? roleSubPermission?.CanDelete ?? false;
                        bool hasAccess = userSubPermission?.HasAccess ?? roleSubPermission?.HasAccess ?? false;

                        return new
                        {
                            id = sub.Id,
                            name = sub.Name,
                            description = sub.Description,
                            path = sub.Path,
                            icon = sub.Icon,
                            order = sub.Order,
                            color = sub.Color,
                            hasAccess = hasAccess,
                            permissions = new
                            {
                                canView,
                                canCreate,
                                canEdit,
                                canDelete
                            },
                            source = userSubPermission != null ? "user" : "role" // De dónde vienen los permisos
                        };
                    })
                    .Where(sub => sub.hasAccess) // Solo incluir submódulos con acceso
                    .ToList();

                    return new
                    {
                        id = module.Id,
                        name = module.Name,
                        description = module.Description,
                        path = module.Path,
                        icon = module.Icon,
                        order = module.Order,
                        hasAccess = hasModuleAccess || submodules.Any(), // Tiene acceso si tiene submódulos
                        submodules = submodules
                    };
                })
                .Where(m => m.hasAccess) // Solo incluir módulos con acceso
                .ToList();

                Console.WriteLine($"? Menú generado para usuario {user.Name}: {menuItems.Count} módulos");

                return Ok(new
                {
                    message = "Menú obtenido exitosamente",
                    error = 0,
                    userId = userId,
                    userName = user.Name,
                    roleName = user.Role.Name,
                    menuItems = menuItems,
                    totalModules = menuItems.Count,
                    totalSubmodules = menuItems.Sum(m => ((dynamic)m).submodules.Count)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener menú del usuario: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener menú del usuario",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #region Helper Methods

        private async Task<bool> IsAdministrator()
        {
            var roleId = HttpContext.Items["RoleId"] as int? ?? 0;
            return roleId == 1; // ID del rol Administrador
        }

        #endregion
    }
}
