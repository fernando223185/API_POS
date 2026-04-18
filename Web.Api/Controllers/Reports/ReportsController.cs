using Application.Core.Reports.Commands;
using Application.Core.Reports.Queries;
using Application.DTOs.Reports;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers.Reports
{
    [Route("api/reports")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportsController(IMediator mediator)
            => _mediator = mediator;

        // ─────────────────────────────────────────────
        // CATÁLOGO DE CAMPOS
        // GET /api/reports/fields/{type}
        // ─────────────────────────────────────────────

        /// <summary>
        /// Retorna todos los campos disponibles para un tipo de reporte.
        /// Usar este endpoint para construir el editor de plantillas en el frontend.
        /// </summary>
        [HttpGet("fields/{type}")]
        public async Task<IActionResult> GetFieldCatalog(string type)
        {
            try
            {
                var result = await _mediator.Send(new GetReportFieldCatalogQuery(type));
                return Ok(new { data = result, error = 0 });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message, error = 1 });
            }
        }

        // ─────────────────────────────────────────────
        // PLANTILLAS — CRUD
        // ─────────────────────────────────────────────

        /// <summary>
        /// Lista las plantillas disponibles para un tipo de reporte.
        /// GET /api/reports/templates?type=Sales&companyId=1
        /// </summary>
        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates([FromQuery] string type, [FromQuery] int? companyId = null)
        {
            try
            {
                var result = await _mediator.Send(new GetReportTemplatesByTypeQuery(type, companyId));
                return Ok(new { data = result, total = result.Count, error = 0 });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, error = 1 });
            }
        }

        /// <summary>
        /// Obtiene una plantilla por ID con sus secciones completas.
        /// GET /api/reports/templates/{id}
        /// </summary>
        [HttpGet("templates/{id:int}")]
        public async Task<IActionResult> GetTemplateById(int id)
        {
            try
            {
                var result = await _mediator.Send(new GetReportTemplateByIdQuery(id));
                if (result is null)
                    return NotFound(new { message = $"Plantilla {id} no encontrada", error = 1 });
                return Ok(new { data = result, error = 0 });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, error = 1 });
            }
        }

        /// <summary>
        /// Crea una nueva plantilla de reporte.
        /// POST /api/reports/templates
        /// </summary>
        [HttpPost("templates")]
        public async Task<IActionResult> CreateTemplate([FromBody] CreateReportTemplateDto request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var companyId = HttpContext.Items["CompanyId"] as int?;
                var result = await _mediator.Send(new CreateReportTemplateCommand(request, userId, companyId));
                return Ok(new { data = result, error = 0 });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, error = 1 });
            }
        }

        /// <summary>
        /// Actualiza una plantilla existente.
        /// PUT /api/reports/templates/{id}
        /// </summary>
        [HttpPut("templates/{id:int}")]
        public async Task<IActionResult> UpdateTemplate(int id, [FromBody] UpdateReportTemplateDto request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var result = await _mediator.Send(new UpdateReportTemplateCommand(id, request, userId));
                return Ok(new { data = result, error = 0 });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, error = 1 });
            }
        }

        /// <summary>
        /// Elimina una plantilla (no se puede eliminar la predeterminada).
        /// DELETE /api/reports/templates/{id}
        /// </summary>
        [HttpDelete("templates/{id:int}")]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                await _mediator.Send(new DeleteReportTemplateCommand(id, userId));
                return Ok(new { message = "Plantilla eliminada", error = 0 });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, error = 1 });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = 1 });
            }
        }

        /// <summary>
        /// Marca una plantilla como predeterminada para su tipo.
        /// POST /api/reports/templates/{id}/set-default
        /// </summary>
        [HttpPost("templates/{id:int}/set-default")]
        public async Task<IActionResult> SetDefault(int id)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                await _mediator.Send(new SetDefaultTemplateCommand(id, userId));
                return Ok(new { message = "Plantilla marcada como predeterminada", error = 0 });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = 1 });
            }
        }

        // ─────────────────────────────────────────────
        // GENERAR PDF
        // POST /api/reports/generate
        // ─────────────────────────────────────────────

        /// <summary>
        /// Genera un PDF usando la plantilla indicada (o la default del tipo).
        /// Retorna el archivo PDF como application/pdf.
        /// </summary>
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateReportDto request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var pdfBytes = await _mediator.Send(new GenerateReportPdfQuery(request, userId));
                return File(pdfBytes, "application/pdf", $"reporte-{DateTime.Now:yyyyMMdd-HHmm}.pdf");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, error = 1 });
            }
        }
    }
}
