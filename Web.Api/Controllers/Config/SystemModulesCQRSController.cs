using Application.Core.SystemModules.Commands;
using Application.Core.SystemModules.Queries;
using Application.DTOs.SystemModules;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Config
{
    /// <summary>
    /// CRUD completo para módulos y submódulos usando patrón CQRS
    /// </summary>
    [Route("api/system/modules")]
    [ApiController]
    public class SystemModulesCQRSController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SystemModulesCQRSController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #region ?? MÓDULOS - QUERIES

        /// <summary>
        /// ?? Obtener todos los módulos con sus submódulos
        /// </summary>
        [HttpGet]
        [RequireAuthentication]
        public async Task<IActionResult> GetAllModules([FromQuery] bool includeInactive = false)
        {
            try
            {
                var query = new GetAllModulesQuery(includeInactive);
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener módulos: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener módulos",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ?? Obtener un módulo por ID con sus submódulos
        /// </summary>
        [HttpGet("{id}")]
        [RequireAuthentication]
        public async Task<IActionResult> GetModuleById(int id)
        {
            try
            {
                var query = new GetModuleByIdQuery(id);
                var module = await _mediator.Send(query);

                if (module == null)
                {
                    return NotFound(new
                    {
                        message = "Módulo no encontrado",
                        error = 1
                    });
                }

                return Ok(new
                {
                    message = "Módulo obtenido exitosamente",
                    error = 0,
                    data = module
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener módulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener módulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ? MÓDULOS - COMMANDS

        /// <summary>
        /// ? Crear un nuevo módulo
        /// </summary>
        [HttpPost]
        [RequirePermission("Configuration", "ManageModules")]
        public async Task<IActionResult> CreateModule([FromBody] CreateModuleDto dto)
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

                var currentUserId = HttpContext.Items["UserId"] as int? ?? 0;
                var command = new CreateModuleCommand(dto, currentUserId);
                var result = await _mediator.Send(command);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al crear módulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al crear módulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ?? Actualizar un módulo existente
        /// </summary>
        [HttpPut("{id}")]
        [RequirePermission("Configuration", "ManageModules")]
        public async Task<IActionResult> UpdateModule(int id, [FromBody] UpdateModuleDto dto)
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

                var currentUserId = HttpContext.Items["UserId"] as int? ?? 0;
                var command = new UpdateModuleCommand(id, dto, currentUserId);
                var result = await _mediator.Send(command);

                if (result.Error == 1)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al actualizar módulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al actualizar módulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ??? Eliminar un módulo (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [RequirePermission("Configuration", "ManageModules")]
        public async Task<IActionResult> DeleteModule(int id)
        {
            try
            {
                var currentUserId = HttpContext.Items["UserId"] as int? ?? 0;
                var command = new DeleteModuleCommand(id, currentUserId);
                var result = await _mediator.Send(command);

                if (result.Error == 1)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al eliminar módulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al eliminar módulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ?? SUBMÓDULOS - QUERIES

        /// <summary>
        /// ?? Obtener todos los submódulos de un módulo
        /// </summary>
        [HttpGet("{moduleId}/submodules")]
        [RequireAuthentication]
        public async Task<IActionResult> GetSubmodulesByModule(int moduleId, [FromQuery] bool includeInactive = false)
        {
            try
            {
                var query = new GetSubmodulesByModuleQuery(moduleId, includeInactive);
                var result = await _mediator.Send(query);

                if (result.Error == 1)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener submódulos: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener submódulos",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ?? Obtener un submódulo por ID
        /// </summary>
        [HttpGet("submodules/{id}")]
        [RequireAuthentication]
        public async Task<IActionResult> GetSubmoduleById(int id)
        {
            try
            {
                var query = new GetSubmoduleByIdQuery(id);
                var submodule = await _mediator.Send(query);

                if (submodule == null)
                {
                    return NotFound(new
                    {
                        message = "Submódulo no encontrado",
                        error = 1
                    });
                }

                return Ok(new
                {
                    message = "Submódulo obtenido exitosamente",
                    error = 0,
                    data = submodule
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener submódulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener submódulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ? SUBMÓDULOS - COMMANDS

        /// <summary>
        /// ? Crear un nuevo submódulo
        /// </summary>
        [HttpPost("submodules")]
        [RequirePermission("Configuration", "ManageModules")]
        public async Task<IActionResult> CreateSubmodule([FromBody] CreateSubmoduleDto dto)
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

                var currentUserId = HttpContext.Items["UserId"] as int? ?? 0;
                var command = new CreateSubmoduleCommand(dto, currentUserId);
                var result = await _mediator.Send(command);

                if (result.Error == 1)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al crear submódulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al crear submódulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ?? Actualizar un submódulo existente
        /// </summary>
        [HttpPut("submodules/{id}")]
        [RequirePermission("Configuration", "ManageModules")]
        public async Task<IActionResult> UpdateSubmodule(int id, [FromBody] UpdateSubmoduleDto dto)
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

                var currentUserId = HttpContext.Items["UserId"] as int? ?? 0;
                var command = new UpdateSubmoduleCommand(id, dto, currentUserId);
                var result = await _mediator.Send(command);

                if (result.Error == 1)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al actualizar submódulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al actualizar submódulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ??? Eliminar un submódulo (soft delete)
        /// </summary>
        [HttpDelete("submodules/{id}")]
        [RequirePermission("Configuration", "ManageModules")]
        public async Task<IActionResult> DeleteSubmodule(int id)
        {
            try
            {
                var currentUserId = HttpContext.Items["UserId"] as int? ?? 0;
                var command = new DeleteSubmoduleCommand(id, currentUserId);
                var result = await _mediator.Send(command);

                if (result.Error == 1)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al eliminar submódulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al eliminar submódulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion
    }
}
