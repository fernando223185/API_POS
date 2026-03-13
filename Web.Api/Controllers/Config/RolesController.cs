using Application.DTOs.Roles;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Config
{
    /// <summary>
    /// Controlador CRUD completo para gestión de roles del sistema
    /// Sistema UNIFICADO con módulos/submódulos
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly POSDbContext _context;

        public RolesController(POSDbContext context)
        {
            _context = context;
        }

        #region ?? CONSULTAS BÁSICAS

        /// <summary>
        /// ?? Obtener todos los roles del sistema
        /// </summary>
        [HttpGet]
        [RequireAuthentication]
        public async Task<IActionResult> GetAllRoles([FromQuery] bool includeInactive = false)
        {
            try
            {
                var query = _context.Roles
                    .Include(r => r.Users)
                    .AsQueryable();

                if (!includeInactive)
                {
                    query = query.Where(r => r.IsActive);
                }

                var roles = await query
                    .OrderBy(r => r.Name)
                    .Select(r => new RoleResponseDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Description = r.Description,
                        IsActive = r.IsActive,
                        TotalUsers = r.Users.Count(u => u.Active),
                        TotalPermissions = 0
                    })
                    .ToListAsync();

                Console.WriteLine($"? Roles obtenidos: {roles.Count}");

                return Ok(new RolesListResponseDto
                {
                    Message = "Roles obtenidos exitosamente",
                    Error = 0,
                    Roles = roles,
                    TotalRoles = roles.Count
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener roles: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener roles",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ?? Obtener un rol por ID
        /// </summary>
        [HttpGet("{id}")]
        [RequireAuthentication]
        public async Task<IActionResult> GetRoleById(int id)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.Users)
                    .Where(r => r.Id == id)
                    .FirstOrDefaultAsync();

                if (role == null)
                {
                    return NotFound(new
                    {
                        message = "Rol no encontrado",
                        error = 1
                    });
                }

                var roleData = new
                {
                    id = role.Id,
                    name = role.Name,
                    description = role.Description,
                    isActive = role.IsActive,
                    totalUsers = role.Users.Count(u => u.Active)
                };

                Console.WriteLine($"? Rol obtenido: {role.Name} (ID: {role.Id})");

                return Ok(new
                {
                    message = "Rol obtenido exitosamente",
                    error = 0,
                    data = roleData
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener rol: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener rol",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ? CREAR

        /// <summary>
        /// ? Crear un nuevo rol
        /// </summary>
        [HttpPost]
        [RequireAuthentication]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            try
            {
                var exists = await _context.Roles.AnyAsync(r => r.Name == dto.Name);
                if (exists)
                {
                    return BadRequest(new
                    {
                        message = $"Ya existe un rol con el nombre '{dto.Name}'",
                        error = 1
                    });
                }

                var role = new Role
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    IsActive = dto.IsActive
                };

                _context.Roles.Add(role);
                await _context.SaveChangesAsync();

                Console.WriteLine($"? Rol creado: {role.Name} (ID: {role.Id})");

                return Ok(new
                {
                    message = "Rol creado exitosamente",
                    error = 0,
                    data = new RoleResponseDto
                    {
                        Id = role.Id,
                        Name = role.Name,
                        Description = role.Description,
                        IsActive = role.IsActive,
                        TotalUsers = 0,
                        TotalPermissions = 0
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al crear rol: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al crear rol",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ?? ACTUALIZAR

        /// <summary>
        /// ?? Actualizar un rol existente
        /// </summary>
        [HttpPut("{id}")]
        [RequireAuthentication]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto dto)
        {
            try
            {
                var role = await _context.Roles.FindAsync(id);
                if (role == null)
                {
                    return NotFound(new
                    {
                        message = "Rol no encontrado",
                        error = 1
                    });
                }

                var nameExists = await _context.Roles
                    .AnyAsync(r => r.Name == dto.Name && r.Id != id);
                
                if (nameExists)
                {
                    return BadRequest(new
                    {
                        message = $"Ya existe otro rol con el nombre '{dto.Name}'",
                        error = 1
                    });
                }

                role.Name = dto.Name;
                role.Description = dto.Description;
                role.IsActive = dto.IsActive;

                await _context.SaveChangesAsync();

                Console.WriteLine($"? Rol actualizado: {role.Name} (ID: {role.Id})");

                return Ok(new
                {
                    message = "Rol actualizado exitosamente",
                    error = 0,
                    data = new RoleResponseDto
                    {
                        Id = role.Id,
                        Name = role.Name,
                        Description = role.Description,
                        IsActive = role.IsActive,
                        TotalUsers = 0,
                        TotalPermissions = 0
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al actualizar rol: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al actualizar rol",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ??? ELIMINAR

        /// <summary>
        /// ??? Eliminar un rol (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [RequireAuthentication]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.Users)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (role == null)
                {
                    return NotFound(new
                    {
                        message = "Rol no encontrado",
                        error = 1
                    });
                }

                var activeUsers = role.Users.Count(u => u.Active);
                if (activeUsers > 0)
                {
                    return BadRequest(new
                    {
                        message = $"No se puede eliminar el rol '{role.Name}' porque tiene {activeUsers} usuario(s) activo(s) asignado(s)",
                        error = 1,
                        activeUsers
                    });
                }

                role.IsActive = false;
                await _context.SaveChangesAsync();

                Console.WriteLine($"??? Rol eliminado (soft): {role.Name} (ID: {role.Id})");

                return Ok(new
                {
                    message = "Rol eliminado exitosamente",
                    error = 0,
                    roleId = id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al eliminar rol: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al eliminar rol",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ?? PERMISOS POR MÓDULOS/SUBMÓDULOS (SISTEMA UNIFICADO)

        /// <summary>
        /// ?? Obtener permisos de un rol por módulos/submódulos
        /// </summary>
        [HttpGet("{id}/module-permissions")]
        [RequireAuthentication]
        public async Task<IActionResult> GetRoleModulePermissions(int id)
        {
            try
            {
                var role = await _context.Roles.FindAsync(id);
                if (role == null)
                {
                    return NotFound(new
                    {
                        message = "Rol no encontrado",
                        error = 1
                    });
                }

                // Obtener todos los módulos y submódulos
                var modules = await _context.Modules
                    .Include(m => m.Submodules.Where(s => s.IsActive))
                    .Where(m => m.IsActive)
                    .OrderBy(m => m.Order)
                    .ToListAsync();

                // Obtener permisos del rol
                var rolePermissions = await _context.RoleModulePermissions
                    .Where(rp => rp.RoleId == id)
                    .ToListAsync();

                // Construir respuesta
                var modulesWithPermissions = modules.Select(module =>
                {
                    var modulePermission = rolePermissions.FirstOrDefault(rp => rp.ModuleId == module.Id && rp.SubmoduleId == null);
                    
                    return new RoleModulePermissionItemDto
                    {
                        ModuleId = module.Id,
                        ModuleName = module.Name,
                        HasAccess = modulePermission?.HasAccess ?? false,
                        Submodules = module.Submodules.Select(sub =>
                        {
                            var subPermission = rolePermissions.FirstOrDefault(rp => rp.ModuleId == module.Id && rp.SubmoduleId == sub.Id);
                            
                            return new RoleSubmodulePermissionItemDto
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

                Console.WriteLine($"? Permisos de módulos obtenidos para rol '{role.Name}': {modulesWithPermissions.Count} módulos");

                return Ok(new RoleModulePermissionsResponseDto
                {
                    Message = "Permisos del rol obtenidos exitosamente",
                    Error = 0,
                    RoleId = role.Id,
                    RoleName = role.Name,
                    Modules = modulesWithPermissions
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener permisos del rol: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener permisos del rol",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ?? Guardar permisos de rol por módulos/submódulos
        /// </summary>
        [HttpPost("{id}/module-permissions")]
        [RequireAuthentication]
        public async Task<IActionResult> SaveRoleModulePermissions(int id, [FromBody] SaveRoleModulePermissionsDto dto)
        {
            try
            {
                var role = await _context.Roles.FindAsync(id);
                if (role == null)
                {
                    return NotFound(new
                    {
                        message = "Rol no encontrado",
                        error = 1
                    });
                }

                var requestingUserId = HttpContext.Items["UserId"] as int? ?? 0;

                Console.WriteLine($"?? Guardando permisos de módulos para rol {role.Name} (ID: {id})");

                // Eliminar permisos existentes del rol
                var existingPermissions = await _context.RoleModulePermissions
                    .Where(rp => rp.RoleId == id)
                    .ToListAsync();

                _context.RoleModulePermissions.RemoveRange(existingPermissions);

                // Insertar nuevos permisos
                var newPermissions = new List<RoleModulePermission>();

                foreach (var module in dto.Modules)
                {
                    var systemModule = await _context.Modules.FindAsync(module.ModuleId);
                    if (systemModule == null) continue;

                    // Permiso a nivel módulo
                    if (module.HasAccess)
                    {
                        newPermissions.Add(new RoleModulePermission
                        {
                            RoleId = id,
                            ModuleId = module.ModuleId,
                            Name = systemModule.Name,
                            Path = systemModule.Path,
                            Icon = systemModule.Icon,
                            Order = systemModule.Order,
                            HasAccess = module.HasAccess,
                            SubmoduleId = null,
                            CanView = false,
                            CanCreate = false,
                            CanEdit = false,
                            CanDelete = false,
                            CreatedAt = DateTime.UtcNow,
                            CreatedByUserId = requestingUserId
                        });
                    }

                    // Permisos a nivel submódulo
                    foreach (var submodule in module.Submodules)
                    {
                        if (submodule.HasAccess)
                        {
                            newPermissions.Add(new RoleModulePermission
                            {
                                RoleId = id,
                                ModuleId = module.ModuleId,
                                Name = systemModule.Name,
                                Path = systemModule.Path,
                                Icon = systemModule.Icon,
                                Order = systemModule.Order,
                                HasAccess = submodule.HasAccess,
                                SubmoduleId = submodule.SubmoduleId,
                                CanView = submodule.CanView,
                                CanCreate = submodule.CanCreate,
                                CanEdit = submodule.CanEdit,
                                CanDelete = submodule.CanDelete,
                                CreatedAt = DateTime.UtcNow,
                                CreatedByUserId = requestingUserId
                            });
                        }
                    }
                }

                await _context.RoleModulePermissions.AddRangeAsync(newPermissions);
                await _context.SaveChangesAsync();

                Console.WriteLine($"? Permisos guardados exitosamente para rol '{role.Name}': {newPermissions.Count} registros");

                return Ok(new
                {
                    message = "Permisos del rol guardados exitosamente",
                    error = 0,
                    roleId = id,
                    roleName = role.Name,
                    totalModules = dto.Modules.Count,
                    totalSubmodules = dto.Modules.SelectMany(m => m.Submodules).Count(),
                    permissionsCreated = newPermissions.Count
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al guardar permisos del rol: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al guardar permisos del rol",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ??? Eliminar todos los permisos de módulos/submódulos de un rol
        /// </summary>
        [HttpDelete("{id}/module-permissions")]
        [RequireAuthentication]
        public async Task<IActionResult> DeleteRoleModulePermissions(int id)
        {
            try
            {
                var role = await _context.Roles.FindAsync(id);
                if (role == null)
                {
                    return NotFound(new
                    {
                        message = "Rol no encontrado",
                        error = 1
                    });
                }

                var permissions = await _context.RoleModulePermissions
                    .Where(rp => rp.RoleId == id)
                    .ToListAsync();

                _context.RoleModulePermissions.RemoveRange(permissions);
                await _context.SaveChangesAsync();

                Console.WriteLine($"??? Permisos de módulos eliminados del rol '{role.Name}': {permissions.Count} registros");

                return Ok(new
                {
                    message = "Permisos del rol eliminados exitosamente",
                    error = 0,
                    roleId = id,
                    removedPermissions = permissions.Count
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al eliminar permisos del rol: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al eliminar permisos del rol",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion
    }
}
