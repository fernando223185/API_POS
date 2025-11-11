using Application.Core.Product.Commands;
using Application.DTOs.Product;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Products
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [RequirePermission("Product", "ViewCatalog")]
        public async Task<IActionResult> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                
                // Aquí implementarías la lógica real
                return Ok(new
                {
                    message = "Products retrieved successfully",
                    error = 0,
                    userId = userId,
                    data = new[] {
                        new { id = 1, name = "Producto 1", code = "PROD001", price = 100.00 },
                        new { id = 2, name = "Producto 2", code = "PROD002", price = 150.00 }
                    },
                    pagination = new { page, pageSize, totalItems = 2 }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [RequirePermission("Product", "ViewCatalog")]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                return Ok(new
                {
                    message = "Product retrieved successfully",
                    error = 0,
                    data = new { id, name = $"Producto {id}", code = $"PROD{id:D3}", price = 100.00 * id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Crear un nuevo producto con toda la información fiscal y de inventario
        /// </summary>
        /// <param name="createProductRequest">Datos del producto a crear</param>
        /// <returns>Producto creado con toda la información</returns>
        [HttpPost]
        [RequirePermission("Product", "Create")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequestDto createProductRequest)
        {
            try
            {
                // Validar modelo
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Datos de entrada inválidos",
                        error = 1,
                        errors = ModelState.SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                // Crear command y enviar a través de MediatR
                var command = new CreateProductCommand(createProductRequest, userId);
                var result = await _mediator.Send(command);

                return Ok(new
                {
                    message = "Producto creado exitosamente",
                    error = 0,
                    data = result,
                    createdBy = userName
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    error = 1
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error interno del servidor",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        [RequirePermission("Product", "Update")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] dynamic productData)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                return Ok(new
                {
                    message = "Product updated successfully",
                    error = 0,
                    productId = id,
                    updatedBy = userName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [RequirePermission("Product", "Delete")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                return Ok(new
                {
                    message = "Product deleted successfully",
                    error = 0,
                    productId = id,
                    deletedBy = userName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }
    }
}