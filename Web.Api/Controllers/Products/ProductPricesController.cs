using Application.Core.ProductPrice.Commands;
using Application.Core.ProductPrice.Queries;
using Application.DTOs.PriceList;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Products
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductPricesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductPricesController> _logger;

        public ProductPricesController(IMediator mediator, ILogger<ProductPricesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Listar precios producto-lista con filtros opcionales.
        /// </summary>
        [HttpGet]
        [RequirePermission("Productos", "View")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? productId = null,
            [FromQuery] int? priceListId = null,
            [FromQuery] bool onlyActive = true)
        {
            try
            {
                var data = await _mediator.Send(new GetProductPricesQuery(productId, priceListId, onlyActive));
                return Ok(new
                {
                    message = "Precios obtenidos exitosamente",
                    error = 0,
                    data,
                    total = data.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener precios");
                return StatusCode(500, new { message = "Error al obtener precios", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Obtener precio por ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [RequirePermission("Productos", "View")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var dto = await _mediator.Send(new GetProductPriceByIdQuery(id));
                if (dto == null)
                    return NotFound(new { message = $"Precio con ID {id} no encontrado", error = 1 });

                return Ok(new { message = "Precio obtenido exitosamente", error = 0, data = dto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener precio {id}");
                return StatusCode(500, new { message = "Error al obtener precio", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Todos los precios de un producto (en todas las listas).
        /// </summary>
        [HttpGet("by-product/{productId:int}")]
        [RequirePermission("Productos", "View")]
        public async Task<IActionResult> GetByProduct(int productId, [FromQuery] bool onlyActive = true)
        {
            try
            {
                var data = await _mediator.Send(new GetPricesByProductQuery(productId, onlyActive));
                return Ok(new { message = "Precios del producto obtenidos exitosamente", error = 0, data, total = data.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener precios del producto {productId}");
                return StatusCode(500, new { message = "Error al obtener precios del producto", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Todos los productos y precios de una lista de precios.
        /// </summary>
        [HttpGet("by-pricelist/{priceListId:int}")]
        [RequirePermission("Productos", "View")]
        public async Task<IActionResult> GetByPriceList(int priceListId, [FromQuery] bool onlyActive = true)
        {
            try
            {
                var data = await _mediator.Send(new GetPricesByPriceListQuery(priceListId, onlyActive));
                return Ok(new { message = "Productos de la lista obtenidos exitosamente", error = 0, data, total = data.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener productos de la lista {priceListId}");
                return StatusCode(500, new { message = "Error al obtener productos de la lista", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Crear o actualizar (upsert) un precio para un par (Producto, Lista).
        /// Si ya existe el par, actualiza el precio existente; si no, lo crea.
        /// </summary>
        [HttpPost]
        [RequirePermission("Productos", "Create")]
        public async Task<IActionResult> Upsert([FromBody] CreateProductPriceDto dto)
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

            var userId = HttpContext.Items["UserId"] as int? ?? 0;
            if (userId == 0)
                return Unauthorized(new { message = "Usuario no autenticado", error = 1 });

            try
            {
                var result = await _mediator.Send(new UpsertProductPriceCommand(dto, userId));
                return Ok(new { message = "Precio guardado exitosamente", error = 0, data = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar precio");
                return StatusCode(500, new { message = "Error al guardar precio", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Asignación masiva: subir varios productos+precios a una sola lista.
        /// Hace upsert por cada par (ProductId, PriceListId).
        /// </summary>
        [HttpPost("bulk")]
        [RequirePermission("Productos", "Create")]
        public async Task<IActionResult> BulkUpsert([FromBody] BulkProductPricesDto dto)
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

            var userId = HttpContext.Items["UserId"] as int? ?? 0;
            if (userId == 0)
                return Unauthorized(new { message = "Usuario no autenticado", error = 1 });

            try
            {
                var result = await _mediator.Send(new BulkUpsertProductPricesCommand(dto, userId));
                return Ok(new
                {
                    message = $"Carga masiva completada: {result.Inserted} insertados, {result.Updated} actualizados, {result.SkippedProductIds.Count} omitidos",
                    error = 0,
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en carga masiva de precios");
                return StatusCode(500, new { message = "Error en carga masiva", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Actualizar un precio por ID.
        /// </summary>
        [HttpPut("{id:int}")]
        [RequirePermission("Productos", "Edit")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductPriceDto dto)
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
                var result = await _mediator.Send(new UpdateProductPriceCommand(id, dto));
                return Ok(new { message = "Precio actualizado exitosamente", error = 0, data = result });
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
                _logger.LogError(ex, $"Error al actualizar precio {id}");
                return StatusCode(500, new { message = "Error al actualizar precio", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Eliminar (soft delete) un precio.
        /// </summary>
        [HttpDelete("{id:int}")]
        [RequirePermission("Productos", "Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var ok = await _mediator.Send(new DeleteProductPriceCommand(id));
                if (!ok)
                    return StatusCode(500, new { message = "No se pudo eliminar el precio", error = 2 });

                return Ok(new { message = "Precio eliminado exitosamente", error = 0, id });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar precio {id}");
                return StatusCode(500, new { message = "Error al eliminar precio", error = 2, details = ex.Message });
            }
        }
    }
}
