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
    /// Gestión de conteos cíclicos de inventario.
    /// Permite crear sesiones de conteo, registrar cantidades físicas, detectar discrepancias
    /// y generar ajustes automáticos al completar el conteo.
    /// </summary>
    [ApiController]
    [Route("api/inventory-counts")]
    [Authorize]
    public class InventoryCountsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InventoryCountsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtener listado paginado de conteos con filtros opcionales
        /// </summary>
        [HttpGet]
        //[RequirePermission("Inventario", "View")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? warehouseId = null,
            [FromQuery] string? countType = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? assignedToUserId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 1, 100);

                var companyId = User.FindFirst("CompanyId")?.Value;
                var companyIdParsed = string.IsNullOrEmpty(companyId) ? (int?)null : int.Parse(companyId);

                var query = new GetInventoryCountsQuery(
                    warehouseId,
                    countType,
                    status,
                    fromDate,
                    toDate,
                    assignedToUserId,
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
                    message = "Error al obtener conteos de inventario",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener detalle completo de un conteo específico con todos sus productos
        /// </summary>
        [HttpGet("{id:int}")]
        //[RequirePermission("Inventario", "View")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _mediator.Send(new GetInventoryCountByIdQuery(id));
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    message = $"Conteo de inventario con ID {id} no encontrado",
                    error = 1
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al obtener conteo de inventario",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener productos pendientes de contar en una sesión
        /// </summary>
        [HttpGet("{id:int}/pending")]
        //[RequirePermission("Inventario", "View")]
        public async Task<IActionResult> GetPendingDetails(int id)
        {
            try
            {
                var result = await _mediator.Send(new GetPendingCountDetailsQuery(id));
                return Ok(new
                {
                    message = "Productos pendientes obtenidos exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al obtener productos pendientes",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener productos con discrepancias (variance != 0)
        /// </summary>
        [HttpGet("{id:int}/variances")]
        //[RequirePermission("Inventario", "View")]
        public async Task<IActionResult> GetVariances(int id)
        {
            try
            {
                var result = await _mediator.Send(new GetCountDetailsWithVarianceQuery(id));
                return Ok(new
                {
                    message = "Discrepancias obtenidas exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al obtener discrepancias",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener estadísticas de un conteo
        /// </summary>
        [HttpGet("{id:int}/statistics")]
        //[RequirePermission("Inventario", "View")]
        public async Task<IActionResult> GetStatistics(int id)
        {
            try
            {
                var result = await _mediator.Send(new GetCountStatisticsQuery(id));
                return Ok(new
                {
                    message = "Estadísticas obtenidas exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    message = $"Conteo con ID {id} no encontrado",
                    error = 1
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al obtener estadísticas",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Crear una nueva sesión de conteo (estado: Draft)
        /// </summary>
        [HttpPost]
        //[RequirePermission("Inventario", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateInventoryCountDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { message = "Datos inválidos", error = 1 });

                // Obtener userId del claim
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });

                var userId = int.Parse(userIdClaim);

                // Obtener companyId del claim (puede ser null)
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                var companyIdParsed = string.IsNullOrEmpty(companyIdClaim) ? (int?)null : int.Parse(companyIdClaim);

                var command = new CreateInventoryCountCommand(dto, userId, companyIdParsed);
                var result = await _mediator.Send(command);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Id },
                    new
                    {
                        message = $"Conteo de inventario creado exitosamente. {result.TotalProducts} producto(s) a contar.",
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
                    message = "Error al crear conteo de inventario",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Iniciar un conteo (cambiar de Draft a InProgress)
        /// </summary>
        [HttpPost("{id:int}/start")]
        //[RequirePermission("Inventario", "Update")]
        public async Task<IActionResult> Start(int id, [FromBody] StartInventoryCountDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var command = new StartInventoryCountCommand(id, dto, userId);
                var result = await _mediator.Send(command);

                return Ok(new
                {
                    message = "Conteo iniciado exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Conteo con ID {id} no encontrado", error = 1 });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al iniciar conteo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Actualizar cantidad física de un producto durante el conteo
        /// </summary>
        [HttpPatch("{countId:int}/details/{detailId:int}")]
        //[RequirePermission("Inventario", "Update")]
        public async Task<IActionResult> UpdateDetail(int countId, int detailId, [FromBody] UpdateCountDetailDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { message = "Datos inválidos", error = 1 });

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var command = new UpdateCountDetailCommand(countId, detailId, dto, userId);
                var result = await _mediator.Send(command);

                return Ok(new
                {
                    message = "Producto contado exitosamente",
                    error = 0,
                    data = result
                });
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
                return StatusCode(500, new
                {
                    message = "Error al actualizar producto",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Completar y aprobar un conteo.
        /// Genera ajustes de inventario automáticamente para productos con variación.
        /// </summary>
        [HttpPost("{id:int}/complete")]
        //[RequirePermission("Inventario", "Approve")]
        public async Task<IActionResult> Complete(int id, [FromBody] CompleteInventoryCountDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var command = new CompleteInventoryCountCommand(id, dto, userId);
                var result = await _mediator.Send(command);

                return Ok(new
                {
                    message = $"Conteo completado exitosamente. Se generaron ajustes automáticos para {result.ProductsWithVariance} producto(s).",
                    error = 0,
                    data = result
                });
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
                return StatusCode(500, new
                {
                    message = "Error al completar conteo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Cancelar un conteo
        /// </summary>
        [HttpPost("{id:int}/cancel")]
        //[RequirePermission("Inventario", "Delete")]
        public async Task<IActionResult> Cancel(int id, [FromBody] CancelCountDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var command = new CancelInventoryCountCommand(id, dto?.Reason, userId);
                await _mediator.Send(command);

                return Ok(new
                {
                    message = "Conteo cancelado exitosamente",
                    error = 0
                });
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
                return StatusCode(500, new
                {
                    message = "Error al cancelar conteo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener tipos de conteo disponibles
        /// </summary>
        [HttpGet("types")]
        //[RequirePermission("Inventario", "View")]
        public IActionResult GetCountTypes()
        {
            var types = CountType.Labels.Select(kvp => new
            {
                code = kvp.Key,
                label = kvp.Value
            }).ToList();

            return Ok(new
            {
                message = "Tipos de conteo obtenidos exitosamente",
                error = 0,
                data = types
            });
        }

        /// <summary>
        /// Obtener estados de conteo disponibles
        /// </summary>
        [HttpGet("statuses")]
        //[RequirePermission("Inventario", "View")]
        public IActionResult GetStatuses()
        {
            var statuses = CountStatus.Labels.Select(kvp => new
            {
                code = kvp.Key,
                label = kvp.Value
            }).ToList();

            return Ok(new
            {
                message = "Estados obtenidos exitosamente",
                error = 0,
                data = statuses
            });
        }
    }

    /// <summary>
    /// DTO para cancelar un conteo
    /// </summary>
    public class CancelCountDto
    {
        public string? Reason { get; set; }
    }
}
