using System.Security.Claims;

namespace Web.Api.Middleware
{
    public class JwtUserMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // Extraer información del usuario del token JWT
                var userIdClaim = context.User.FindFirst("userId")?.Value;
                var userCodeClaim = context.User.FindFirst("userCode")?.Value;
                var userNameClaim = context.User.FindFirst("userName")?.Value;
                var roleIdClaim = context.User.FindFirst("roleId")?.Value;
                var roleNameClaim = context.User.FindFirst("roleName")?.Value;

                // Agregar al HttpContext para uso posterior
                if (int.TryParse(userIdClaim, out var userId))
                {
                    context.Items["UserId"] = userId;
                }

                if (int.TryParse(roleIdClaim, out var roleId))
                {
                    context.Items["RoleId"] = roleId;
                }

                context.Items["UserCode"] = userCodeClaim;
                context.Items["UserName"] = userNameClaim;
                context.Items["RoleName"] = roleNameClaim;
            }

            await _next(context);
        }
    }

    // Extension method para facilitar el uso
    public static class JwtUserMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtUser(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtUserMiddleware>();
        }
    }
}