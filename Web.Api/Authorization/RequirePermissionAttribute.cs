using Application.Abstractions.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Api.Authorization
{
    public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _resource;
        private readonly string _action;

        public RequirePermissionAttribute(string resource, string action)
        {
            _resource = resource;
            _action = action;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Verificar si el usuario está autenticado
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Authentication required", error = 1 });
                return;
            }

            // Obtener userId del token JWT
            var userIdClaim = context.HttpContext.User.FindFirst("userId")?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Invalid token", error = 1 });
                return;
            }

            // Verificar permisos usando un nuevo scope de servicio
            using (var scope = context.HttpContext.RequestServices.CreateScope())
            {
                var permissionService = scope.ServiceProvider.GetService<IPermissionService>();
                if (permissionService != null)
                {
                    try
                    {
                        var hasPermission = await permissionService.HasPermissionAsync(userId, _resource, _action);
                        if (!hasPermission)
                        {
                            context.Result = new ObjectResult(new { message = "Insufficient permissions", error = 1 })
                            {
                                StatusCode = 403
                            };
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        context.Result = new ObjectResult(new { message = "Authorization error", error = 2, details = ex.Message })
                        {
                            StatusCode = 500
                        };
                        return;
                    }
                }
            }
        }
    }

    // Attribute simple para endpoints que solo requieren autenticación
    public class RequireAuthenticationAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Authentication required", error = 1 });
            }
        }
    }
}