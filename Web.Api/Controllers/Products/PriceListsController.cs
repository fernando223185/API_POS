using Application.Core.PriceList.Commands;
using Application.Core.PriceList.Queries;
using Application.DTOs.PriceList;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Products
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriceListsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PriceListsController> _logger;

        public PriceListsController(IMediator mediator, ILogger<PriceListsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Listar listas de precios. Por defecto retorna activas e inactivas.
        /// Usa ?isActive=true para sólo activas o ?isActive=false para sólo inactivas.
        /// </summary>
        [HttpGet]
        [RequirePermission("Productos", "View")]
        public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null)
        {
            try
            {
                var priceLists = await _mediator.Send(new GetAllPriceListsQuery { IsActive = isActive });
                var data = priceLists.Select(MapToDto).ToList();

                return Ok(new
                {
                    message = "Listas de precios obtenidas exitosamente",
                    error = 0,
                    data,
                    total = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener listas de precios");
                return StatusCode(500, new { message = "Error al obtener listas de precios", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Obtener lista de precios por ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [RequirePermission("Productos", "View")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var priceList = await _mediator.Send(new GetPriceListByIdQuery { Id = id });
                if (priceList == null)
                    return NotFound(new { message = $"Lista de precios con ID {id} no encontrada", error = 1 });

                return Ok(new { message = "Lista de precios obtenida exitosamente", error = 0, data = MapToDto(priceList) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener lista de precios con ID {id}");
                return StatusCode(500, new { message = "Error al obtener lista de precios", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Obtener la lista de precios predeterminada (IsDefault = true).
        /// </summary>
        [HttpGet("default")]
        [RequirePermission("Productos", "View")]
        public async Task<IActionResult> GetDefault()
        {
            try
            {
                var priceList = await _mediator.Send(new GetDefaultPriceListQuery());
                if (priceList == null)
                    return NotFound(new { message = "No hay lista de precios marcada como predeterminada", error = 1 });

                return Ok(new { message = "Lista predeterminada obtenida exitosamente", error = 0, data = MapToDto(priceList) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lista predeterminada");
                return StatusCode(500, new { message = "Error al obtener lista predeterminada", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Listas de precios activas en formato optimizado para selects/dropdown.
        /// </summary>
        [HttpGet("dropdown")]
        [RequirePermission("Productos", "View")]
        public async Task<IActionResult> GetDropdown()
        {
            try
            {
                var lists = await _mediator.Send(new GetPriceListsDropdownQuery());
                var data = lists.Select(p => new
                {
                    value = p.Id,
                    label = p.Name,
                    code = p.Code,
                    isDefault = p.IsDefault,
                    defaultDiscountPercentage = p.DefaultDiscountPercentage
                }).ToList();

                return Ok(new { message = "Listas para dropdown obtenidas exitosamente", error = 0, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener dropdown");
                return StatusCode(500, new { message = "Error al obtener dropdown", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Crear una nueva lista de precios.
        /// </summary>
        [HttpPost]
        [RequirePermission("Productos", "Create")]
        public async Task<IActionResult> Create([FromBody] CreatePriceListDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Datos inválidos",
                    error = 1,
                    errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                });
            }

            try
            {
                var command = new CreatePriceListCommand
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Code = dto.Code,
                    DefaultDiscountPercentage = dto.DefaultDiscountPercentage,
                    IsDefault = dto.IsDefault,
                    ValidFrom = dto.ValidFrom,
                    ValidTo = dto.ValidTo
                };

                var created = await _mediator.Send(command);

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, new
                {
                    message = "Lista de precios creada exitosamente",
                    error = 0,
                    data = MapToDto(created)
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear lista de precios");
                return StatusCode(500, new { message = "Error al crear lista de precios", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Actualizar una lista de precios. Permite reactivar enviando isActive = true.
        /// </summary>
        [HttpPut("{id:int}")]
        [RequirePermission("Productos", "Edit")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePriceListDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Datos inválidos",
                    error = 1,
                    errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                });
            }

            try
            {
                var command = new UpdatePriceListCommand
                {
                    Id = id,
                    Name = dto.Name,
                    Description = dto.Description,
                    Code = dto.Code,
                    DefaultDiscountPercentage = dto.DefaultDiscountPercentage,
                    IsDefault = dto.IsDefault,
                    IsActive = dto.IsActive,
                    ValidFrom = dto.ValidFrom,
                    ValidTo = dto.ValidTo
                };

                var updated = await _mediator.Send(command);

                return Ok(new { message = "Lista de precios actualizada exitosamente", error = 0, data = MapToDto(updated) });
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
                _logger.LogError(ex, $"Error al actualizar lista de precios con ID {id}");
                return StatusCode(500, new { message = "Error al actualizar lista de precios", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Reactivar (logical undelete) una lista previamente desactivada.
        /// Equivalente a PUT con isActive = true pero más expresivo.
        /// </summary>
        [HttpPost("{id:int}/reactivate")]
        [RequirePermission("Productos", "Edit")]
        public async Task<IActionResult> Reactivate(int id)
        {
            try
            {
                var priceList = await _mediator.Send(new GetPriceListByIdQuery { Id = id });
                if (priceList == null)
                    return NotFound(new { message = $"Lista de precios con ID {id} no encontrada", error = 1 });

                if (priceList.IsActive)
                    return Ok(new { message = "La lista ya está activa", error = 0, data = MapToDto(priceList) });

                var command = new UpdatePriceListCommand
                {
                    Id = priceList.Id,
                    Name = priceList.Name,
                    Description = priceList.Description,
                    Code = priceList.Code,
                    DefaultDiscountPercentage = priceList.DefaultDiscountPercentage,
                    IsDefault = priceList.IsDefault,
                    IsActive = true,
                    ValidFrom = priceList.ValidFrom,
                    ValidTo = priceList.ValidTo
                };

                var updated = await _mediator.Send(command);
                return Ok(new { message = "Lista de precios reactivada exitosamente", error = 0, data = MapToDto(updated) });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al reactivar lista de precios con ID {id}");
                return StatusCode(500, new { message = "Error al reactivar lista de precios", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Eliminar (soft delete: marcar IsActive = false) una lista de precios.
        /// Falla si la lista es la predeterminada o tiene clientes/ventas/cotizaciones/precios activos asociados.
        /// </summary>
        [HttpDelete("{id:int}")]
        [RequirePermission("Productos", "Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _mediator.Send(new DeletePriceListCommand { Id = id });
                if (!result)
                    return NotFound(new { message = $"Lista de precios con ID {id} no encontrada", error = 1 });

                return Ok(new { message = "Lista de precios desactivada exitosamente", error = 0, priceListId = id });
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
                _logger.LogError(ex, $"Error al eliminar lista de precios con ID {id}");
                return StatusCode(500, new { message = "Error al eliminar lista de precios", error = 2, details = ex.Message });
            }
        }

        private static PriceListDTO MapToDto(Domain.Entities.PriceList p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Code = p.Code,
            DefaultDiscountPercentage = p.DefaultDiscountPercentage,
            IsDefault = p.IsDefault,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt,
            ValidFrom = p.ValidFrom,
            ValidTo = p.ValidTo
        };
    }
}
