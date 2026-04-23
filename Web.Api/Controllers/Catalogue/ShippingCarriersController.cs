using Application.Core.ShippingCarrier.Commands;
using Application.Core.ShippingCarrier.Queries;
using Application.DTOs.ShippingCarrier;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Catalogue
{
    /// <summary>
    /// Catálogo de paqueterías / transportistas
    /// </summary>
    [Route("api/shipping-carriers")]
    [ApiController]
    public class ShippingCarriersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ShippingCarriersController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// GET /api/shipping-carriers
        /// Lista todas las paqueterías. Filtra por compañía si se indica.
        /// </summary>
        [HttpGet]
        [RequireAuthentication]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? companyId = null,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                var result = await _mediator.Send(new GetAllShippingCarriersQuery(companyId, includeInactive));
                return Ok(new { data = result, total = result.Count, error = 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, error = 2 });
            }
        }

        /// <summary>
        /// GET /api/shipping-carriers/{id}
        /// </summary>
        [HttpGet("{id:int}")]
        [RequireAuthentication]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _mediator.Send(new GetShippingCarrierByIdQuery(id));
                if (result is null)
                    return NotFound(new { message = $"Paquetería {id} no encontrada", error = 1 });
                return Ok(new { data = result, error = 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, error = 2 });
            }
        }

        /// <summary>
        /// POST /api/shipping-carriers
        /// </summary>
        [HttpPost]
        [RequireAuthentication]
        public async Task<IActionResult> Create([FromBody] CreateShippingCarrierDto dto)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var result = await _mediator.Send(new CreateShippingCarrierCommand(dto, userId));
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { data = result, error = 0 });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, error = 2 });
            }
        }

        /// <summary>
        /// PUT /api/shipping-carriers/{id}
        /// </summary>
        [HttpPut("{id:int}")]
        [RequireAuthentication]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateShippingCarrierDto dto)
        {
            try
            {
                var result = await _mediator.Send(new UpdateShippingCarrierCommand(id, dto));
                return Ok(new { data = result, error = 0 });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, error = 2 });
            }
        }

        /// <summary>
        /// DELETE /api/shipping-carriers/{id}
        /// Desactiva la paquetería (soft delete).
        /// </summary>
        [HttpDelete("{id:int}")]
        [RequireAuthentication]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _mediator.Send(new DeleteShippingCarrierCommand(id));
                return Ok(new { message = "Paquetería desactivada correctamente", error = 0 });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, error = 2 });
            }
        }
    }
}
