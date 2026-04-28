using Application.Core.Inventory.Commands;
using Application.Core.Inventory.Queries;
using Application.DTOs.Inventory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Inventory
{
    /// <summary>
    /// Gestión de ajustes de inventario para corregir discrepancias entre stock físico y sistema.
    /// Registra automáticamente movimientos en el Kardex.
    /// </summary>
    [ApiController]
    [Route("api/stock-adjustments")]
    [Authorize]
    public class StockAdjustmentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StockAdjustmentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtener listado paginado de ajustes de inventario con filtros opcionales
        /// </summary>
        [HttpGet]
        //[RequirePermission("Inventario", "View")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? warehouseId = null,
            [FromQuery] string? reason = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 1, 100);

                var companyId = User.FindFirst("CompanyId")?.Value;
                var companyIdParsed = string.IsNullOrEmpty(companyId) ? (int?)null : int.Parse(companyId);

                var query = new GetStockAdjustmentsQuery(
                    warehouseId,
                    reason,
                    fromDate,
                    toDate,
                    companyIdParsed,
                    page,
                    pageSize
                );

                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al obtener ajustes de inventario",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener detalle completo de un ajuste específico
        /// </summary>
        [HttpGet("{id:int}")]
        //[RequirePermission("Inventario", "View")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _mediator.Send(new GetStockAdjustmentByIdQuery(id));
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    message = $"Ajuste de inventario con ID {id} no encontrado",
                    error = 1
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al obtener ajuste de inventario",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Crear un nuevo ajuste de inventario.
        /// Actualiza stock y registra movimientos en kardex automáticamente.
        /// </summary>
        [HttpPost]
        //[RequirePermission("Inventario", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateStockAdjustmentDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { message = "Datos inválidos", error = 1 });

                if (dto.Details == null || dto.Details.Count == 0)
                    return BadRequest(new { message = "El ajuste debe incluir al menos un producto", error = 1 });

                // Validar razón del ajuste
                var validReasons = new[]
                {
                    AdjustmentReason.PHYSICAL_COUNT,
                    AdjustmentReason.DAMAGE,
                    AdjustmentReason.LOSS,
                    AdjustmentReason.EXPIRATION,
                    AdjustmentReason.ERROR,
                    AdjustmentReason.SAMPLE,
                    AdjustmentReason.PRODUCTION_WASTE,
                    AdjustmentReason.OTHER
                };

                if (!validReasons.Contains(dto.Reason))
                    return BadRequest(new { message = "Razón de ajuste inválida", error = 1 });

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var companyId = User.FindFirst("CompanyId")?.Value;
                var companyIdParsed = string.IsNullOrEmpty(companyId) ? (int?)null : int.Parse(companyId);

                var command = new CreateStockAdjustmentCommand(dto, userId, companyIdParsed);
                var result = await _mediator.Send(command);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Id },
                    new
                    {
                        message = $"Ajuste de inventario creado exitosamente. {result.Details.Count} producto(s) ajustado(s).",
                        error = 0,
                        data = result
                    });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al crear ajuste de inventario",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener lista de razones de ajuste disponibles
        /// </summary>
        [HttpGet("reasons")]
        //[RequirePermission("Inventario", "View")]
        public IActionResult GetReasons()
        {
            var reasons = AdjustmentReason.Labels.Select(kvp => new
            {
                code = kvp.Key,
                label = kvp.Value
            }).ToList();

            return Ok(new
            {
                message = "Razones de ajuste obtenidas exitosamente",
                error = 0,
                data = reasons
            });
        }
    }
}
