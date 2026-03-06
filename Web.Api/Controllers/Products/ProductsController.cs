using Application.Core.Product.Commands;
using Application.Core.Product.Queries;
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

        /// <summary>
        /// ? OBTENER PRODUCTOS CON CONSULTA REAL A BASE DE DATOS
        /// Endpoint mejorado con paginación, filtros, estadísticas y datos reales
        /// </summary>
        [HttpGet]
        [RequirePermission("Product", "ViewCatalog")]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 50,
            [FromQuery] string? search = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] bool? isActive = true,
            [FromQuery] string? sortBy = "name",
            [FromQuery] string? sortOrder = "asc")
        {
            try
            {
                Console.WriteLine($"?? Getting products - Page: {page}, PageSize: {pageSize}, Search: '{search}'");

                var userId = HttpContext.Items["UserId"] as int? ?? 0;

                // Validar parámetros
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;

                // Crear query para MediatR
                var query = new GetProductByPageQuery
                {
                    Page = page,
                    PageSize = pageSize,
                    Search = search,
                    CategoryId = categoryId,
                    IsActive = isActive,
                    SortBy = sortBy,
                    SortOrder = sortOrder
                };

                // Ejecutar query usando MediatR
                var result = await _mediator.Send(query);

                Console.WriteLine($"? Products retrieved: {result.Data.Count} items, Total: {result.Pagination.TotalItems}");

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error getting products: {ex.Message}");
                return StatusCode(500, new GetProductsPagedResponseDto
                {
                    Message = "Error al obtener productos de la base de datos",
                    Error = 2,
                    Data = new List<ProductTableDto>(),
                    Pagination = new PaginationMetadata
                    {
                        Page = page,
                        PageSize = pageSize,
                        TotalItems = 0
                    },
                    Statistics = new ProductsStatistics()
                });
            }
        }

        /// <summary>
        /// Obtener producto por ID desde base de datos
        /// </summary>
        [HttpGet("{id}")]
        [RequirePermission("Product", "ViewCatalog")]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                Console.WriteLine($"?? Getting product by ID: {id}");

                // Crear query para obtener por ID
                var query = new GetProductByIdQuery { ID = id }; // ? CORREGIDO: usar ID en lugar de Id
                var product = await _mediator.Send(query);

                if (product == null)
                {
                    return NotFound(new
                    {
                        message = "Producto no encontrado",
                        error = 1,
                        productId = id
                    });
                }

                Console.WriteLine($"? Product found: {product.name} (Code: {product.code})");

                return Ok(new
                {
                    message = "Producto obtenido exitosamente",
                    error = 0,
                    data = new 
                    { 
                        id = product.ID,
                        name = product.name,
                        code = product.code,
                        price = product.price,
                        baseCost = product.BaseCost,
                        brand = product.Brand,
                        model = product.Model,
                        category = product.Category?.Name,
                        subcategory = product.Subcategory?.Name,
                        description = product.description,
                        barcode = product.barcode,
                        minimumStock = product.MinimumStock,
                        maximumStock = product.MaximumStock,
                        unit = product.Unit,
                        isActive = product.IsActive,
                        isService = product.IsService,
                        createdAt = product.CreatedAt,
                        createdBy = product.CreatedBy?.Name,
                        location = product.Location,
                        satCode = product.SatCode,
                        satUnit = product.SatUnit,
                        priceWithTax = product.price * (1 + product.TaxRate)
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error getting product {id}: {ex.Message}");
                return StatusCode(500, new { 
                    message = "Error al obtener producto de la base de datos", 
                    error = 2, 
                    details = ex.Message,
                    productId = id
                });
            }
        }

        /// <summary>
        /// ? CREAR PRODUCTO COMPLETO CON TODOS LOS CAMPOS AVANZADOS
        /// Endpoint principal para crear productos con información fiscal, inventario, marketing, etc.
        /// </summary>
        /// <param name="createProductRequest">Datos completos del producto</param>
        /// <returns>Producto creado con toda la información</returns>
        [HttpPost]
        [RequirePermission("Product", "Create")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequestDto createProductRequest)
        {
            try
            {
                Console.WriteLine($"??? Creating product: {createProductRequest.Name}");

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

                // Obtener información del usuario del token
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                if (userId == 0)
                {
                    return Unauthorized(new { 
                        message = "Usuario no autenticado", 
                        error = 1 
                    });
                }

                // Crear command y enviar a través de MediatR
                var command = new CreateProductCommand(createProductRequest, userId);
                var result = await _mediator.Send(command);

                Console.WriteLine($"? Product created successfully with ID: {result.ID}");

                return Ok(new
                {
                    message = "Producto creado exitosamente",
                    error = 0,
                    data = result,
                    createdBy = userName,
                    summary = new
                    {
                        productId = result.ID,
                        productCode = result.Code,
                        productName = result.Name,
                        price = result.Price,
                        priceWithTax = result.PriceWithTax,
                        profitMargin = result.Price > 0 && result.BaseCost > 0 
                            ? Math.Round(((result.Price - result.BaseCost) / result.Price) * 100, 2) 
                            : 0,
                        category = result.CategoryName ?? "Sin categoría",
                        isActive = result.IsActive,
                        isService = result.IsService,
                        trackSerial = result.TrackSerial,
                        trackExpiry = result.TrackExpiry
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"? Business logic error: {ex.Message}");
                return BadRequest(new
                {
                    message = ex.Message,
                    error = 1
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error creating product: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    message = "Error interno del servidor al crear producto",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Crear producto básico (endpoint simplificado para testing)
        /// </summary>
        [HttpPost("basic")]
        [RequirePermission("Product", "Create")]
        public async Task<IActionResult> CreateBasicProduct([FromBody] BasicProductDto basicProduct)
        {
            try
            {
                Console.WriteLine($"??? Creating basic product: {basicProduct.Name}");

                // Mapear datos básicos a CreateProductRequestDto completo
                var createRequest = new CreateProductRequestDto
                {
                    Name = basicProduct.Name,
                    Description = basicProduct.Description,
                    Code = basicProduct.Code,
                    Price = basicProduct.Price,
                    BaseCost = basicProduct.BaseCost,
                    Brand = basicProduct.Brand,
                    MinimumStock = basicProduct.MinimumStock,
                    MaximumStock = basicProduct.MaximumStock,
                    
                    // Valores por defecto para campos avanzados
                    SatCode = "01010101",
                    SatUnit = "PZA",
                    SatTaxType = "Tasa",
                    CountryOfOrigin = "México",
                    TaxRate = 0.16m,
                    Unit = "PZA",
                    IsActive = true,
                    IsService = false,
                    IsWebVisible = true,
                    IsDiscountAllowed = true,
                    MinQuantityPerSale = 1
                };

                return await CreateProduct(createRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error creating basic product: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al crear producto básico",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Búsqueda optimizada de productos
        /// </summary>
        [HttpGet("search")]
        [RequirePermission("Product", "ViewCatalog")]
        public async Task<IActionResult> SearchProducts(
            [FromQuery] string term, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20,
            [FromQuery] int? categoryId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                {
                    return BadRequest(new { 
                        message = "Término de búsqueda requerido", 
                        error = 1 
                    });
                }

                Console.WriteLine($"?? Searching products: '{term}' in category {categoryId}");

                // Usar el mismo endpoint de GetProducts con parámetros de búsqueda
                return await GetProducts(page, pageSize, term, categoryId, true, "name", "asc");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error searching products: {ex.Message}");
                return StatusCode(500, new { 
                    message = "Error en búsqueda de productos", 
                    error = 2, 
                    details = ex.Message 
                });
            }
        }

        /// <summary>
        /// Estadísticas rápidas de productos
        /// </summary>
        [HttpGet("stats")]
        [RequirePermission("Product", "ViewReports")]
        public async Task<IActionResult> GetProductStats()
        {
            try
            {
                Console.WriteLine($"?? Getting product statistics");

                // Obtener solo las estadísticas sin productos
                var query = new GetProductByPageQuery
                {
                    Page = 1,
                    PageSize = 1 // Solo necesitamos las estadísticas
                };

                var result = await _mediator.Send(query);

                return Ok(new
                {
                    message = "Estadísticas obtenidas exitosamente",
                    error = 0,
                    data = result.Statistics
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error getting product stats: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener estadísticas",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Actualizar producto
        /// </summary>
        [HttpPut("{id}")]
        [RequirePermission("Product", "Update")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] dynamic productData)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                // TODO: Implementar UpdateProductCommand después
                Console.WriteLine($"?? Update product {id} - Not implemented yet");

                return Ok(new
                {
                    message = "Producto actualizado exitosamente (simulado)",
                    error = 0,
                    productId = id,
                    updatedBy = userName,
                    updatedAt = DateTime.UtcNow,
                    note = "Implementación pendiente - usar CreateProduct para nuevos productos"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Error al actualizar producto", 
                    error = 2, 
                    details = ex.Message 
                });
            }
        }

        /// <summary>
        /// Eliminar producto (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [RequirePermission("Product", "Delete")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                // TODO: Implementar DeleteProductCommand después
                Console.WriteLine($"?? Delete product {id} - Not implemented yet");

                return Ok(new
                {
                    message = "Producto eliminado exitosamente (simulado)",
                    error = 0,
                    productId = id,
                    deletedBy = userName,
                    deletedAt = DateTime.UtcNow,
                    note = "Implementación pendiente - soft delete recomendado"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Error al eliminar producto", 
                    error = 2, 
                    details = ex.Message 
                });
            }
        }
    }

    // DTO para producto básico (testing)
    public class BasicProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal BaseCost { get; set; }
        public string? Brand { get; set; }
        public decimal MinimumStock { get; set; }
        public decimal MaximumStock { get; set; }
    }
}