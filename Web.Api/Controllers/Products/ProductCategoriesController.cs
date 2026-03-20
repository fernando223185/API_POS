using Microsoft.AspNetCore.Mvc;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Products
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoriesController : ControllerBase
    {
        private readonly POSDbContext _context;

        public ProductCategoriesController(POSDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtener todas las categor�as de productos activas
        /// </summary>
        [HttpGet]
        [RequirePermission("Productos", "View")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.ProductCategories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        description = c.Description,
                        code = c.Code,
                        isActive = c.IsActive,
                        createdAt = c.CreatedAt,
                        productsCount = c.Products.Count() // Cantidad de productos en esta categor�a
                    })
                    .ToListAsync();

                return Ok(new
                {
                    message = "Categor�as obtenidas exitosamente",
                    error = 0,
                    data = categories,
                    totalCategories = categories.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al obtener categor�as",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener categor�a por ID
        /// </summary>
        [HttpGet("{id}")]
        [RequirePermission("Productos", "View")]
        public async Task<IActionResult> GetCategory(int id)
        {
            try
            {
                var category = await _context.ProductCategories
                    .Where(c => c.Id == id && c.IsActive)
                    .Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        description = c.Description,
                        code = c.Code,
                        isActive = c.IsActive,
                        createdAt = c.CreatedAt,
                        productsCount = c.Products.Count(),
                        subcategories = c.Subcategories.Where(s => s.IsActive).Select(s => new
                        {
                            id = s.Id,
                            name = s.Name,
                            code = s.Code,
                            description = s.Description
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (category == null)
                {
                    return NotFound(new
                    {
                        message = "Categor�a no encontrada",
                        error = 1
                    });
                }

                return Ok(new
                {
                    message = "Categor�a obtenida exitosamente",
                    error = 0,
                    data = category
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al obtener categor�a",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener categor�as para select/dropdown (formato optimizado para frontend)
        /// </summary>
        [HttpGet("dropdown")]
        [RequirePermission("Productos", "View")]
        public async Task<IActionResult> GetCategoriesForDropdown()
        {
            try
            {
                var categories = await _context.ProductCategories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .Select(c => new
                    {
                        value = c.Id,
                        label = c.Name,
                        code = c.Code.ToLower() // Para compatibilidad con tu HTML original
                    })
                    .ToListAsync();

                return Ok(new
                {
                    message = "Categor�as para dropdown obtenidas exitosamente",
                    error = 0,
                    data = categories
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al obtener categor�as para dropdown",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener estad�sticas de categor�as
        /// </summary>
        [HttpGet("stats")]
        [RequirePermission("Productos", "View")]
        public async Task<IActionResult> GetCategoryStats()
        {
            try
            {
                var stats = await _context.ProductCategories
                    .Where(c => c.IsActive)
                    .Select(c => new
                    {
                        categoryId = c.Id,
                        categoryName = c.Name,
                        categoryCode = c.Code,
                        productsCount = c.Products.Count(p => p.IsActive),
                        totalProducts = c.Products.Count(),
                        subcategoriesCount = c.Subcategories.Count(s => s.IsActive),
                        // Agregar m�s estad�sticas cuando tengamos productos reales
                        avgPrice = c.Products.Any() ? c.Products.Average(p => p.price) : 0,
                        totalValue = c.Products.Sum(p => p.price * (p.MinimumStock + p.MaximumStock) / 2)
                    })
                    .OrderByDescending(s => s.productsCount)
                    .ToListAsync();

                var totalStats = new
                {
                    totalCategories = stats.Count,
                    totalProducts = stats.Sum(s => s.productsCount),
                    totalValue = stats.Sum(s => s.totalValue),
                    avgProductsPerCategory = stats.Any() ? stats.Average(s => s.productsCount) : 0
                };

                return Ok(new
                {
                    message = "Estad�sticas de categor�as obtenidas exitosamente",
                    error = 0,
                    data = new
                    {
                        categoryStats = stats,
                        summary = totalStats
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al obtener estad�sticas",
                    error = 2,
                    details = ex.Message
                });
            }
        }
    }
}