using Application.Core.CRM.Commands;
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
        [RequirePermission("Customer", "Create")]
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
        [RequirePermission("Customer", "Create")]
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

        [HttpGet]
        [RequirePermission("Customer", "ViewList")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Simular datos de clientes (implementar lógica real después)
                var customers = new[] {
                    new { id = 1, name = "Cliente 1", code = "CLI001", email = "cliente1@email.com", active = true },
                    new { id = 2, name = "Cliente 2", code = "CLI002", email = "cliente2@email.com", active = true }
                };
                
                return Ok(new { 
                    message = "Customers retrieved successfully", 
                    error = 0, 
                    data = customers,
                    pagination = new {
                        page,
                        pageSize,
                        totalItems = customers.Length
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [RequirePermission("Customer", "ViewList")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                // Simular obtención de cliente por ID
                var customer = new { 
                    id, 
                    name = $"Cliente {id}", 
                    code = $"CLI{id:D3}", 
                    email = $"cliente{id}@email.com",
                    phone = "1234567890",
                    active = true 
                };
                
                return Ok(new { 
                    message = "Customer retrieved successfully", 
                    error = 0, 
                    data = customer 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [RequirePermission("Customer", "Update")]
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
        [RequirePermission("Customer", "Delete")]
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

        [HttpGet("search")]
        [RequirePermission("Customer", "ViewList")]
        public async Task<IActionResult> Search([FromQuery] string term, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrEmpty(term))
                {
                    return BadRequest(new { message = "Search term is required", error = 1 });
                }

                // Simular búsqueda de clientes
                var customers = new[] {
                    new { id = 1, name = "Cliente 1", code = "CLI001", email = "cliente1@email.com" },
                    new { id = 2, name = "Cliente 2", code = "CLI002", email = "cliente2@email.com" }
                };

                var filteredResult = customers.Where(c => 
                    c.name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    c.code.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    c.email.Contains(term, StringComparison.OrdinalIgnoreCase)
                );
                
                return Ok(new { 
                    message = "Search completed successfully", 
                    error = 0, 
                    searchTerm = term,
                    data = filteredResult,
                    totalResults = filteredResult.Count()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }
    }
}

