using Application.DTOs.Users;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Users
{
    /// <summary>
    /// Controlador para consultar usuarios y roles del sistema
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly POSDbContext _context;

        public UsersController(POSDbContext context)
        {
            _context = context;
        }

        #region ?? USUARIOS

        /// <summary>
        /// ?? Obtener todos los usuarios del sistema
        /// </summary>
        [HttpGet]
        [RequireAuthentication]  // Solo requiere autenticaci�n, no permiso espec�fico
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] string? search = null,
            [FromQuery] int? roleId = null,
            [FromQuery] bool? active = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100)
        {
            try
            {
                var query = _context.User
                    .Include(u => u.Role)
                    .AsQueryable();

                // Filtrar por b�squeda (nombre, c�digo o email) - OPCIONAL
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(u => 
                        u.Name.Contains(search) || 
                        u.Code.Contains(search) ||
                        u.Email.Contains(search));
                }

                // Filtrar por rol - OPCIONAL
                if (roleId.HasValue)
                {
                    query = query.Where(u => u.RoleId == roleId.Value);
                }

                // Filtrar por estado activo/inactivo - OPCIONAL
                if (active.HasValue)
                {
                    query = query.Where(u => u.Active == active.Value);
                }

                // Contar total antes de paginaci�n
                var total = await query.CountAsync();

                // Aplicar paginación
                var users = await query
                    .OrderBy(u => u.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UserResponseDto
                    {
                        Id = u.Id,
                        Code = u.Code,
                        Name = u.Name,
                        Email = u.Email,
                        Phone = u.Phone,
                        RoleId = u.RoleId,
                        RoleName = u.Role.Name,
                        Active = u.Active,
                        CompanyId = u.CompanyId,
                        BranchId = u.BranchId,
                        DefaultWarehouseId = u.DefaultWarehouseId,
                        CanSellFromMultipleWarehouses = u.CanSellFromMultipleWarehouses,
                        CreatedAt = u.CreatedAt,
                        UpdatedAt = u.UpdatedAt
                    })
                    .ToListAsync();

                Console.WriteLine($"? Usuarios obtenidos: {users.Count} de {total}");

                return Ok(new
                {
                    message = "Usuarios obtenidos exitosamente",
                    error = 0,
                    data = new
                    {
                        users,
                        totalUsers = total,
                        totalPages = (int)Math.Ceiling(total / (double)pageSize),
                        currentPage = page,
                        pageSize = pageSize
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener usuarios: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener usuarios",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ?? Obtener un usuario por ID
        /// </summary>
        [HttpGet("{id}")]
        [RequireAuthentication]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _context.User
                    .Include(u => u.Role)
                    .Where(u => u.Id == id)
                    .Select(u => new UserResponseDto
                    {
                        Id = u.Id,
                        Code = u.Code,
                        Name = u.Name,
                        Email = u.Email,
                        Phone = u.Phone,
                        RoleId = u.RoleId,
                        RoleName = u.Role.Name,
                        Active = u.Active,
                        CompanyId = u.CompanyId,
                        BranchId = u.BranchId,
                        DefaultWarehouseId = u.DefaultWarehouseId,
                        CanSellFromMultipleWarehouses = u.CanSellFromMultipleWarehouses,
                        CreatedAt = u.CreatedAt,
                        UpdatedAt = u.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new
                    {
                        message = "Usuario no encontrado",
                        error = 1
                    });
                }

                Console.WriteLine($"? Usuario obtenido: {user.Name} (ID: {user.Id})");

                return Ok(new
                {
                    message = "Usuario obtenido exitosamente",
                    error = 0,
                    data = user
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener usuario: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener usuario",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ?? Obtener estad�sticas de usuarios
        /// </summary>
        [HttpGet("statistics")]
        [RequireAuthentication]
        public async Task<IActionResult> GetUserStatistics()
        {
            try
            {
                var totalUsers = await _context.User.CountAsync();
                var activeUsers = await _context.User.CountAsync(u => u.Active);
                var inactiveUsers = totalUsers - activeUsers;

                var usersByRole = await _context.User
                    .Include(u => u.Role)
                    .GroupBy(u => new { u.RoleId, u.Role.Name })
                    .Select(g => new
                    {
                        roleId = g.Key.RoleId,
                        roleName = g.Key.Name,
                        totalUsers = g.Count(),
                        activeUsers = g.Count(u => u.Active)
                    })
                    .ToListAsync();

                Console.WriteLine($"? Estad�sticas obtenidas: {totalUsers} usuarios totales");

                return Ok(new
                {
                    message = "Estadísticas obtenidas exitosamente",
                    error = 0,
                    data = new
                    {
                        totalUsers,
                        activeUsers,
                        inactiveUsers,
                        usersByRole
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener estadísticas: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener estadísticas",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ✅ Actualizar información de un usuario
        /// </summary>
        [HttpPut("{id}")]
        [RequireAuthentication]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var user = await _context.User.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new
                    {
                        message = "Usuario no encontrado",
                        error = 1
                    });
                }

                // Actualizar solo los campos proporcionados
                if (!string.IsNullOrWhiteSpace(dto.Name))
                    user.Name = dto.Name;

                if (!string.IsNullOrWhiteSpace(dto.Email))
                    user.Email = dto.Email;

                if (dto.Phone != null)
                    user.Phone = dto.Phone;

                if (dto.RoleId.HasValue)
                    user.RoleId = dto.RoleId.Value;

                if (dto.CompanyId.HasValue)
                    user.CompanyId = dto.CompanyId;

                if (dto.BranchId.HasValue)
                    user.BranchId = dto.BranchId;

                if (dto.DefaultWarehouseId.HasValue)
                    user.DefaultWarehouseId = dto.DefaultWarehouseId;

                if (dto.CanSellFromMultipleWarehouses.HasValue)
                    user.CanSellFromMultipleWarehouses = dto.CanSellFromMultipleWarehouses.Value;

                if (dto.Active.HasValue)
                    user.Active = dto.Active.Value;

                // Actualizar contraseña si se proporcionó
                if (!string.IsNullOrWhiteSpace(dto.NewPassword))
                {
                    user.PasswordHash = Application.Common.Security.PasswordHasher.HashPassword(dto.NewPassword);
                }

                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Usuario actualizado: {user.Code}");

                return Ok(new
                {
                    message = "Usuario actualizado exitosamente",
                    error = 0,
                    data = new UserResponseDto
                    {
                        Id = user.Id,
                        Code = user.Code,
                        Name = user.Name,
                        Email = user.Email,
                        Phone = user.Phone,
                        RoleId = user.RoleId,
                        Active = user.Active,
                        CompanyId = user.CompanyId,
                        BranchId = user.BranchId,
                        DefaultWarehouseId = user.DefaultWarehouseId,
                        CanSellFromMultipleWarehouses = user.CanSellFromMultipleWarehouses,
                        UpdatedAt = user.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al actualizar usuario: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al actualizar usuario",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ?? ROLES

        /// <summary>
        /// ?? Obtener todos los roles del sistema
        /// </summary>
        [HttpGet("roles")]
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

                // Contar permisos del nuevo sistema
                var roles = new List<RoleResponseDto>();
                
                var rolesList = await query
                    .OrderBy(r => r.Name)
                    .ToListAsync();

                foreach (var role in rolesList)
                {
                    var totalPermissions = await _context.RoleModulePermissions
                        .CountAsync(rp => rp.RoleId == role.Id);

                    roles.Add(new RoleResponseDto
                    {
                        Id = role.Id,
                        Name = role.Name,
                        Description = role.Description,
                        IsActive = role.IsActive,
                        TotalUsers = role.Users.Count(u => u.Active),
                        TotalPermissions = totalPermissions
                    });
                }

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
        /// ?? Obtener un rol por ID con sus usuarios y permisos
        /// </summary>
        [HttpGet("roles/{id}")]
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

                // Obtener permisos del nuevo sistema
                var permissions = await _context.RoleModulePermissions
                    .Where(rp => rp.RoleId == id)
                    .Select(rp => new
                    {
                        id = rp.Id,
                        moduleId = rp.ModuleId,
                        moduleName = rp.Name,
                        submoduleId = rp.SubmoduleId,
                        canView = rp.CanView,
                        canCreate = rp.CanCreate,
                        canEdit = rp.CanEdit,
                        canDelete = rp.CanDelete
                    })
                    .ToListAsync();

                var roleData = new
                {
                    id = role.Id,
                    name = role.Name,
                    description = role.Description,
                    isActive = role.IsActive,
                    totalUsers = role.Users.Count(u => u.Active),
                    users = role.Users
                        .Where(u => u.Active)
                        .Select(u => new
                        {
                            u.Id,
                            u.Code,
                            u.Name,
                            u.Email
                        })
                        .ToList(),
                    permissions = permissions
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

        /// <summary>
        /// ?? Obtener usuarios de un rol espec�fico
        /// </summary>
        [HttpGet("roles/{roleId}/users")]
        [RequireAuthentication]
        public async Task<IActionResult> GetUsersByRole(int roleId)
        {
            try
            {
                var role = await _context.Roles.FindAsync(roleId);
                if (role == null)
                {
                    return NotFound(new
                    {
                        message = "Rol no encontrado",
                        error = 1
                    });
                }

                var users = await _context.User
                    .Where(u => u.RoleId == roleId && u.Active)
                    .Select(u => new UserResponseDto
                    {
                        Id = u.Id,
                        Code = u.Code,
                        Name = u.Name,
                        Email = u.Email,
                        Phone = u.Phone,
                        RoleId = u.RoleId,
                        RoleName = u.Role.Name,
                        Active = u.Active,
                        CreatedAt = u.CreatedAt,
                        UpdatedAt = u.UpdatedAt
                    })
                    .ToListAsync();

                Console.WriteLine($"? Usuarios del rol '{role.Name}': {users.Count}");

                return Ok(new
                {
                    message = "Usuarios obtenidos exitosamente",
                    error = 0,
                    data = new
                    {
                        roleId,
                        roleName = role.Name,
                        users,
                        totalUsers = users.Count
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener usuarios por rol: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener usuarios",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion
    }
}
