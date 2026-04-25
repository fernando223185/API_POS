using Application.Core.WarehouseTransfer.Commands;
using Application.Core.WarehouseTransfer.Queries;
using Application.DTOs.Inventory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Inventory
{
    /// <summary>
    /// Módulo de Traspasos de almacén con soporte de entrada parcial o completa.
    /// 
    /// Flujo:
    ///   1. POST /                  → Crear orden (Draft)
    ///   2. PUT  /{id}              → Actualizar orden (solo Draft)
    ///   3. POST /{id}/dispatch     → Despachar mercancía desde almacén origen (OUT)
    ///   4. POST /{id}/receivings   → Registrar entrada en almacén destino (IN, parcial o completa)
    ///   5. POST /{id}/cancel       → Cancelar (solo Draft)
    /// </summary>
    [ApiController]
    [Route("api/warehouse-transfers")]
    [Authorize]
    public class WarehouseTransfersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WarehouseTransfersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ─── CONSULTAS ────────────────────────────────────────────────────────────

        /// <summary>
        /// Lista paginada de órdenes de traspaso con filtros opcionales.
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

                var result = await _mediator.Send(new GetWarehouseTransfersQuery(
                    page, pageSize, search,
                    sourceWarehouseId, destinationWarehouseId,
                    status, companyId));

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, error = 1 });
            }
        }

        /// <summary>
        /// Obtiene una orden de traspaso por ID con todos sus detalles y entradas registradas.
        /// </summary>
        [HttpGet("{id:int}")]
        [RequirePermission("Inventario", "View")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _mediator.Send(new GetWarehouseTransferByIdQuery(id));
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, error = 1 });
            }
        }

        /// <summary>
        /// Obtiene todas las entradas (receivings) de una orden de traspaso.
        /// </summary>
        [HttpGet("{id:int}/receivings")]
        [RequirePermission("Inventario", "View")]
        public async Task<IActionResult> GetReceivings(int id)
        {
            try
            {
                var result = await _mediator.Send(new GetWarehouseTransferReceivingsQuery(id));
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, error = 1 });
            }
        }

        /// <summary>
        /// Obtiene una entrada específica por ID.
        /// </summary>
        [HttpGet("receivings/{receivingId:int}")]
        [RequirePermission("Inventario", "View")]
        public async Task<IActionResult> GetReceivingById(int receivingId)
        {
            try
            {
                var result = await _mediator.Send(new GetWarehouseTransferReceivingByIdQuery(receivingId));
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, error = 1 });
            }
        }

        // ─── COMANDOS ────────────────────────────────────────────────────────────

        /// <summary>
        /// Crea una nueva orden de traspaso en estado Draft.
        /// </summary>
        [HttpPost]
        [RequirePermission("Inventario", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateWarehouseTransferDto dto)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var result = await _mediator.Send(new CreateWarehouseTransferCommand(dto, userId));
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
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

        /// <summary>
        /// Actualiza una orden en estado Draft (productos, cantidades, notas).
        /// </summary>
        [HttpPut("{id:int}")]
        [RequirePermission("Inventario", "Edit")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateWarehouseTransferDto dto)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var result = await _mediator.Send(new UpdateWarehouseTransferCommand(id, dto, userId));
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
                return StatusCode(500, new { message = ex.Message, error = 1 });
            }
        }

        /// <summary>
        /// Cancela una orden en estado Draft.
        /// </summary>
        [HttpPost("{id:int}/cancel")]
        [RequirePermission("Inventario", "Edit")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                await _mediator.Send(new CancelWarehouseTransferCommand(id, userId));
                return Ok(new { message = "Orden de traspaso cancelada correctamente.", error = 0 });
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

        /// <summary>
        /// Confirma la salida de mercancía desde el almacén origen.
        /// Crea movimientos de inventario TRANSFER_OUT, descuenta stock del almacén origen
        /// y avanza el estado a Dispatched.
        /// </summary>
        [HttpPost("{id:int}/dispatch")]
        [RequirePermission("Inventario", "Edit")]
        public async Task<IActionResult> Dispatch(int id, [FromBody] DispatchWarehouseTransferDto dto)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var result = await _mediator.Send(new DispatchWarehouseTransferCommand(id, dto, userId));
                return Ok(new
                {
                    message = $"Mercancía despachada correctamente. {result.TotalMovementsCreated} movimiento(s) de salida creados.",
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
                return StatusCode(500, new { message = ex.Message, error = 1 });
            }
        }

        /// <summary>
        /// Registra una entrada de mercancía en el almacén destino.
        /// Puede ser parcial (solo algunos productos o cantidades menores) o completa.
        /// Crea movimientos de inventario TRANSFER_IN y actualiza el stock del almacén destino.
        /// Si todos los productos quedan completamente recibidos, la orden pasa a estado Received.
        /// </summary>
        [HttpPost("{id:int}/receivings")]
        [RequirePermission("Inventario", "Edit")]
        public async Task<IActionResult> CreateReceiving(int id, [FromBody] CreateWarehouseTransferReceivingDto dto)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var result = await _mediator.Send(new CreateWarehouseTransferReceivingCommand(id, dto, userId));
                return Ok(new
                {
                    message = $"Entrada registrada correctamente ({result.ReceivingType}). " +
                              $"{result.TotalProducts} producto(s), {result.TotalQuantityReceived} unidad(es) recibidas.",
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
                return StatusCode(500, new { message = ex.Message, error = 1 });
            }
        }
    }
}
