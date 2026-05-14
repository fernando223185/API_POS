using Application.Core.ProductCategory.Commands;
using Application.Core.ProductCategory.Queries;
using Application.DTOs.Product;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Products
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductCategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtener todas las categorías de productos.
        /// Por defecto retorna solo las activas; usa includeInactive=true para incluir las inactivas.
        /// </summary>
        [HttpGet]
        [RequirePermission("Productos", "View")]
        public async Task<IActionResult> GetCategories([FromQuery] bool includeInactive = false)
        {
            try
            {
                var categories = await _mediator.Send(new GetAllProductCategoriesQuery(includeInactive));

                return Ok(new
                {
                    message = "Categorías obtenidas exitosamente",
                    error = 0,
                    data = categories,
                    totalCategories = categories.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al obtener categorías",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener una categoría por ID con sus subcategorías activas.
        /// </summary>
        [HttpGet("{id:int}")]
        [RequirePermission("Productos", "View")]
        public async Task<IActionResult> GetCategory(int id)
        {
            try
            {
                var category = await _mediator.Send(new GetProductCategoryByIdQuery(id));

                if (category == null)
                {
                    return NotFound(new
                    {
                        message = "Categoría no encontrada",
                        error = 1
                    });
                }

                return Ok(new
                {
                    message = "Categoría obtenida exitosamente",
                    error = 0,
                    data = category
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al obtener categoría",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener categorías en formato optimizado para selects/dropdown del frontend.
        /// </summary>
        [HttpGet("dropdown")]
        [RequirePermission("Productos", "View")]
        public async Task<IActionResult> GetCategoriesForDropdown()
        {
            try
            {
                var categories = await _mediator.Send(new GetProductCategoriesDropdownQuery());

                return Ok(new
                {
                    message = "Categorías para dropdown obtenidas exitosamente",
                    error = 0,
                    data = categories
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al obtener categorías para dropdown",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener estadísticas de categorías (conteo de productos, precio promedio, valor total).
        /// </summary>
        [HttpGet("stats")]
        [RequirePermission("Productos", "View")]
        public async Task<IActionResult> GetCategoryStats()
        {
            try
            {
                var result = await _mediator.Send(new GetProductCategoryStatsQuery());

                return Ok(new
                {
                    message = "Estadísticas de categorías obtenidas exitosamente",
                    error = 0,
                    data = result
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
        /// Crear una nueva categoría de productos.
        /// </summary>
        [HttpPost]
        [RequirePermission("Productos", "Create")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateProductCategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Datos de entrada inválidos",
                    error = 1,
                    errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                });
            }

            try
            {
                var created = await _mediator.Send(new CreateProductCategoryCommand(dto));

                return CreatedAtAction(
                    nameof(GetCategory),
                    new { id = created.Id },
                    new
                    {
                        message = "Categoría creada exitosamente",
                        error = 0,
                        data = created
                    });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al crear categoría",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Actualizar una categoría de productos existente.
        /// </summary>
        [HttpPut("{id:int}")]
        [RequirePermission("Productos", "Edit")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateProductCategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Datos de entrada inválidos",
                    error = 1,
                    errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                });
            }

            try
            {
                var updated = await _mediator.Send(new UpdateProductCategoryCommand(id, dto));

                return Ok(new
                {
                    message = "Categoría actualizada exitosamente",
                    error = 0,
                    data = updated
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
                    message = "Error al actualizar categoría",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Eliminar una categoría (soft delete: marca IsActive = false).
        /// No permite eliminar categorías que tengan productos asociados.
        /// </summary>
        [HttpDelete("{id:int}")]
        [RequirePermission("Productos", "Delete")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var deleted = await _mediator.Send(new DeleteProductCategoryCommand(id));

                if (!deleted)
                {
                    return StatusCode(500, new
                    {
                        message = "No se pudo eliminar la categoría",
                        error = 2
                    });
                }

                return Ok(new
                {
                    message = "Categoría eliminada exitosamente",
                    error = 0,
                    categoryId = id
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
                    message = "Error al eliminar categoría",
                    error = 2,
                    details = ex.Message
                });
            }
        }
    }
}
