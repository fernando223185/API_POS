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

        public CustomerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [RequirePermission("Customer", "Create")] // Requiere permiso específico
        public async Task<IActionResult> Create([FromBody] dynamic customerData)
        {
            try
            {
                // Agregar información del usuario del token
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                // Simular creación de cliente (implementar lógica real después)
                var customerId = new Random().Next(1000, 9999);

                return Ok(new { 
                    message = "Customer created successfully", 
                    error = 0, 
                    customerId = customerId,
                    createdBy = userName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpGet]
        [RequirePermission("Customer", "ViewList")] // Requiere permiso para ver lista
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
        [RequirePermission("Customer", "ViewList")] // Requiere permiso para ver detalles
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
        [RequirePermission("Customer", "Update")] // Requiere permiso para actualizar
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
        [RequirePermission("Customer", "Delete")] // Requiere permiso para eliminar
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
        [RequirePermission("Customer", "ViewList")] // Requiere permiso para buscar
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

