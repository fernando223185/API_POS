using Application.Abstractions.Authorization;
using Application.Core.Authorization.Commands;
using Application.Core.Authorization.Queries;
using Application.DTOs.UserPermissions;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Users
{
    /// <summary>
    /// Controlador unificado para gesti�n de permisos
    /// - Permisos por ROL (sistema antiguo: Permission/RolePermission)
    /// - Permisos personalizados por USUARIO (sistema nuevo: UserModulePermission)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IPermissionService _permissionService;
        private readonly POSDbContext _context;

        public PermissionsController(IMediator mediator, IPermissionService permissionService, POSDbContext context)
        {
            _mediator = mediator;
            _permissionService = permissionService;
            _context = context;
        }

        #region ?? PERMISOS PERSONALIZADOS POR USUARIO (Sistema Nuevo - UserModulePermission)

        /// <summary>
        /// ?? Guardar permisos personalizados de usuario por m�dulos/subm�dulos
        /// Sistema NUEVO con control granular (View, Create, Edit, Delete)
        /// </summary>
        [HttpPost("user/save-custom")]
        [RequirePermission("Configuracion", "Edit")]
        public async Task<IActionResult> SaveUserCustomPermissions([FromBody] SaveUserPermissionsRequestDto request)
        {
            try
            {
                var requestingUserId = HttpContext.Items["UserId"] as int? ?? 0;

                if (requestingUserId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                Console.WriteLine($"?? Guardando permisos personalizados para usuario {request.UserId} por usuario {requestingUserId}");

                var command = new SaveUserPermissionsCommand(request, requestingUserId);
                var result = await _mediator.Send(command);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al guardar permisos personalizados: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al guardar permisos personalizados",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ?? Obtener permisos personalizados de usuario (m�dulos/subm�dulos con acciones)
        /// Sistema NUEVO
        /// </summary>
        [HttpGet("user/{userId}/custom")]
        [RequireAuthentication]
        public async Task<IActionResult> GetUserCustomPermissions(int userId)
        {
            try
            {
                var currentUserId = HttpContext.Items["UserId"] as int? ?? 0;
                var roleId = HttpContext.Items["RoleId"] as int? ?? 0;

                // Solo puede ver sus propios permisos o si es administrador
                if (currentUserId != userId && roleId != 1)
                {
                    return Forbid();
                }

                Console.WriteLine($"?? Obteniendo permisos personalizados de usuario {userId}");

                var query = new GetUserPermissionsQuery(userId);
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener permisos personalizados: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener permisos personalizados",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ? Verificar si un usuario tiene un permiso espec�fico en m�dulo/subm�dulo
        /// Sistema NUEVO
        /// </summary>
        [HttpPost("user/check-custom")]
        [RequireAuthentication]
        public async Task<IActionResult> CheckUserCustomPermission([FromBody] CheckUserPermissionRequestDto request)
        {
            try
            {
                var currentUserId = HttpContext.Items["UserId"] as int? ?? 0;

                // Si no especifica userId, usar el del token
                if (request.UserId == 0)
                {
                    request.UserId = currentUserId;
                }

                Console.WriteLine($"?? Verificando permiso personalizado: Usuario={request.UserId}, M�dulo={request.ModuleId}, Subm�dulo={request.SubmoduleId}, Acci�n={request.Action}");

                var query = new CheckUserPermissionQuery(
                    request.UserId,
                    request.ModuleId,
                    request.SubmoduleId,
                    request.Action
                );

                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al verificar permiso personalizado: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al verificar permiso personalizado",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ??? Eliminar todos los permisos personalizados de un usuario
        /// Sistema NUEVO
        /// </summary>
        [HttpDelete("user/{userId}/custom")]
        [RequirePermission("Configuracion", "Delete")]
        public async Task<IActionResult> DeleteUserCustomPermissions(int userId)
        {
            try
            {
                var requestingUserId = HttpContext.Items["UserId"] as int? ?? 0;

                if (requestingUserId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                Console.WriteLine($"??? Eliminando permisos personalizados del usuario {userId}");

                // Guardar array vac�o equivale a eliminar permisos
                var command = new SaveUserPermissionsCommand(
                    new SaveUserPermissionsRequestDto { UserId = userId, Modules = new List<UserModulePermissionItemDto>() },
                    requestingUserId
                );

                await _mediator.Send(command);

                return Ok(new
                {
                    message = "Permisos personalizados eliminados exitosamente",
                    error = 0,
                    userId = userId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al eliminar permisos personalizados: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al eliminar permisos personalizados",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ?? PERMISOS POR ROL (Consultas directas)

        /// <summary>
        /// ?? Obtener todos los permisos de un rol espec�fico (NUEVO SISTEMA)
        /// </summary>
        [HttpGet("role/{roleId}")]
        [RequireAuthentication]
        public async Task<IActionResult> GetRolePermissions(int roleId)
        {
            try
            {
                var role = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Id == roleId);

                if (role == null)
                {
                    return NotFound(new
                    {
                        message = "Rol no encontrado",
                        error = 1
                    });
                }

                // Obtener permisos del nuevo sistema (RoleModulePermissions)
                var permissions = await _context.RoleModulePermissions
                    .Where(rp => rp.RoleId == roleId)
                    .Select(rp => new
                    {
                        id = rp.Id,
                        moduleId = rp.ModuleId,
                        moduleName = rp.Name,
                        submoduleId = rp.SubmoduleId,
                        hasAccess = rp.HasAccess,
                        canView = rp.CanView,
                        canCreate = rp.CanCreate,
                        canEdit = rp.CanEdit,
                        canDelete = rp.CanDelete
                    })
                    .ToListAsync();

                Console.WriteLine($"? Permisos del rol '{role.Name}' obtenidos: {permissions.Count}");

                return Ok(new
                {
                    message = "Permisos del rol obtenidos exitosamente",
                    error = 0,
                    data = new
                    {
                        roleId = role.Id,
                        roleName = role.Name,
                        permissions,
                        totalPermissions = permissions.Count
                    }
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

        #endregion

        #region ?? PERMISOS POR ROL (Sistema Antiguo - ELIMINADO)
        // NOTA: Los endpoints antiguos basados en Permissions/RolePermissions han sido eliminados
        // Usa los nuevos endpoints en RolesController: GET /api/Roles/{id}/module-permissions
        #endregion
    }

    /// <summary>
    /// Request DTO para verificar permisos basados en ROL
    /// </summary>
    public class CheckRolePermissionRequest
    {
        public int UserId { get; set; }
        public string Resource { get; set; }
        public string Action { get; set; }
    }
}
