using Application.Core.StockTransfer.Commands;
using Application.Core.StockTransfer.Queries;
using Application.DTOs.Inventory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Inventory
{
    /// <summary>
    /// Módulo de Traspasos directos entre almacenes.
    /// Afecta inventario y kardex en el momento de aplicar.
    /// Flujo: Crear (Draft) → Aplicar (Completed) | Cancelar
    /// </summary>
    [ApiController]
    [Route("api/stock-transfers")]
    [Authorize]
    public class StockTransfersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StockTransfersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Lista paginada de traspasos con filtros opcionales.
        /// </summary>
        [HttpGet]
        [RequirePermission("Inventario", "View")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] int? sourceWarehouseId = null,
            [FromQuery] int? destinationWarehouseId = null,
            [FromQuery] string? status = null,
            [FromQuery] int? companyId = null)
        {
            try
            {
                pageSize = Math.Clamp(pageSize, 1, 100);

                var result = await _mediator.Send(new GetStockTransfersQuery
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchTerm = search,
                    SourceWarehouseId = sourceWarehouseId,
                    DestinationWarehouseId = destinationWarehouseId,
                    Status = status,
                    CompanyId = companyId
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un traspaso por ID con su detalle completo.
        /// </summary>
        [HttpGet("{id:int}")]
        [RequirePermission("Inventario", "View")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _mediator.Send(new GetStockTransferByIdQuery(id));
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Crea un traspaso en estado Draft.
        /// No afecta inventario hasta que se llame al endpoint /apply.
        /// </summary>
        [HttpPost]
        [RequirePermission("Inventario", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateStockTransferDto dto)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                if (userId == 0) return Unauthorized();

                var result = await _mediator.Send(new CreateStockTransferCommand(dto, userId));
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un traspaso en estado Draft (fecha, notas, productos).
        /// </summary>
        [HttpPut("{id:int}")]
        [RequirePermission("Inventario", "Edit")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStockTransferDto dto)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                if (userId == 0) return Unauthorized();

                var result = await _mediator.Send(new UpdateStockTransferCommand(id, dto, userId));
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Aplica el traspaso al inventario y al kardex.
        /// Crea 2 movimientos por producto (OUT origen + IN destino).
        /// Solo disponible en estado Draft. Acción irreversible.
        /// </summary>
        [HttpPost("{id:int}/apply")]
        [RequirePermission("Inventario", "Edit")]
        public async Task<IActionResult> Apply(int id)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                if (userId == 0) return Unauthorized();

                var result = await _mediator.Send(new ApplyStockTransferCommand(id, userId));
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cancela un traspaso en estado Draft.
        /// No afecta inventario (nunca se aplicó).
        /// </summary>
        [HttpPost("{id:int}/cancel")]
        [RequirePermission("Inventario", "Edit")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                if (userId == 0) return Unauthorized();

                await _mediator.Send(new CancelStockTransferCommand(id, userId));
                return Ok(new { message = "Traspaso cancelado correctamente." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
