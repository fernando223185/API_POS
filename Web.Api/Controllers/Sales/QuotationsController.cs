using Application.Core.Quotations.Commands;
using Application.Core.Quotations.Queries;
using Application.DTOs.Quotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Sales
{
    /// <summary>
    /// Gestión de cotizaciones. Al escanear el QR con el código se convierte en venta.
    /// </summary>
    [Route("api/quotations")]
    [ApiController]
    public class QuotationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public QuotationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ============================================================
        // POST /api/quotations
        // ============================================================
        /// <summary>
        /// Crear una nueva cotización en estado Draft
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateQuotation([FromBody] CreateQuotationRequestDto request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                if (userId == 0)
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });

                var command = new CreateQuotationCommand(request, userId);
                var result = await _mediator.Send(command);

                return Ok(new { message = "Cotización creada exitosamente", error = 0, data = result });
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
                return StatusCode(500, new { message = $"Error interno: {ex.Message}", error = 1 });
            }
        }

        // ============================================================
        // GET /api/quotations
        // ============================================================
        /// <summary>
        /// Listar cotizaciones paginadas con filtros opcionales
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetQuotations(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? warehouseId = null,
            [FromQuery] int? customerId = null,
            [FromQuery] int? userId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? status = null)
        {
            try
            {
                var query = new GetQuotationsPagedQuery(
                    Page: page,
                    PageSize: pageSize,
                    WarehouseId: warehouseId,
                    CustomerId: customerId,
                    UserId: userId,
                    FromDate: fromDate,
                    ToDate: toDate,
                    Status: status
                );

                var result = await _mediator.Send(query);
                return Ok(new { error = 0, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno: {ex.Message}", error = 1 });
            }
        }

        // ============================================================
        // GET /api/quotations/{id}
        // ============================================================
        /// <summary>
        /// Obtener cotización por ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetQuotationById(int id)
        {
            try
            {
                var result = await _mediator.Send(new GetQuotationByIdQuery(id));
                return Ok(new { error = 0, data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno: {ex.Message}", error = 1 });
            }
        }

        // ============================================================
        // GET /api/quotations/by-code/{code}
        // ============================================================
        /// <summary>
        /// Obtener cotización por código (resultado de escanear el QR)
        /// </summary>
        [HttpGet("by-code/{code}")]
        public async Task<IActionResult> GetQuotationByCode(string code)
        {
            try
            {
                var result = await _mediator.Send(new GetQuotationByCodeQuery(code));
                return Ok(new { error = 0, data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno: {ex.Message}", error = 1 });
            }
        }

        // ============================================================
        // POST /api/quotations/{id}/convert
        // ============================================================
        /// <summary>
        /// Convertir cotización en venta. Se invoca después de escanear el QR con el código.
        /// La venta creada queda en estado Draft para completar el pago (POS) o la entrega (Delivery).
        /// </summary>
        [HttpPost("{id:int}/convert")]
        public async Task<IActionResult> ConvertToSale(int id, [FromBody] ConvertQuotationToSaleDto request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                if (userId == 0)
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });

                var command = new ConvertQuotationToSaleCommand(id, request, userId);
                var result = await _mediator.Send(command);

                return Ok(result);
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
                return StatusCode(500, new { message = $"Error interno: {ex.Message}", error = 1 });
            }
        }

        // ============================================================
        // DELETE /api/quotations/{id}  (cancel)
        // ============================================================
        /// <summary>
        /// Cancelar una cotización
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> CancelQuotation(int id, [FromQuery] string reason = "Cancelada por usuario")
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                if (userId == 0)
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });

                var command = new CancelQuotationCommand(id, reason, userId);
                var result = await _mediator.Send(command);

                return Ok(new { message = "Cotización cancelada", error = 0, data = result });
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
                return StatusCode(500, new { message = $"Error interno: {ex.Message}", error = 1 });
            }
        }
    }
}
