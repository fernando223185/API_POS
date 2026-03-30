using Application.Abstractions.Alerts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers.Alerts
{
    public record UpsertAlertRuleRequest(
        int CompanyId,
        string AlertType,
        int? TargetRoleId,
        bool IsActive
    );

    [ApiController]
    [Route("api/alert-rules")]
    [Authorize]
    public class AlertRulesController : ControllerBase
    {
        private readonly IAlertRuleConfigRepository _ruleRepo;

        public AlertRulesController(IAlertRuleConfigRepository ruleRepo)
        {
            _ruleRepo = ruleRepo;
        }

        /// <summary>
        /// Lista todas las reglas de alerta configuradas para una empresa.
        /// El admin las ve y puede modificar TargetRoleId e IsActive.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRules([FromQuery] int companyId)
        {
            if (companyId <= 0)
                return BadRequest(new { message = "CompanyId requerido." });

            // Auto-crear defaults si la empresa es nueva
            await _ruleRepo.EnsureDefaultsAsync(companyId);

            var rules = await _ruleRepo.GetByCompanyAsync(companyId);

            return Ok(rules.Select(r => new
            {
                r.Id,
                r.AlertType,
                r.Description,
                r.CompanyId,
                r.TargetRoleId,
                TargetRoleName = r.TargetRole?.Name,
                r.IsActive,
                r.UpdatedAt
            }));
        }

        /// <summary>
        /// Crea o actualiza una regla de alerta (upsert).
        /// Solo permite cambiar TargetRoleId e IsActive.
        /// AlertType y Description son definidos por el sistema.
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpsertRule([FromBody] UpsertAlertRuleRequest request)
        {
            var userId = HttpContext.Items["UserId"] as int? ?? 0;
            if (userId == 0)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.AlertType))
                return BadRequest(new { message = "AlertType requerido." });

            if (request.CompanyId <= 0)
                return BadRequest(new { message = "CompanyId requerido." });

            await _ruleRepo.UpsertAsync(
                request.CompanyId,
                request.AlertType,
                request.TargetRoleId,
                request.IsActive,
                userId);

            return Ok(new { message = $"Regla '{request.AlertType}' actualizada correctamente." });
        }
    }
}
