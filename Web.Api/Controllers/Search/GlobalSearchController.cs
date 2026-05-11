using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Controllers.Search
{
    /// <summary>
    /// Búsqueda global en múltiples entidades del sistema
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GlobalSearchController : ControllerBase
    {
        private readonly POSDbContext _context;

        public GlobalSearchController(POSDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Búsqueda global en productos, clientes, ventas, módulos y submódulos
        /// </summary>
        /// <param name="query">Término de búsqueda</param>
        /// <param name="limit">Límite de resultados por categoría (default: 5)</param>
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int limit = 5)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                {
                    return Ok(new
                    {
                        message = "Ingrese al menos 2 caracteres",
                        error = 0,
                        data = new
                        {
                            products = new List<object>(),
                            customers = new List<object>(),
                            sales = new List<object>(),
                            modules = new List<object>(),
                            submodules = new List<object>(),
                            totalResults = 0
                        }
                    });
                }

                var searchTerm = query.ToLower().Trim();
                var userId = HttpContext.Items["UserId"] as int? ?? 0;

                // Buscar productos
                var products = await _context.Products
                    .Where(p => p.IsActive &&
                        (p.name.ToLower().Contains(searchTerm) ||
                         p.code.ToLower().Contains(searchTerm) ||
                         (p.barcode != null && p.barcode.ToLower().Contains(searchTerm))))
                    .Take(limit)
                    .Select(p => new
                    {
                        type = "product",
                        id = p.ID,
                        title = p.name,
                        subtitle = p.code,
                        description = $"${p.price:F2}",
                        category = p.Category != null ? p.Category.Name : "Sin categoría",
                        icon = "📦",
                        navigationUrl = $"/products/{p.ID}",
                        extraInfo = new
                        {
                            price = p.price,
                            categoryId = p.CategoryId
                        }
                    })
                    .ToListAsync();

                // Buscar clientes
                var customers = await _context.Customer
                    .Where(c => c.IsActive &&
                        (c.Name.ToLower().Contains(searchTerm) ||
                         c.TaxId.ToLower().Contains(searchTerm) ||
                         c.Email.ToLower().Contains(searchTerm) ||
                         c.Phone.ToLower().Contains(searchTerm)))
                    .Take(limit)
                    .Select(c => new
                    {
                        type = "customer",
                        id = c.ID,
                        title = c.Name,
                        subtitle = c.TaxId,
                        description = c.Email,
                        category = "Cliente",
                        icon = "👤",
                        navigationUrl = $"/customers/{c.ID}",
                        extraInfo = new
                        {
                            phone = c.Phone,
                            address = c.Address,
                            code = c.Code
                        }
                    })
                    .ToListAsync();

                // Buscar ventas
                var sales = await _context.SalesNew
                    .Where(s => 
                        s.Code.ToLower().Contains(searchTerm) ||
                        (s.Customer != null && s.Customer.Name.ToLower().Contains(searchTerm)))
                    .OrderByDescending(s => s.SaleDate)
                    .Take(limit)
                    .Select(s => new
                    {
                        type = "sale",
                        id = s.Id,
                        title = $"Venta #{s.Code}",
                        subtitle = s.Customer != null ? s.Customer.Name : "Público general",
                        description = $"${s.Total:F2} - {s.SaleDate:dd/MMM/yyyy}",
                        category = s.Status == "Completed" ? "Completada" : 
                                   s.Status == "Draft" ? "Borrador" : "Cancelada",
                        icon = "🛒",
                        navigationUrl = $"/sales/{s.Id}",
                        extraInfo = new
                        {
                            total = s.Total,
                            date = s.SaleDate,
                            status = s.Status,
                            customerId = s.CustomerId
                        }
                    })
                    .ToListAsync();

                // Buscar módulos
                var modules = await _context.Modules
                    .Where(m => m.IsActive &&
                        (m.Name.ToLower().Contains(searchTerm) ||
                         m.Description.ToLower().Contains(searchTerm)))
                    .Take(limit)
                    .Select(m => new
                    {
                        type = "module",
                        id = m.Id,
                        title = m.Name,
                        subtitle = "Módulo del sistema",
                        description = m.Description,
                        category = "Módulos",
                        icon = m.Icon,
                        navigationUrl = m.Path,
                        extraInfo = new
                        {
                            path = m.Path,
                            order = m.Order,
                            submodulesCount = m.Submodules.Count(sm => sm.IsActive)
                        }
                    })
                    .ToListAsync();

                // Buscar submódulos
                var submodules = await _context.Submodules
                    .Where(sm => sm.IsActive &&
                        (sm.Name.ToLower().Contains(searchTerm) ||
                         sm.Description.ToLower().Contains(searchTerm)))
                    .Take(limit)
                    .Select(sm => new
                    {
                        type = "submodule",
                        id = sm.Id,
                        title = sm.Name,
                        subtitle = sm.Module.Name,
                        description = sm.Description,
                        category = sm.Module.Name,
                        icon = sm.Icon,
                        navigationUrl = sm.Path,
                        extraInfo = new
                        {
                            moduleId = sm.ModuleId,
                            moduleName = sm.Module.Name,
                            path = sm.Path,
                            order = sm.Order,
                            color = sm.Color
                        }
                    })
                    .ToListAsync();

                // Combinar resultados
                var allResults = new List<object>();
                allResults.AddRange(products);
                allResults.AddRange(customers);
                allResults.AddRange(sales);
                allResults.AddRange(modules);
                allResults.AddRange(submodules);

                return Ok(new
                {
                    message = $"Se encontraron {allResults.Count} resultados",
                    error = 0,
                    data = new
                    {
                        query = query,
                        products = products,
                        customers = customers,
                        sales = sales,
                        modules = modules,
                        submodules = submodules,
                        allResults = allResults,
                        totalResults = allResults.Count,
                        productCount = products.Count,
                        customerCount = customers.Count,
                        saleCount = sales.Count,
                        moduleCount = modules.Count,
                        submoduleCount = submodules.Count
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error en la búsqueda",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Búsqueda de sugerencias rápidas (autocompletado)
        /// </summary>
        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions([FromQuery] string query, [FromQuery] int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                {
                    return Ok(new { data = new List<object>() });
                }

                var searchTerm = query.ToLower().Trim();

                // Sugerencias rápidas (solo nombres/códigos)
                var suggestions = new List<object>();

                // Productos
                var productSuggestions = await _context.Products
                    .Where(p => p.IsActive &&
                        (p.name.ToLower().Contains(searchTerm) ||
                         p.code.ToLower().Contains(searchTerm)))
                    .Take(3)
                    .Select(p => new
                    {
                        text = p.name,
                        type = "product",
                        icon = "📦"
                    })
                    .ToListAsync();

                // Clientes
                var customerSuggestions = await _context.Customer
                    .Where(c => c.IsActive && c.Name.ToLower().Contains(searchTerm))
                    .Take(3)
                    .Select(c => new
                    {
                        text = c.Name,
                        type = "customer",
                        icon = "👤"
                    })
                    .ToListAsync();

                // Módulos
                var moduleSuggestions = await _context.Modules
                    .Where(m => m.IsActive && m.Name.ToLower().Contains(searchTerm))
                    .Take(2)
                    .Select(m => new
                    {
                        text = m.Name,
                        type = "module",
                        icon = m.Icon
                    })
                    .ToListAsync();

                // Submódulos
                var submoduleSuggestions = await _context.Submodules
                    .Where(sm => sm.IsActive && sm.Name.ToLower().Contains(searchTerm))
                    .Take(2)
                    .Select(sm => new
                    {
                        text = $"{sm.Name} ({sm.Module.Name})",
                        type = "submodule",
                        icon = sm.Icon
                    })
                    .ToListAsync();

                suggestions.AddRange(productSuggestions);
                suggestions.AddRange(customerSuggestions);
                suggestions.AddRange(moduleSuggestions);
                suggestions.AddRange(submoduleSuggestions);

                return Ok(new
                {
                    data = suggestions.Take(limit)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error obteniendo sugerencias",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Búsqueda avanzada con filtros por tipo
        /// </summary>
        [HttpGet("advanced")]
        public async Task<IActionResult> AdvancedSearch(
            [FromQuery] string query,
            [FromQuery] string? type = null, // "product", "customer", "sale", "module", "submodule"
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                {
                    return BadRequest(new { message = "Query debe tener al menos 2 caracteres" });
                }

                var searchTerm = query.ToLower().Trim();
                var skip = (page - 1) * pageSize;

                object results = null;
                int totalCount = 0;

                switch (type?.ToLower())
                {
                    case "product":
                        var productsQuery = _context.Products
                            .Where(p => p.IsActive &&
                                (p.name.ToLower().Contains(searchTerm) ||
                                 p.code.ToLower().Contains(searchTerm) ||
                                 (p.barcode != null && p.barcode.ToLower().Contains(searchTerm))));

                        totalCount = await productsQuery.CountAsync();
                        
                        results = await productsQuery
                            .Skip(skip)
                            .Take(pageSize)
                            .Select(p => new
                            {
                                id = p.ID,
                                name = p.name,
                                code = p.code,
                                price = p.price,
                                category = p.Category != null ? p.Category.Name : "Sin categoría",
                                navigationUrl = $"/products/{p.ID}"
                            })
                            .ToListAsync();
                        break;

                    case "customer":
                        var customersQuery = _context.Customer
                            .Where(c => c.IsActive &&
                                (c.Name.ToLower().Contains(searchTerm) ||
                                 c.TaxId.ToLower().Contains(searchTerm) ||
                                 c.Email.ToLower().Contains(searchTerm)));

                        totalCount = await customersQuery.CountAsync();
                        
                        results = await customersQuery
                            .Skip(skip)
                            .Take(pageSize)
                            .Select(c => new
                            {
                                id = c.ID,
                                name = c.Name,
                                taxId = c.TaxId,
                                email = c.Email,
                                phone = c.Phone,
                                navigationUrl = $"/customers/{c.ID}"
                            })
                            .ToListAsync();
                        break;

                    case "sale":
                        var salesQuery = _context.SalesNew
                            .Where(s => s.Code.ToLower().Contains(searchTerm) ||
                                       (s.Customer != null && s.Customer.Name.ToLower().Contains(searchTerm)));

                        totalCount = await salesQuery.CountAsync();
                        
                        results = await salesQuery
                            .OrderByDescending(s => s.SaleDate)
                            .Skip(skip)
                            .Take(pageSize)
                            .Select(s => new
                            {
                                id = s.Id,
                                code = s.Code,
                                customer = s.Customer != null ? s.Customer.Name : "Público general",
                                total = s.Total,
                                date = s.SaleDate,
                                status = s.Status,
                                navigationUrl = $"/sales/{s.Id}"
                            })
                            .ToListAsync();
                        break;

                    case "module":
                        var modulesQuery = _context.Modules
                            .Where(m => m.IsActive &&
                                (m.Name.ToLower().Contains(searchTerm) ||
                                 m.Description.ToLower().Contains(searchTerm)));

                        totalCount = await modulesQuery.CountAsync();
                        
                        results = await modulesQuery
                            .Skip(skip)
                            .Take(pageSize)
                            .Select(m => new
                            {
                                id = m.Id,
                                name = m.Name,
                                description = m.Description,
                                path = m.Path,
                                icon = m.Icon,
                                order = m.Order,
                                navigationUrl = m.Path
                            })
                            .ToListAsync();
                        break;

                    case "submodule":
                        var submodulesQuery = _context.Submodules
                            .Where(sm => sm.IsActive &&
                                (sm.Name.ToLower().Contains(searchTerm) ||
                                 sm.Description.ToLower().Contains(searchTerm)));

                        totalCount = await submodulesQuery.CountAsync();
                        
                        results = await submodulesQuery
                            .Skip(skip)
                            .Take(pageSize)
                            .Select(sm => new
                            {
                                id = sm.Id,
                                name = sm.Name,
                                description = sm.Description,
                                moduleName = sm.Module.Name,
                                moduleId = sm.ModuleId,
                                path = sm.Path,
                                order = sm.Order,
                                navigationUrl = sm.Path,
                                icon = sm.Icon,
                                color = sm.Color
                            })
                            .ToListAsync();
                        break;

                    default:
                        return BadRequest(new { message = "Tipo no válido. Use: product, customer, sale, module, submodule" });
                }

                return Ok(new
                {
                    message = "Búsqueda completada",
                    error = 0,
                    data = results,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error en búsqueda avanzada",
                    error = 2,
                    details = ex.Message
                });
            }
        }
    }
}
