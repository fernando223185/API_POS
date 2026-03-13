using Application.Core.Warehouse.Commands;
using Application.Core.Warehouse.Queries;
using Application.DTOs.Warehouse;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Config
{
    /// <summary>
    /// Controlador para gestión de almacenes
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class WarehousesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WarehousesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #region ?? CONSULTAS

        /// <summary>
        /// Obtener todos los almacenes
        /// </summary>
        /// <param name="includeInactive">Incluir almacenes inactivos</param>
        [HttpGet]
        [RequireAuthentication]
        public async Task<IActionResult> GetAllWarehouses([FromQuery] bool includeInactive = false)
        {
            try
            {
                var query = new GetAllWarehousesQuery(includeInactive);
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener almacenes: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener almacenes",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener almacenes paginados con filtros
        /// </summary>
        [HttpGet("paged")]
        [RequireAuthentication]
        public async Task<IActionResult> GetWarehousesPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool includeInactive = false,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? branchId = null)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var query = new GetWarehousesPagedQuery(pageNumber, pageSize, includeInactive, searchTerm, branchId);
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener almacenes paginados: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener almacenes paginados",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener almacén por ID
        /// </summary>
        [HttpGet("{id}")]
        [RequireAuthentication]
        public async Task<IActionResult> GetWarehouseById(int id)
        {
            try
            {
                var query = new GetWarehouseByIdQuery(id);
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Almacén no encontrado",
                        error = 1
                    });
                }

                return Ok(new
                {
                    message = "Almacén obtenido exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener almacén: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener almacén",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener almacén por código
        /// </summary>
        [HttpGet("code/{code}")]
        [RequireAuthentication]
        public async Task<IActionResult> GetWarehouseByCode(string code)
        {
            try
            {
                var query = new GetWarehouseByCodeQuery(code);
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Almacén no encontrado",
                        error = 1
                    });
                }

                return Ok(new
                {
                    message = "Almacén obtenido exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener almacén por código: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener almacén",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener almacenes de una sucursal específica
        /// </summary>
        [HttpGet("branch/{branchId}")]
        [RequireAuthentication]
        public async Task<IActionResult> GetWarehousesByBranch(int branchId, [FromQuery] bool includeInactive = false)
        {
            try
            {
                var query = new GetWarehousesByBranchQuery(branchId, includeInactive);
                var result = await _mediator.Send(query);

                return Ok(result);
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
                Console.WriteLine($"? Error al obtener almacenes de sucursal: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener almacenes de la sucursal",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ? CREAR

        /// <summary>
        /// Crear un nuevo almacén (el código se genera automáticamente)
        /// </summary>
        [HttpPost]
        [RequireAuthentication]
        public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseDto warehouseDto)
        {
            try
            {
                // Obtener ID del usuario autenticado
                var userId = HttpContext.Items["UserId"] as int? ?? 0;

                var command = new CreateWarehouseCommand(warehouseDto, userId);
                var result = await _mediator.Send(command);

                Console.WriteLine($"? Almacén creado: {result.Code} - {result.Name} (Sucursal: {result.BranchName})");

                return Ok(new
                {
                    message = "Almacén creado exitosamente",
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
                Console.WriteLine($"? Error al crear almacén: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al crear almacén",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ?? ACTUALIZAR

        /// <summary>
        /// Actualizar un almacén existente
        /// </summary>
        [HttpPut("{id}")]
        [RequireAuthentication]
        public async Task<IActionResult> UpdateWarehouse(int id, [FromBody] UpdateWarehouseDto warehouseDto)
        {
            try
            {
                // Obtener ID del usuario autenticado
                var userId = HttpContext.Items["UserId"] as int? ?? 0;

                var command = new UpdateWarehouseCommand(id, warehouseDto, userId);
                var result = await _mediator.Send(command);

                Console.WriteLine($"? Almacén actualizado: {result.Code} - {result.Name}");

                return Ok(new
                {
                    message = "Almacén actualizado exitosamente",
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
                Console.WriteLine($"? Error al actualizar almacén: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al actualizar almacén",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ??? BAJA LÓGICA

        /// <summary>
        /// Dar de baja lógica un almacén (desactivar)
        /// </summary>
        [HttpDelete("{id}")]
        [RequireAuthentication]
        public async Task<IActionResult> DeactivateWarehouse(int id)
        {
            try
            {
                // Obtener ID del usuario autenticado
                var userId = HttpContext.Items["UserId"] as int? ?? 0;

                var command = new DeactivateWarehouseCommand(id, userId);
                var result = await _mediator.Send(command);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Almacén no encontrado",
                        error = 1
                    });
                }

                Console.WriteLine($"? Almacén desactivado: ID {id}");

                return Ok(new
                {
                    message = "Almacén desactivado exitosamente",
                    error = 0,
                    warehouseId = id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al desactivar almacén: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al desactivar almacén",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Reactivar un almacén desactivado
        /// </summary>
        [HttpPatch("{id}/reactivate")]
        [RequireAuthentication]
        public async Task<IActionResult> ReactivateWarehouse(int id)
        {
            try
            {
                // Obtener ID del usuario autenticado
                var userId = HttpContext.Items["UserId"] as int? ?? 0;

                var command = new ReactivateWarehouseCommand(id, userId);
                var result = await _mediator.Send(command);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Almacén no encontrado",
                        error = 1
                    });
                }

                Console.WriteLine($"? Almacén reactivado: ID {id}");

                return Ok(new
                {
                    message = "Almacén reactivado exitosamente",
                    error = 0,
                    warehouseId = id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al reactivar almacén: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al reactivar almacén",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion
    }
}
