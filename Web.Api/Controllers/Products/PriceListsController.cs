using Application.Core.PriceList.Commands;
using Application.Core.PriceList.Queries;
using Application.DTOs.PriceList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Products
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        /// Obtener todas las listas de precios
        /// </summary>
        /// <param name="isActive">Filtrar por estado activo/inactivo (opcional)</param>
        [HttpGet]
        [RequirePermission("Product", "ViewCatalog")]
        public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null)
        {
            try
            {
                var query = new GetAllPriceListsQuery { IsActive = isActive };
                var priceLists = await _mediator.Send(query);

                var dtoList = priceLists.Select(p => new PriceListDTO
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
                }).ToList();

                return Ok(new
                {
                    message = "Listas de precios obtenidas exitosamente",
                    error = 0,
                    data = dtoList,
                    total = dtoList.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener listas de precios");
                return StatusCode(500, new
                {
                    message = "Error al obtener listas de precios",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener lista de precios por ID
        /// </summary>
        [HttpGet("{id}")]
        [RequirePermission("Product", "ViewCatalog")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var query = new GetPriceListByIdQuery { Id = id };
                var priceList = await _mediator.Send(query);

                if (priceList == null)
                {
                    return NotFound(new
                    {
                        message = $"Lista de precios con ID {id} no encontrada",
                        error = 1
                    });
                }

                var dto = new PriceListDTO
                {
                    Id = priceList.Id,
                    Name = priceList.Name,
                    Description = priceList.Description,
                    Code = priceList.Code,
                    DefaultDiscountPercentage = priceList.DefaultDiscountPercentage,
                    IsDefault = priceList.IsDefault,
                    IsActive = priceList.IsActive,
                    CreatedAt = priceList.CreatedAt,
                    ValidFrom = priceList.ValidFrom,
                    ValidTo = priceList.ValidTo
                };

                return Ok(new
                {
                    message = "Lista de precios obtenida exitosamente",
                    error = 0,
                    data = dto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener lista de precios con ID {id}");
                return StatusCode(500, new
                {
                    message = "Error al obtener lista de precios",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Crear una nueva lista de precios
        /// </summary>
        [HttpPost]
        [RequirePermission("Product", "ManageCatalog")]
        public async Task<IActionResult> Create([FromBody] CreatePriceListDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Datos inválidos",
                        error = 1,
                        details = ModelState
                    });
                }

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

                var priceList = await _mediator.Send(command);

                var responseDto = new PriceListDTO
                {
                    Id = priceList.Id,
                    Name = priceList.Name,
                    Description = priceList.Description,
                    Code = priceList.Code,
                    DefaultDiscountPercentage = priceList.DefaultDiscountPercentage,
                    IsDefault = priceList.IsDefault,
                    IsActive = priceList.IsActive,
                    CreatedAt = priceList.CreatedAt,
                    ValidFrom = priceList.ValidFrom,
                    ValidTo = priceList.ValidTo
                };

                return CreatedAtAction(nameof(GetById), new { id = priceList.Id }, new
                {
                    message = "Lista de precios creada exitosamente",
                    error = 0,
                    data = responseDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear lista de precios");
                return StatusCode(500, new
                {
                    message = "Error al crear lista de precios",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Actualizar una lista de precios existente
        /// </summary>
        [HttpPut("{id}")]
        [RequirePermission("Product", "ManageCatalog")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePriceListDTO dto)
        {
            try
            {
                if (id != dto.Id)
                {
                    return BadRequest(new
                    {
                        message = "El ID de la ruta no coincide con el ID del cuerpo",
                        error = 1
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Datos inválidos",
                        error = 1,
                        details = ModelState
                    });
                }

                var command = new UpdatePriceListCommand
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Description = dto.Description,
                    Code = dto.Code,
                    DefaultDiscountPercentage = dto.DefaultDiscountPercentage,
                    IsDefault = dto.IsDefault,
                    ValidFrom = dto.ValidFrom,
                    ValidTo = dto.ValidTo
                };

                var priceList = await _mediator.Send(command);

                var responseDto = new PriceListDTO
                {
                    Id = priceList.Id,
                    Name = priceList.Name,
                    Description = priceList.Description,
                    Code = priceList.Code,
                    DefaultDiscountPercentage = priceList.DefaultDiscountPercentage,
                    IsDefault = priceList.IsDefault,
                    IsActive = priceList.IsActive,
                    CreatedAt = priceList.CreatedAt,
                    ValidFrom = priceList.ValidFrom,
                    ValidTo = priceList.ValidTo
                };

                return Ok(new
                {
                    message = "Lista de precios actualizada exitosamente",
                    error = 0,
                    data = responseDto
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message,
                    error = 1
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar lista de precios con ID {id}");
                return StatusCode(500, new
                {
                    message = "Error al actualizar lista de precios",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Eliminar (desactivar) una lista de precios
        /// </summary>
        [HttpDelete("{id}")]
        [RequirePermission("Product", "ManageCatalog")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var command = new DeletePriceListCommand { Id = id };
                var result = await _mediator.Send(command);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = $"Lista de precios con ID {id} no encontrada",
                        error = 1
                    });
                }

                return Ok(new
                {
                    message = "Lista de precios eliminada (desactivada) exitosamente",
                    error = 0
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message,
                    error = 1
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar lista de precios con ID {id}");
                return StatusCode(500, new
                {
                    message = "Error al eliminar lista de precios",
                    error = 2,
                    details = ex.Message
                });
            }
        }
    }
}
