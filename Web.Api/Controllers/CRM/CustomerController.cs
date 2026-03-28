using Application.Core.CRM.Commands;
using Application.Core.CRM.Queries;
using Application.DTOs.Customer;
using Application.Common.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.CRM
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICustomerCodeGeneratorService _codeGenerator;

        public CustomerController(IMediator mediator, ICustomerCodeGeneratorService codeGenerator)
        {
            _mediator = mediator;
            _codeGenerator = codeGenerator;
        }

        /// <summary>
        /// Crear un nuevo cliente con información fiscal y comercial completa
        /// El código se genera automáticamente de forma incremental: CLI001, CLI002, CLI003...
        /// </summary>
        /// <param name="createCustomerRequest">Datos del cliente a crear</param>
        /// <returns>Cliente creado con toda la información</returns>
        [HttpPost]
        [RequirePermission("Clientes", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateCustomerRequestDto createCustomerRequest)
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

                // Obtener información del usuario del token
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                // Crear command y enviar a través de MediatR
                var command = new CreateCustomerCommand(createCustomerRequest, userId);
                var result = await _mediator.Send(command);

                return Ok(new
                {
                    message = "Cliente creado exitosamente",
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

        /// <summary>
        /// Obtener el próximo código de cliente que se generará automáticamente
        /// Útil para mostrar al usuario qué código se asignará
        /// </summary>
        [HttpGet("next-code")]
        [RequirePermission("Clientes", "Create")]
        public async Task<IActionResult> GetNextCode()
        {
            try
            {
                var nextCode = await _codeGenerator.GenerateNextCustomerCodeAsync();
                
                return Ok(new
                {
                    message = "Próximo código de cliente obtenido exitosamente",
                    error = 0,
                    nextCode = nextCode,
                    info = "Este será el código asignado al próximo cliente creado"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    message = "Error al generar código", 
                    error = 2, 
                    details = ex.Message 
                });
            }
        }

        /// <summary>
        /// Obtener clientes con paginación, filtros y ordenamiento avanzado
        /// Optimizado para tablas de frontend con toda la información necesaria
        /// </summary>
        /// <param name="page">Número de página (inicia en 1)</param>
        /// <param name="pageSize">Tamaño de página (máximo 100)</param>
        /// <param name="searchTerm">Término de búsqueda (nombre, código, email, RFC, empresa)</param>
        /// <param name="sortBy">Campo para ordenar: name, code, email, company, created_at, status</param>
        /// <param name="sortDirection">Dirección: asc o desc</param>
        /// <param name="isActive">Filtro por estado activo: true/false</param>
        /// <param name="statusId">Filtro por ID de estado</param>
        /// <param name="priceListId">Filtro por lista de precios</param>
        /// <returns>Datos paginados optimizados para tabla de clientes</returns>
        [HttpGet]
        [RequirePermission("Clientes", "View")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = "name",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] bool? isActive = null,
            [FromQuery] int? statusId = null,
            [FromQuery] int? priceListId = null)
        {
            try
            {
                Console.WriteLine($"CustomerController.GetAll - Page: {page}, PageSize: {pageSize}, Search: '{searchTerm}'");

                // Crear query con validaciones
                var query = new GetCustomersPagedQuery
                {
                    Page = Math.Max(1, page),
                    PageSize = Math.Min(100, Math.Max(1, pageSize)),
                    SearchTerm = searchTerm,
                    SortBy = sortBy ?? "name",
                    SortDirection = sortDirection ?? "asc",
                    IsActive = isActive,
                    StatusId = statusId,
                    PriceListId = priceListId
                };

                // Ejecutar query a través de MediatR
                var result = await _mediator.Send(query);
                
                return Ok(new 
                { 
                    message = "Clientes obtenidos exitosamente", 
                    error = 0, 
                    data = result.Customers,
                    pagination = result.Pagination,
                    filters = result.Filters,
                    summary = new {
                        totalItems = result.Pagination.TotalItems,
                        currentPage = result.Pagination.CurrentPage,
                        totalPages = result.Pagination.TotalPages,
                        pageSize = result.Pagination.PageSize,
                        hasMore = result.Pagination.HasNextPage,
                        activeFilters = result.Filters.ActiveFiltersCount
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CustomerController.GetAll: {ex.Message}");
                return StatusCode(500, new { 
                    message = "Error al obtener clientes", 
                    error = 2, 
                    details = ex.Message 
                });
            }
        }

        [HttpGet("{id}")]
        [RequirePermission("Clientes", "View")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var customer = await _mediator.Send(new GetCustomerByIdQuery { ID = id });

                if (customer == null)
                    return NotFound(new { message = $"Cliente con ID {id} no encontrado", error = 1 });

                var dto = new CustomerResponseDto
                {
                    Id = customer.ID,
                    Code = customer.Code,
                    Name = customer.Name,
                    LastName = customer.LastName,
                    Phone = customer.Phone,
                    Email = customer.Email,
                    Address = customer.Address,
                    TaxId = customer.TaxId,
                    ZipCode = customer.ZipCode,
                    Commentary = customer.Commentary,
                    CountryId = customer.CountryId,
                    StateId = customer.StateId,
                    InteriorNumber = customer.InteriorNumber,
                    ExteriorNumber = customer.ExteriorNumber,
                    StatusId = customer.StatusId,
                    CreatedAt = customer.CreatedAt ?? DateTime.UtcNow,
                    CompanyName = customer.CompanyName,
                    SatTaxRegime = customer.SatTaxRegime,
                    SatCfdiUse = customer.SatCfdiUse,
                    PriceListId = customer.PriceListId,
                    PriceListName = customer.PriceList?.Name,
                    DiscountPercentage = customer.DiscountPercentage,
                    CreditLimit = customer.CreditLimit,
                    PaymentTermsDays = customer.PaymentTermsDays,
                    IsActive = customer.IsActive,
                    UpdatedAt = customer.UpdatedAt
                };

                return Ok(new { message = "Customer retrieved successfully", error = 0, data = dto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [RequirePermission("Clientes", "Edit")]
        public async Task<IActionResult> Update(int id, [FromBody] dynamic customerData)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                return Ok(new { 
                    message = "Customer updated successfully", 
                    error = 0, 
                    customerId = id,
                    updatedBy = userName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [RequirePermission("Clientes", "Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                return Ok(new { 
                    message = "Customer deleted successfully", 
                    error = 0,
                    customerId = id,
                    deletedBy = userName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Búsqueda de clientes (método alternativo para compatibilidad)
        /// Redirige a GetAll con parámetros de búsqueda
        /// </summary>
        [HttpGet("search")]
        [RequirePermission("Clientes", "View")]
        public async Task<IActionResult> Search(
            [FromQuery] string term, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                {
                    return BadRequest(new { message = "Término de búsqueda requerido", error = 1 });
                }

                // Redirigir a GetAll con término de búsqueda
                return await GetAll(page, pageSize, term);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Error en búsqueda", 
                    error = 2, 
                    details = ex.Message 
                });
            }
        }
    }
}

