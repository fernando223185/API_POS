using Application.Core.CashierShifts.Commands;
using Application.Core.CashierShifts.Queries;
using Application.DTOs.CashierShift;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.CashierShifts
{
    /// <summary>
    /// Controlador para gestión de turnos de cajero (corte de caja)
    /// </summary>
    [Route("api/cashier-shifts")]
    [ApiController]
    public class CashierShiftsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CashierShiftsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Abrir un nuevo turno de cajero
        /// </summary>
        [HttpPost("open")]
        [RequirePermission("Ventas", "Create")]
        public async Task<IActionResult> OpenShift([FromBody] OpenShiftRequestDto request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                Console.WriteLine($"📂 Abriendo turno - Usuario: {userName} ({userId}), " +
                                $"Sucursal: {request.BranchId}, Fondo inicial: ${request.InitialCash:N2}");

                var command = new OpenShiftCommand(request, userId);
                var result = await _mediator.Send(command);

                return Ok(new
                {
                    message = $"Turno {result.Code} abierto exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"❌ Not found: {ex.Message}");
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"⚠️ Validation error: {ex.Message}");
                return BadRequest(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error opening shift: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    message = "Error al abrir turno",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Cerrar un turno de cajero (corte de caja)
        /// Calcula automáticamente el efectivo esperado basado en ventas
        /// </summary>
        [HttpPut("{shiftId}/close")]
        [RequirePermission("Ventas", "Update")]
        public async Task<IActionResult> CloseShift(int shiftId, [FromBody] CloseShiftRequestDto request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                Console.WriteLine($"🔒 Cerrando turno {shiftId} - Usuario: {userName} ({userId}), " +
                                $"Efectivo final: ${request.FinalCash:N2}");

                var command = new CloseShiftCommand(shiftId, request, userId);
                var result = await _mediator.Send(command);

                string differenceMsg = result.Difference switch
                {
                    0 => "Cuadrado",
                    > 0 => $"Sobrante de ${result.Difference:N2}",
                    < 0 => $"Faltante de ${Math.Abs(result.Difference.Value):N2}"
                };

                return Ok(new
                {
                    message = $"Turno {result.Code} cerrado exitosamente. {differenceMsg}",
                    error = 0,
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"❌ Not found: {ex.Message}");
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"⚠️ Validation error: {ex.Message}");
                return BadRequest(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error closing shift: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    message = "Error al cerrar turno",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Cancelar un turno de cajero
        /// Solo se pueden cancelar turnos con estado 'Open'
        /// </summary>
        [HttpPut("{shiftId}/cancel")]
        [RequirePermission("Ventas", "Delete")]
        public async Task<IActionResult> CancelShift(int shiftId, [FromBody] CancelShiftRequestDto request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                Console.WriteLine($"❌ Cancelando turno {shiftId} - Usuario: {userName} ({userId}), " +
                                $"Razón: {request.Reason}");

                var command = new CancelShiftCommand(shiftId, request.Reason, userId);
                var result = await _mediator.Send(command);

                return Ok(new
                {
                    message = $"Turno {result.Code} cancelado exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"❌ Not found: {ex.Message}");
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"⚠️ Validation error: {ex.Message}");
                return BadRequest(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error cancelling shift: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    message = "Error al cancelar turno",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener turno por ID
        /// </summary>
        [HttpGet("{shiftId}")]
        [RequirePermission("Ventas", "View")]
        public async Task<IActionResult> GetShiftById(int shiftId)
        {
            try
            {
                var query = new GetShiftByIdQuery(shiftId);
                var result = await _mediator.Send(query);

                return Ok(new
                {
                    message = "Turno obtenido exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"❌ Not found: {ex.Message}");
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting shift: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener turno",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener turno activo del cajero actual
        /// </summary>
        [HttpGet("active")]
        [RequirePermission("Ventas", "View")]
        public async Task<IActionResult> GetActiveShift([FromQuery] int? branchId = null)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                var query = new GetActiveShiftQuery(userId, branchId);
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return Ok(new
                    {
                        message = "No hay turno activo",
                        error = 0,
                        data = (object?)null
                    });
                }

                return Ok(new
                {
                    message = "Turno activo obtenido exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting active shift: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener turno activo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener listado paginado de turnos con filtros
        /// </summary>
        [HttpGet]
        [RequirePermission("Ventas", "View")]
        public async Task<IActionResult> GetShifts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? cashierId = null,
            [FromQuery] int? warehouseId = null,
            [FromQuery] int? branchId = null,
            [FromQuery] int? companyId = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var query = new GetCashierShiftsQuery(
                    page,
                    pageSize,
                    cashierId,
                    warehouseId,
                    branchId,
                    companyId,
                    status,
                    fromDate,
                    toDate
                );

                var result = await _mediator.Send(query);

                return Ok(new
                {
                    message = "Turnos obtenidos exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting shifts: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener turnos",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener reporte detallado de un turno
        /// Incluye ventas, resumen por método de pago, y flujo de efectivo
        /// </summary>
        [HttpGet("{shiftId}/report")]
        [RequirePermission("Ventas", "View")]
        public async Task<IActionResult> GetShiftReport(int shiftId)
        {
            try
            {
                var query = new GetShiftReportQuery(shiftId);
                var result = await _mediator.Send(query);

                return Ok(new
                {
                    message = "Reporte de turno generado exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"❌ Not found: {ex.Message}");
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generating report: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al generar reporte",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Exportar el corte de caja como PDF
        /// Genera un PDF estructurado con todos los detalles del turno
        /// </summary>
        [HttpGet("{shiftId}/export-pdf")]
        [RequirePermission("Ventas", "View")]
        public async Task<IActionResult> ExportShiftPdf(int shiftId)
        {
            try
            {
                Console.WriteLine($"📄 Generando PDF para turno ID: {shiftId}");

                var query = new GenerateShiftPdfQuery(shiftId);
                var pdfBytes = await _mediator.Send(query);

                // Retornar el PDF como archivo descargable
                return File(pdfBytes, "application/pdf", $"Corte-Caja-{shiftId}-{DateTime.Now:yyyyMMdd-HHmmss}.pdf");
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"❌ Turno no encontrado: {ex.Message}");
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generando PDF: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    message = "Error al generar PDF del corte de caja",
                    error = 2,
                    details = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// DTO para solicitud de cancelación
    /// </summary>
    public class CancelShiftRequestDto
    {
        public string Reason { get; set; } = string.Empty;
    }
}
