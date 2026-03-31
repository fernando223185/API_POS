using Application.Core.Product.Commands;
using Application.Core.Product.Queries;
using Application.DTOs.Product;
using Application.Abstractions.Storage;
using Application.Abstractions.Catalogue;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;
using Domain.Entities;

namespace Web.Api.Controllers.Products
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IS3StorageService _s3Service;
        private readonly IProductImageRepository _imageRepository;

        public ProductsController(
            IMediator mediator,
            IS3StorageService s3Service,
            IProductImageRepository imageRepository)
        {
            _mediator = mediator;
            _s3Service = s3Service;
            _imageRepository = imageRepository;
        }

        /// <summary>
        /// ? OBTENER PRODUCTOS CON CONSULTA REAL A BASE DE DATOS
        /// Endpoint mejorado con paginaci�n, filtros, estad�sticas y datos reales
        /// </summary>
        [HttpGet]
        [RequirePermission("Productos", "View")]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 50,
            [FromQuery] string? search = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] bool? isActive = true,
            [FromQuery] string? sortBy = "name",
            [FromQuery] string? sortOrder = "asc",
            // ? NUEVO: Par�metros de inventario
            [FromQuery] bool includeWarehouseStock = false,
            [FromQuery] int? warehouseId = null,
            [FromQuery] bool? onlyWithStock = null,
            [FromQuery] bool? onlyBelowMinimum = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                Console.WriteLine($"?? Getting products - Page: {page}, PageSize: {pageSize}, Search: '{search}', " +
                                $"Category: {categoryId}, IncludeStock: {includeWarehouseStock}, Warehouse: {warehouseId}");

                // Validar par�metros
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
                    SortDirection = sortOrder,
                    // ? NUEVO: Par�metros de inventario
                    IncludeWarehouseStock = includeWarehouseStock,
                    WarehouseId = warehouseId,
                    OnlyWithStock = onlyWithStock,
                    OnlyBelowMinimum = onlyBelowMinimum
                };

                // Ejecutar query usando MediatR
                var result = await _mediator.Send(query);

                Console.WriteLine($"?? Products retrieved: {result.Data.Count} items, Page: {result.Page}/{result.TotalPages}");

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
        /// ✅ Obtener producto por ID con TODA su información para edición
        /// Retorna el ProductResponseDto completo con todos los campos necesarios para el formulario
        /// </summary>
        [HttpGet("{id}")]
        [RequirePermission("Productos", "View")]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                Console.WriteLine($"✅ Getting product by ID: {id}");

                // Crear query para obtener por ID
                var query = new GetProductByIdQuery { ID = id };
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

                Console.WriteLine($"✅ Product found: {product.Name} (Code: {product.Code})");

                // Retornar el DTO completo con TODA la información
                return Ok(new
                {
                    message = "Producto obtenido exitosamente",
                    error = 0,
                    data = product // ProductResponseDto completo con todos los campos
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting product {id}: {ex.Message}");
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
        /// Endpoint principal para crear productos con informaci�n fiscal, inventario, marketing, etc.
        /// </summary>
        /// <param name="createProductRequest">Datos completos del producto</param>
        /// <returns>Producto creado con toda la informaci�n</returns>
        [HttpPost]
        [RequirePermission("Productos", "Create")]
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
                        message = "Datos de entrada inv�lidos",
                        error = 1,
                        errors = ModelState.SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                    });
                }

                // Obtener informaci�n del usuario del token
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                if (userId == 0)
                {
                    return Unauthorized(new { 
                        message = "Usuario no autenticado", 
                        error = 1 
                    });
                }

                // Crear command y enviar a trav�s de MediatR
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
                        category = result.CategoryName ?? "Sin categor�a",
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
        /// Crear producto b�sico (endpoint simplificado para testing)
        /// </summary>
        [HttpPost("basic")]
        [RequirePermission("Productos", "Create")]
        public async Task<IActionResult> CreateBasicProduct([FromBody] BasicProductDto basicProduct)
        {
            try
            {
                Console.WriteLine($"??? Creating basic product: {basicProduct.Name}");

                // Mapear datos b�sicos a CreateProductRequestDto completo
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
                    CountryOfOrigin = "M�xico",
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
        /// B�squeda optimizada de productos
        /// </summary>
        [HttpGet("search")]
        [RequirePermission("Productos", "View")]
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

                // Usar el mismo endpoint de GetProducts con par�metros de b�squeda
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
        /// Estad�sticas r�pidas de productos
        /// </summary>
        [HttpGet("stats")]
        [RequirePermission("Productos", "View")]
        public async Task<IActionResult> GetProductStats()
        {
            try
            {
                Console.WriteLine($"?? Getting product statistics");

                // Obtener solo las estad�sticas sin productos
                var query = new GetProductByPageQuery
                {
                    Page = 1,
                    PageSize = 1 // Solo necesitamos las estad�sticas
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
        /// ✅ Actualizar producto existente (CQRS implementado)
        /// </summary>
        /// <param name="id">ID del producto a actualizar</param>
        /// <param name="updateProductRequest">Datos actualizados del producto</param>
        /// <returns>Producto actualizado con toda la información</returns>
        [HttpPut("{id}")]
        [RequirePermission("Productos", "Edit")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequestDto updateProductRequest)
        {
            try
            {
                Console.WriteLine($"✅ Updating product ID: {id} - {updateProductRequest.Name}");

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
                var command = new UpdateProductCommand(id, updateProductRequest, userId);
                var result = await _mediator.Send(command);

                Console.WriteLine($"✅ Product updated successfully: ID {result.ID}");

                return Ok(new
                {
                    message = "Producto actualizado exitosamente",
                    error = 0,
                    data = result,
                    updatedBy = userName,
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
                        trackExpiry = result.TrackExpiry,
                        updatedAt = result.UpdatedAt
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"❌ Business logic error: {ex.Message}");
                return BadRequest(new
                {
                    message = ex.Message,
                    error = 1
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error updating product: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    message = "Error interno del servidor al actualizar producto",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// 🖼️ Actualizar producto con nueva imagen (reemplaza la imagen primaria en S3)
        /// Este endpoint permite actualizar los datos del producto Y reemplazar su imagen principal
        /// </summary>
        /// <param name="id">ID del producto a actualizar</param>
        /// <param name="file">Nueva imagen del producto (opcional)</param>
        /// <param name="productData">Datos JSON del producto (como string, se deserializa internamente)</param>
        /// <returns>Producto actualizado con la nueva imagen</returns>
        [HttpPut("{id}/with-image")]
        [RequirePermission("Productos", "Edit")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProductWithImage(
            int id,
            [FromForm] IFormFile? file,
            [FromForm] string productData)
        {
            try
            {
                Console.WriteLine($"🖼️ Updating product {id} with image replacement...");

                // Obtener usuario actual
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                // Deserializar datos del producto
                UpdateProductRequestDto? updateRequest;
                try
                {
                    updateRequest = System.Text.Json.JsonSerializer.Deserialize<UpdateProductRequestDto>(
                        productData,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (updateRequest == null)
                    {
                        return BadRequest(new { message = "Datos del producto inválidos", error = 1 });
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(new
                    {
                        message = "Error al deserializar datos del producto",
                        error = 1,
                        details = ex.Message
                    });
                }

                // ✅ PASO 1: Actualizar los datos del producto usando CQRS
                var command = new UpdateProductCommand(id, updateRequest, userId);
                var updatedProduct = await _mediator.Send(command);

                Console.WriteLine($"✅ Product data updated: {updatedProduct.Name}");

                // ✅ PASO 2: Si se envió una nueva imagen, reemplazar la imagen primaria
                string? newImageUrl = null;
                string? newS3Key = null;

                if (file != null && file.Length > 0)
                {
                    Console.WriteLine($"🖼️ Processing new image: {file.FileName} ({file.Length} bytes)");

                    // Validar tamaño de archivo (máximo 5MB)
                    if (file.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest(new
                        {
                            message = "El archivo es demasiado grande. Tamaño máximo: 5MB",
                            error = 1,
                            fileSize = file.Length,
                            maxSize = 5 * 1024 * 1024
                        });
                    }

                    // Validar tipo de archivo
                    var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                    if (!allowedTypes.Contains(file.ContentType.ToLower()))
                    {
                        return BadRequest(new
                        {
                            message = "Tipo de archivo no permitido. Solo se permiten imágenes (JPEG, PNG, GIF, WEBP)",
                            error = 1,
                            contentType = file.ContentType
                        });
                    }

                    // 🗑️ PASO 2.1: Eliminar la imagen primaria anterior de S3 (si existe)
                    var existingImages = await _imageRepository.GetByProductIdAsync(id);
                    var primaryImage = existingImages.FirstOrDefault(img => img.IsPrimary);

                    if (primaryImage != null)
                    {
                        Console.WriteLine($"🗑️ Deleting old primary image from S3: {primaryImage.ImageUrl}");
                        
                        try
                        {
                            // Extraer S3 Key de la URL
                            var oldS3Key = primaryImage.ImageUrl.Replace(_s3Service.GetPublicUrl(""), "");
                            var deletedFromS3 = await _s3Service.DeleteImageAsync(oldS3Key);
                            
                            // Eliminar registro de la base de datos
                            await _imageRepository.DeleteAsync(primaryImage.Id);
                            
                            Console.WriteLine($"✅ Old image deleted from S3: {deletedFromS3}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Warning: Could not delete old image: {ex.Message}");
                            // Continuar aunque falle la eliminación de la imagen anterior
                        }
                    }

                    // ⬆️ PASO 2.2: Subir nueva imagen a S3
                    using (var stream = file.OpenReadStream())
                    {
                        newS3Key = await _s3Service.UploadImageAsync(
                            stream,
                            file.FileName,
                            file.ContentType,
                            $"products/{id}");
                    }

                    Console.WriteLine($"✅ New image uploaded to S3: {newS3Key}");

                    // Obtener URL pública
                    newImageUrl = _s3Service.GetPublicUrl(newS3Key);

                    // 💾 PASO 2.3: Crear nuevo registro en la base de datos
                    var productImage = new ProductImage
                    {
                        ProductId = id,
                        ImageUrl = newImageUrl,
                        ImageName = file.FileName,
                        AltText = updatedProduct.Name,
                        IsPrimary = true, // Siempre es la imagen primaria
                        DisplayOrder = 1,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UploadedByUserId = userId
                    };

                    var savedImage = await _imageRepository.CreateAsync(productImage);
                    Console.WriteLine($"✅ New image record created in database: ID {savedImage.Id}");
                }

                // ✅ Respuesta final
                return Ok(new
                {
                    message = file != null 
                        ? "Producto e imagen actualizados exitosamente" 
                        : "Producto actualizado exitosamente (sin cambio de imagen)",
                    error = 0,
                    data = updatedProduct,
                    image = file != null ? new
                    {
                        s3Key = newS3Key,
                        publicUrl = newImageUrl,
                        fileName = file.FileName,
                        size = file.Length,
                        uploadedAt = DateTime.UtcNow,
                        uploadedBy = userName
                    } : null,
                    updatedBy = userName,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"❌ Business logic error: {ex.Message}");
                return BadRequest(new
                {
                    message = ex.Message,
                    error = 1
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error updating product with image: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    message = "Error interno del servidor al actualizar producto con imagen",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Eliminar producto (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [RequirePermission("Productos", "Delete")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                // TODO: Implementar DeleteProductCommand despu�s
                Console.WriteLine($"?? Delete product {id} - Not implemented yet");

                return Ok(new
                {
                    message = "Producto eliminado exitosamente (simulado)",
                    error = 0,
                    productId = id,
                    deletedBy = userName,
                    deletedAt = DateTime.UtcNow,
                    note = "Implementaci�n pendiente - soft delete recomendado"
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

        /// <summary>
        /// ?? SUBIR IMAGEN DE PRODUCTO A AWS S3
        /// Sube una imagen al bucket S3 y guarda el registro en la base de datos
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <param name="file">Archivo de imagen (JPEG, PNG, GIF, WebP)</param>
        /// <param name="isPrimary">Marcar como imagen principal</param>
        /// <param name="altText">Texto alternativo para SEO</param>
        /// <returns>Informaci�n de la imagen subida incluyendo el S3 Key</returns>
        [HttpPost("{productId}/upload-image")]
        [RequirePermission("Productos", "Edit")]
        public async Task<IActionResult> UploadProductImage(
            int productId,
            [FromForm] IFormFile file,
            [FromForm] bool isPrimary = false,
            [FromForm] string? altText = null)
        {
            try
            {
                Console.WriteLine($"?? Uploading image for product {productId}");

                // Validar que el producto existe
                var productQuery = new GetProductByIdQuery { ID = productId };
                var product = await _mediator.Send(productQuery);

                if (product == null)
                {
                    return NotFound(new
                    {
                        message = "Producto no encontrado",
                        error = 1,
                        productId
                    });
                }

                // Validar archivo
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new
                    {
                        message = "Archivo de imagen requerido",
                        error = 1
                    });
                }

                // Validar tipo de archivo
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new
                    {
                        message = "Formato de archivo no válido. Solo se permiten: JPG, JPEG, PNG, GIF, WebP",
                        error = 1,
                        allowedFormats = allowedExtensions
                    });
                }

                // Validar tama�o (m�ximo 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new
                    {
                        message = "El archivo es demasiado grande. Tamaño máximo: 5MB",
                        error = 1,
                        fileSize = file.Length,
                        maxSize = 5 * 1024 * 1024
                    });
                }

                // Obtener usuario actual
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                Console.WriteLine($"?? Uploading {file.FileName} ({file.Length} bytes) to S3...");

                // Subir a S3
                string s3Key;
                using (var stream = file.OpenReadStream())
                {
                    s3Key = await _s3Service.UploadImageAsync(
                        stream,
                        file.FileName,
                        file.ContentType,
                        $"products/{productId}"); // Carpeta por producto
                }

                Console.WriteLine($"? S3 Upload successful: {s3Key}");

                // Obtener URL p�blica
                var publicUrl = _s3Service.GetPublicUrl(s3Key);

                // Obtener siguiente orden de visualizaci�n
                var displayOrder = await _imageRepository.GetNextDisplayOrderAsync(productId);

                // Si es primary, quitar primary de las dem�s
                if (isPrimary)
                {
                    await _imageRepository.SetAsPrimaryAsync(productId, -1); // -1 para quitar de todas
                }

                // Crear registro en base de datos
                var productImage = new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = publicUrl,
                    ImageName = file.FileName,
                    AltText = altText ?? product.Name,
                    IsPrimary = isPrimary,
                    DisplayOrder = displayOrder,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UploadedByUserId = userId
                };

                var savedImage = await _imageRepository.CreateAsync(productImage);

                Console.WriteLine($"? Image saved to database with ID: {savedImage.Id}");

                // Mapear a DTO
                var response = new ProductImageDto
                {
                    Id = savedImage.Id,
                    ProductId = savedImage.ProductId,
                    S3Key = s3Key,  // ? RETORNAR EL KEY DE S3
                    PublicUrl = publicUrl,
                    ImageName = savedImage.ImageName ?? "",
                    AltText = savedImage.AltText,
                    IsPrimary = savedImage.IsPrimary,
                    DisplayOrder = savedImage.DisplayOrder,
                    UploadedAt = savedImage.CreatedAt,
                    UploadedBy = userName
                };

                return Ok(new UploadProductImageResponseDto
                {
                    Message = "Imagen subida exitosamente",
                    Error = 0,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error uploading image: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    message = "Error al subir imagen",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ??? OBTENER IM�GENES DE UN PRODUCTO
        /// </summary>
        [HttpGet("{productId}/images")]
        [RequirePermission("Productos", "View")]
        public async Task<IActionResult> GetProductImages(int productId)
        {
            try
            {
                // Verificar que el producto existe
                var productQuery = new GetProductByIdQuery { ID = productId };
                var product = await _mediator.Send(productQuery);

                if (product == null)
                {
                    return NotFound(new
                    {
                        message = "Producto no encontrado",
                        error = 1,
                        productId
                    });
                }

                // Obtener im�genes
                var images = await _imageRepository.GetByProductIdAsync(productId);

                var imageDtos = images.Select(img => new ProductImageDto
                {
                    Id = img.Id,
                    ProductId = img.ProductId,
                    S3Key = img.ImageUrl.Replace(_s3Service.GetPublicUrl(""), ""), // Extraer key de URL
                    PublicUrl = img.ImageUrl,
                    ImageName = img.ImageName ?? "",
                    AltText = img.AltText,
                    IsPrimary = img.IsPrimary,
                    DisplayOrder = img.DisplayOrder,
                    UploadedAt = img.CreatedAt,
                    UploadedBy = img.UploadedBy?.Name ?? "Unknown"
                }).ToList();

                return Ok(new ProductImagesResponseDto
                {
                    Message = "Imágenes obtenidas exitosamente",
                    Error = 0,
                    Data = imageDtos,
                    TotalImages = imageDtos.Count
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error getting product images: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener im�genes",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ??? ELIMINAR IMAGEN DE PRODUCTO
        /// Elimina la imagen de S3 y marca como inactiva en la BD
        /// </summary>
        [HttpDelete("images/{imageId}")]
        [RequirePermission("Productos", "Delete")]
        public async Task<IActionResult> DeleteProductImage(int imageId)
        {
            try
            {
                // Obtener imagen
                var image = await _imageRepository.GetByIdAsync(imageId);

                if (image == null)
                {
                    return NotFound(new
                    {
                        message = "Imagen no encontrada",
                        error = 1,
                        imageId
                    });
                }

                // Extraer S3 Key de la URL
                var s3Key = image.ImageUrl.Replace(_s3Service.GetPublicUrl(""), "");

                // Eliminar de S3
                var deletedFromS3 = await _s3Service.DeleteImageAsync(s3Key);

                // Soft delete en BD
                var deletedFromDb = await _imageRepository.DeleteAsync(imageId);

                if (deletedFromDb)
                {
                    return Ok(new
                    {
                        message = "Imagen eliminada exitosamente",
                        error = 0,
                        imageId,
                        deletedFromS3,
                        deletedFromDatabase = deletedFromDb
                    });
                }

                return StatusCode(500, new
                {
                    message = "Error al eliminar imagen de la base de datos",
                    error = 2
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error deleting image: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al eliminar imagen",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ? ESTABLECER IMAGEN COMO PRINCIPAL
        /// </summary>
        [HttpPut("images/{imageId}/set-primary")]
        [RequirePermission("Productos", "Edit")]
        public async Task<IActionResult> SetPrimaryImage(int imageId)
        {
            try
            {
                var image = await _imageRepository.GetByIdAsync(imageId);

                if (image == null)
                {
                    return NotFound(new
                    {
                        message = "Imagen no encontrada",
                        error = 1,
                        imageId
                    });
                }

                var result = await _imageRepository.SetAsPrimaryAsync(image.ProductId, imageId);

                if (result)
                {
                    return Ok(new
                    {
                        message = "Imagen establecida como principal",
                        error = 0,
                        imageId,
                        productId = image.ProductId
                    });
                }

                return StatusCode(500, new
                {
                    message = "Error al establecer imagen como principal",
                    error = 2
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error setting primary image: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al establecer imagen principal",
                    error = 2,
                    details = ex.Message
                });
            }
        }
    }

    // DTO para producto b�sico (testing)
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