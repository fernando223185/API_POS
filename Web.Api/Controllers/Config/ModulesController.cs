using Application.Core.SystemModules.Commands;
using Application.Core.SystemModules.Queries;
using Application.DTOs.SystemModules;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Config
{
    /// <summary>
    /// CRUD completo para m�dulos y subm�dulos del sistema
    /// </summary>
    [Route("api/modules")]
    [ApiController]
    public class ModulesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ModulesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #region ?? M�DULOS - QUERIES

        /// <summary>
        /// ?? Obtener todos los m�dulos con sus subm�dulos
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
                Console.WriteLine($"? Error al obtener m�dulos: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener m�dulos",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ?? Obtener un m�dulo por ID con sus subm�dulos
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
                        message = "M�dulo no encontrado",
                        error = 1
                    });
                }

                return Ok(new
                {
                    message = "M�dulo obtenido exitosamente",
                    error = 0,
                    data = module
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener m�dulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener m�dulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ? M�DULOS - COMMANDS

        /// <summary>
        /// ? Crear un nuevo m�dulo
        /// </summary>
        [HttpPost]
        [RequirePermission("Configuracion", "Create")]
        public async Task<IActionResult> CreateModule([FromBody] CreateModuleDto dto)
        {
            try
            {
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

                var currentUserId = HttpContext.Items["UserId"] as int? ?? 0;
                var command = new CreateModuleCommand(dto, currentUserId);
                var result = await _mediator.Send(command);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al crear m�dulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al crear m�dulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ?? Actualizar un m�dulo existente
        /// </summary>
        [HttpPut("{id}")]
        [RequirePermission("Configuracion", "Edit")]
        public async Task<IActionResult> UpdateModule(int id, [FromBody] UpdateModuleDto dto)
        {
            try
            {
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
                Console.WriteLine($"? Error al actualizar m�dulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al actualizar m�dulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ??? Eliminar un m�dulo (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [RequirePermission("Configuracion", "Delete")]
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
                Console.WriteLine($"? Error al eliminar m�dulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al eliminar m�dulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ?? SUBM�DULOS - QUERIES

        /// <summary>
        /// ?? Obtener todos los subm�dulos de un m�dulo
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
                Console.WriteLine($"? Error al obtener subm�dulos: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener subm�dulos",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ?? Obtener un subm�dulo por ID
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
                        message = "Subm�dulo no encontrado",
                        error = 1
                    });
                }

                return Ok(new
                {
                    message = "Subm�dulo obtenido exitosamente",
                    error = 0,
                    data = submodule
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener subm�dulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener subm�dulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ? SUBM�DULOS - COMMANDS

        /// <summary>
        /// ? Crear un nuevo subm�dulo
        /// </summary>
        [HttpPost("submodules")]
        [RequirePermission("Configuracion", "Create")]
        public async Task<IActionResult> CreateSubmodule([FromBody] CreateSubmoduleDto dto)
        {
            try
            {
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
                Console.WriteLine($"? Error al crear subm�dulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al crear subm�dulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ?? Actualizar un subm�dulo existente
        /// </summary>
        [HttpPut("submodules/{id}")]
        [RequirePermission("Configuracion", "Edit")]
        public async Task<IActionResult> UpdateSubmodule(int id, [FromBody] UpdateSubmoduleDto dto)
        {
            try
            {
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
                Console.WriteLine($"? Error al actualizar subm�dulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al actualizar subm�dulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ??? Eliminar un subm�dulo (soft delete)
        /// </summary>
        [HttpDelete("submodules/{id}")]
        [RequirePermission("Configuracion", "Delete")]
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
                Console.WriteLine($"? Error al eliminar subm�dulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al eliminar subm�dulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion
    }
}
