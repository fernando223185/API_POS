using Application.Core.Branch.Commands;
using Application.Core.Branch.Queries;
using Application.DTOs.Branch;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Config
{
    /// <summary>
    /// Controlador para gestión de sucursales/puntos de venta
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BranchesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BranchesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #region ?? CONSULTAS

        /// <summary>
        /// Obtener todas las sucursales
        /// </summary>
        /// <param name="includeInactive">Incluir sucursales inactivas</param>
        [HttpGet]
        [RequireAuthentication]
        public async Task<IActionResult> GetAllBranches([FromQuery] bool includeInactive = false)
        {
            try
            {
                var query = new GetAllBranchesQuery(includeInactive);
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener sucursales: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener sucursales",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener sucursales paginadas
        /// </summary>
        [HttpGet("paged")]
        [RequireAuthentication]
        public async Task<IActionResult> GetBranchesPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool includeInactive = false,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var query = new GetBranchesPagedQuery(pageNumber, pageSize, includeInactive, searchTerm);
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener sucursales paginadas: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener sucursales paginadas",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener sucursal por ID
        /// </summary>
        [HttpGet("{id}")]
        [RequireAuthentication]
        public async Task<IActionResult> GetBranchById(int id)
        {
            try
            {
                var query = new GetBranchByIdQuery(id);
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Sucursal no encontrada",
                        error = 1
                    });
                }

                return Ok(new
                {
                    message = "Sucursal obtenida exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener sucursal: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener sucursal",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener sucursal por código
        /// </summary>
        [HttpGet("code/{code}")]
        [RequireAuthentication]
        public async Task<IActionResult> GetBranchByCode(string code)
        {
            try
            {
                var query = new GetBranchByCodeQuery(code);
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Sucursal no encontrada",
                        error = 1
                    });
                }

                return Ok(new
                {
                    message = "Sucursal obtenida exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener sucursal por código: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener sucursal",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ? CREAR

        /// <summary>
        /// Crear una nueva sucursal (el código se genera automáticamente)
        /// </summary>
        [HttpPost]
        [RequireAuthentication]
        public async Task<IActionResult> CreateBranch([FromBody] CreateBranchDto branchDto)
        {
            try
            {
                // Obtener ID del usuario autenticado
                var userId = HttpContext.Items["UserId"] as int? ?? 0;

                var command = new CreateBranchCommand(branchDto, userId);
                var result = await _mediator.Send(command);

                Console.WriteLine($"? Sucursal creada: {result.Code} - {result.Name}");

                return Ok(new
                {
                    message = "Sucursal creada exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al crear sucursal: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al crear sucursal",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ?? ACTUALIZAR

        /// <summary>
        /// Actualizar una sucursal existente
        /// </summary>
        [HttpPut("{id}")]
        [RequireAuthentication]
        public async Task<IActionResult> UpdateBranch(int id, [FromBody] UpdateBranchDto branchDto)
        {
            try
            {
                // Obtener ID del usuario autenticado
                var userId = HttpContext.Items["UserId"] as int? ?? 0;

                var command = new UpdateBranchCommand(id, branchDto, userId);
                var result = await _mediator.Send(command);

                Console.WriteLine($"? Sucursal actualizada: {result.Code} - {result.Name}");

                return Ok(new
                {
                    message = "Sucursal actualizada exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message,
                    error = 1
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al actualizar sucursal: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al actualizar sucursal",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ??? BAJA LÓGICA

        /// <summary>
        /// Dar de baja lógica una sucursal (desactivar)
        /// </summary>
        [HttpDelete("{id}")]
        [RequireAuthentication]
        public async Task<IActionResult> DeactivateBranch(int id)
        {
            try
            {
                // Obtener ID del usuario autenticado
                var userId = HttpContext.Items["UserId"] as int? ?? 0;

                var command = new DeactivateBranchCommand(id, userId);
                var result = await _mediator.Send(command);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Sucursal no encontrada",
                        error = 1
                    });
                }

                Console.WriteLine($"? Sucursal desactivada: ID {id}");

                return Ok(new
                {
                    message = "Sucursal desactivada exitosamente",
                    error = 0,
                    branchId = id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al desactivar sucursal: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al desactivar sucursal",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Reactivar una sucursal desactivada
        /// </summary>
        [HttpPatch("{id}/reactivate")]
        [RequireAuthentication]
        public async Task<IActionResult> ReactivateBranch(int id)
        {
            try
            {
                // Obtener ID del usuario autenticado
                var userId = HttpContext.Items["UserId"] as int? ?? 0;

                var command = new ReactivateBranchCommand(id, userId);
                var result = await _mediator.Send(command);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Sucursal no encontrada",
                        error = 1
                    });
                }

                Console.WriteLine($"? Sucursal reactivada: ID {id}");

                return Ok(new
                {
                    message = "Sucursal reactivada exitosamente",
                    error = 0,
                    branchId = id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al reactivar sucursal: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al reactivar sucursal",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion
    }
}
