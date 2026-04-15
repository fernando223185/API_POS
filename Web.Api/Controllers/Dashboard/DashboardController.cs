using Application.Core.Dashboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Dashboard
{
    /// <summary>
    /// Dashboard dinámico por rol/departamento — cada usuario ve solo sus métricas relevantes
    /// </summary>
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtiene el dashboard personalizado del usuario autenticado.
        /// Las secciones retornadas dependen de los módulos a los que tiene acceso (rol + permisos personales).
        /// </summary>
        /// <param name="companyId">Filtrar por empresa (opcional)</param>
        [HttpGet]
        [RequireAuthentication]
        public async Task<IActionResult> GetDashboard([FromQuery] int? companyId = null)
        {
            var userId = HttpContext.Items["UserId"] as int?;
            if (userId == null || userId == 0)
                return Unauthorized(new { message = "Usuario no autenticado", error = 1 });

            var query = new GetDashboardQuery
            {
                UserId = userId.Value,
                CompanyId = companyId
            };

            var result = await _mediator.Send(query);

            if (result.Error != 0)
                return NotFound(result);

            return Ok(result);
        }
    }
}
