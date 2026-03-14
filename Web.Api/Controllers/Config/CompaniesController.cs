using Application.Core.Company.Commands;
using Application.Core.Company.Queries;
using Application.DTOs.Company;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Config
{
    /// <summary>
    /// Controlador para gestión de empresas
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CompaniesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Crear nueva empresa
        /// </summary>
        [HttpPost]
        [RequirePermission("Configuracion", "Empresas")]  // ✅ Coincide con la BD
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyDto request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                Console.WriteLine($"?? Creando empresa - Usuario: {userName}, Razón Social: {request.LegalName}");

                var command = new CreateCompanyCommand(request, userId);
                var result = await _mediator.Send(command);

                return Ok(new
                {
                    message = "Empresa creada exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"? Validation error: {ex.Message}");
                return BadRequest(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error creating company: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al crear empresa",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Actualizar empresa existente
        /// </summary>
        [HttpPut("{id}")]
        [RequirePermission("Configuracion", "Empresas")]  // ✅ Coincide con la BD
        public async Task<IActionResult> UpdateCompany(int id, [FromBody] UpdateCompanyDto request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                Console.WriteLine($"?? Actualizando empresa {id} - Usuario: {userName}");

                var command = new UpdateCompanyCommand(id, request, userId);
                var result = await _mediator.Send(command);

                return Ok(new
                {
                    message = "Empresa actualizada exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"? Not found: {ex.Message}");
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error updating company: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al actualizar empresa",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener empresa por ID
        /// </summary>
        [HttpGet("{id}")]
        [RequirePermission("Configuracion", "Empresas")]  // ✅ Coincide con la BD
        public async Task<IActionResult> GetCompany(int id)
        {
            try
            {
                var query = new GetCompanyByIdQuery(id);
                var result = await _mediator.Send(query);

                return Ok(new
                {
                    message = "Empresa obtenida exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error getting company: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener empresa",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener todas las empresas activas
        /// </summary>
        [HttpGet("active")]
        [RequirePermission("Configuracion", "Empresas")]  // ✅ Coincide con la BD
        public async Task<IActionResult> GetActiveCompanies()
        {
            try
            {
                var query = new GetAllActiveCompaniesQuery();
                var result = await _mediator.Send(query);

                return Ok(new
                {
                    message = "Empresas activas obtenidas exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error getting active companies: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener empresas activas",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener empresas paginadas con filtros
        /// </summary>
        [HttpGet]
        [RequirePermission("Configuracion", "Empresas")]  // ✅ Coincide con la BD
        public async Task<IActionResult> GetCompanies(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                Console.WriteLine($"?? Getting companies - Page: {page}, PageSize: {pageSize}, Active: {isActive}, Search: {searchTerm}");

                var query = new GetCompaniesPagedQuery(page, pageSize, isActive, searchTerm);
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error getting companies: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener empresas",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener empresa principal/matriz
        /// </summary>
        [HttpGet("main")]
        [RequirePermission("Configuracion", "Empresas")]  // ✅ Coincide con la BD
        public async Task<IActionResult> GetMainCompany()
        {
            try
            {
                var query = new GetMainCompanyQuery();
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "No hay empresa principal configurada",
                        error = 1
                    });
                }

                return Ok(new
                {
                    message = "Empresa principal obtenida exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error getting main company: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener empresa principal",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Desactivar empresa (baja lógica)
        /// </summary>
        [HttpPut("{id}/deactivate")]
        [RequirePermission("Configuracion", "Empresas")]  // ✅ Coincide con la BD
        public async Task<IActionResult> DeactivateCompany(int id)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                Console.WriteLine($"?? Desactivando empresa {id} - Usuario: {userName}");

                var command = new DeactivateCompanyCommand(id, userId);
                await _mediator.Send(command);

                return Ok(new
                {
                    message = "Empresa desactivada exitosamente",
                    error = 0
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
                Console.WriteLine($"? Error deactivating company: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al desactivar empresa",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Reactivar empresa
        /// </summary>
        [HttpPut("{id}/reactivate")]
        [RequirePermission("Configuracion", "Empresas")]  // ✅ Coincide con la BD
        public async Task<IActionResult> ReactivateCompany(int id)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                Console.WriteLine($"?? Reactivando empresa {id} - Usuario: {userName}");

                var command = new ReactivateCompanyCommand(id, userId);
                await _mediator.Send(command);

                return Ok(new
                {
                    message = "Empresa reactivada exitosamente",
                    error = 0
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
                Console.WriteLine($"? Error reactivating company: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al reactivar empresa",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Actualizar configuración fiscal (certificados SAT, folios)
        /// </summary>
        [HttpPut("{id}/fiscal-config")]
        [RequirePermission("Configuracion", "Empresas")]  // ✅ Coincide con la BD
        public async Task<IActionResult> UpdateFiscalConfig(int id, [FromBody] UpdateCompanyFiscalConfigDto request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                Console.WriteLine($"?? Actualizando configuración fiscal empresa {id} - Usuario: {userName}");

                var command = new UpdateCompanyFiscalConfigCommand(id, request, userId);
                var result = await _mediator.Send(command);

                return Ok(new
                {
                    message = "Configuración fiscal actualizada exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error updating fiscal config: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al actualizar configuración fiscal",
                    error = 2,
                    details = ex.Message
                });
            }
        }
    }
}
