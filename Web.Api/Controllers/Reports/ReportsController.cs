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
        /// Renderiza un HTML Liquid con datos de ejemplo SIN guardarlo.
        /// Úsalo en el editor de plantillas para obtener el preview en tiempo real
        /// mientras el usuario escribe, antes de hacer clic en "Guardar".
        /// POST /api/reports/templates/live-preview
        /// </summary>
        [HttpPost("templates/live-preview")]
        public async Task<IActionResult> LivePreview([FromBody] LivePreviewRequestDto request)
        {
            try
            {
                var html = await _mediator.Send(new GetLivePreviewHtmlQuery(request.ReportType, request.HtmlTemplate));
                return Content(html, "text/html; charset=utf-8");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, error = 1 });
            }
        }

        /// <summary>
        /// Obtiene la plantilla activa (IsDefault=true, IsActive=true) para un tipo de reporte.
        /// Solo puede existir una plantilla activa por tipo — esta es la que se usa para generar PDFs.
        /// GET /api/reports/templates/active/{type}
        /// </summary>
        [HttpGet("templates/active/{type}")]
        public async Task<IActionResult> GetActiveTemplate(string type, [FromQuery] int? companyId = null)
        {
            try
            {
                var result = await _mediator.Send(new GetActiveTemplateByTypeQuery(type, companyId));
                if (result is null)
                    return NotFound(new { message = $"No hay plantilla activa para el tipo '{type}'", error = 1 });
                return Ok(new { data = result, error = 0 });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, error = 1 });
            }
        }

        /// <summary>
        /// Lista las plantillas guardadas. Si se indica type, filtra por tipo de reporte.
        /// GET /api/reports/templates
        /// GET /api/reports/templates?type=Sales&companyId=1
        /// </summary>
        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates([FromQuery] string? type = null, [FromQuery] int? companyId = null)
        {
            try
            {
                List<Application.DTOs.Reports.ReportTemplateSummaryDto> result = string.IsNullOrWhiteSpace(type)
                    ? await _mediator.Send(new GetAllReportTemplatesQuery(companyId))
                    : await _mediator.Send(new GetReportTemplatesByTypeQuery(type, companyId));

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
        /// Devuelve el esquema de secciones de la plantilla junto con datos de ejemplo,
        /// para renderizar el preview en el frontend sin consultar documentos reales.
        /// GET /api/reports/templates/{id}/preview-data
        /// </summary>
        [HttpGet("templates/{id:int}/preview-data")]
        public async Task<IActionResult> GetPreviewData(int id)
        {
            try
            {
                var result = await _mediator.Send(new GetReportPreviewDataQuery(id));
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
        /// Genera un HTML de vista previa usando datos de ejemplo (mock).
        /// El layout es idéntico al PDF — misma estructura visual, lista para WebView.
        /// GET /api/reports/templates/{id}/preview-html
        /// </summary>
        [HttpGet("templates/{id:int}/preview-html")]
        public async Task<IActionResult> GetPreviewHtml(int id)
        {
            try
            {
                var html = await _mediator.Send(new GetReportTemplateHtmlPreviewQuery(id));
                return Content(html, "text/html; charset=utf-8");
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
        /// Genera un PDF de vista previa usando datos de ejemplo (mock).
        /// El resultado es idéntico al PDF real — misma plantilla, mismo motor QuestPDF.
        /// GET /api/reports/templates/{id}/preview-pdf
        /// </summary>
        [HttpGet("templates/{id:int}/preview-pdf")]
        public async Task<IActionResult> GetPreviewPdf(int id)
        {
            try
            {
                var pdf = await _mediator.Send(new GetReportTemplatePdfPreviewQuery(id));
                return File(pdf, "application/pdf", $"preview_{id}.pdf");
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
