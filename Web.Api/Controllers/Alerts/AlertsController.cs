using Application.Abstractions.Alerts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers.Alerts
{
    [ApiController]
    [Route("api/alerts")]
    [Authorize]
    public class AlertsController : ControllerBase
    {
        private readonly IAlertRepository _alertRepo;

        public AlertsController(IAlertRepository alertRepo)
        {
            _alertRepo = alertRepo;
        }

        /// <summary>
        /// Contador de alertas Pending para la campanita del frontend.
        /// Consulta ligera — ideal para polling cada 60-120 segundos.
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount([FromQuery] int? companyId)
        {
            var userId = HttpContext.Items["UserId"] as int? ?? 0;
            if (userId == 0) return Unauthorized();

            var roleId = int.TryParse(User.FindFirst("roleId")?.Value, out var rid) ? rid : 0;
            var isAdmin = HttpContext.Items["RoleName"] as string == "Admin";

            var count = await _alertRepo.GetUnreadCountAsync(userId, roleId, companyId, isAdmin);
            return Ok(new { unreadCount = count });
        }

        /// <summary>
        /// Lista de alertas paginadas del usuario actual.
        /// Incluye: alertas directas al usuario, alertas de su rol, y broadcast de empresa.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAlerts(
            [FromQuery] int? companyId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = HttpContext.Items["UserId"] as int? ?? 0;
            if (userId == 0) return Unauthorized();

            var roleId = int.TryParse(User.FindFirst("roleId")?.Value, out var rid) ? rid : 0;
            var isAdmin = HttpContext.Items["RoleName"] as string == "Admin";

            pageSize = Math.Clamp(pageSize, 1, 100);

            var alerts = await _alertRepo.GetByUserAsync(userId, roleId, companyId, page, pageSize, isAdmin);
            return Ok(alerts);
        }

        /// <summary>
        /// Marca una alerta como leída.
        /// </summary>
        [HttpPatch("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = HttpContext.Items["UserId"] as int? ?? 0;
            if (userId == 0)
                return Unauthorized();

            var ok = await _alertRepo.MarkAsReadAsync(id, userId);
            if (!ok)
                return NotFound(new { message = "Alerta no encontrada." });

            return NoContent();
        }

        /// <summary>
        /// Marca una alerta como resuelta manualmente.
        /// </summary>
        [HttpPatch("{id:int}/resolve")]
        public async Task<IActionResult> MarkAsResolved(int id)
        {
            var userId = HttpContext.Items["UserId"] as int? ?? 0;
            if (userId == 0)
                return Unauthorized();

            var ok = await _alertRepo.MarkAsResolvedAsync(id, userId);
            if (!ok)
                return NotFound(new { message = "Alerta no encontrada." });

            return NoContent();
        }
    }
}
