using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Test
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult GetPublicData()
        {
            return Ok(new { message = "This is public data", timestamp = DateTime.UtcNow });
        }

        [HttpGet("protected")]
        [RequireAuthentication]
        public IActionResult GetProtectedData()
        {
            var userId = HttpContext.Items["UserId"];
            var userName = HttpContext.Items["UserName"];
            var roleName = HttpContext.Items["RoleName"];

            return Ok(new { 
                message = "This is protected data - you are authenticated!", 
                userId = userId,
                userName = userName,
                roleName = roleName,
                timestamp = DateTime.UtcNow 
            });
        }

        [HttpGet("admin-only")]
        [RequirePermission("Configuracion", "View")]
        public IActionResult GetAdminData()
        {
            var userId = HttpContext.Items["UserId"];
            var userName = HttpContext.Items["UserName"];

            return Ok(new { 
                message = "This is admin-only data - you have permissions!", 
                userId = userId,
                userName = userName,
                timestamp = DateTime.UtcNow 
            });
        }

        [HttpGet("dashboard")]
        [RequirePermission("Dashboard", "View")]
        public IActionResult GetDashboardData()
        {
            return Ok(new { 
                message = "Welcome to dashboard!", 
                data = new {
                    totalSales = 12500.75,
                    totalProducts = 1543,
                    lowStockAlerts = 8,
                    pendingOrders = 23
                },
                timestamp = DateTime.UtcNow 
            });
        }
    }
}