using Application.Abstractions.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Users
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet("user/{userId}")]
        [RequirePermission("Configuration", "ManagePermissions")] // Solo admin puede ver permisos de usuarios
        public async Task<IActionResult> GetUserPermissions(int userId)
        {
            try
            {
                var permissions = await _permissionService.GetUserPermissionsAsync(userId);
                
                return Ok(new 
                { 
                    message = "User permissions retrieved successfully",
                    error = 0,
                    userId = userId,
                    permissions = permissions.Select(p => new {
                        p.Id,
                        p.Name,
                        p.Resource,
                        p.Description,
                        Module = p.Module.Name
                    }).ToArray()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpPost("check")]
        [RequireAuthentication] // Solo requiere estar autenticado
        public async Task<IActionResult> CheckPermission([FromBody] CheckPermissionRequest request)
        {
            try
            {
                // Si no especifica userId, usar el del token JWT
                var userId = request.UserId;
                if (userId == 0)
                {
                    userId = HttpContext.Items["UserId"] as int? ?? 0;
                }

                var hasPermission = await _permissionService.HasPermissionAsync(userId, request.Resource, request.Action);
                
                return Ok(new 
                { 
                    message = "Permission checked successfully",
                    error = 0,
                    userId = userId,
                    resource = request.Resource,
                    action = request.Action,
                    hasPermission = hasPermission
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpGet("my-permissions")]
        [RequireAuthentication] // Ver propios permisos
        public async Task<IActionResult> GetMyPermissions()
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                
                if (userId == 0)
                {
                    return Unauthorized(new { message = "User not found in token", error = 1 });
                }

                var permissions = await _permissionService.GetUserPermissionsAsync(userId);
                
                return Ok(new 
                { 
                    message = "Your permissions retrieved successfully",
                    error = 0,
                    userId = userId,
                    permissions = permissions.Select(p => new {
                        p.Id,
                        p.Name,
                        p.Resource,
                        p.Description,
                        Module = p.Module.Name
                    }).ToArray()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }
    }

    public class CheckPermissionRequest
    {
        public int UserId { get; set; } // Opcional, si es 0 usa el del token
        public string Resource { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
    }
}