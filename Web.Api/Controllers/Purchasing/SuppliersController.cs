using Application.Core.Supplier.Commands;
using Application.Core.Supplier.Queries;
using Application.DTOs.Supplier;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Purchasing
{
    /// <summary>
    /// Controlador para gestión de proveedores
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SuppliersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SuppliersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #region ?? CONSULTAS

        /// <summary>
        /// Obtener todos los proveedores
        /// </summary>
        [HttpGet]
        [RequireAuthentication]
        public async Task<IActionResult> GetAllSuppliers([FromQuery] bool includeInactive = false)
        {
            try
            {
                var query = new GetAllSuppliersQuery(includeInactive);
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener proveedores: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener proveedores",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener proveedores paginados con filtros
        /// </summary>
        [HttpGet("paged")]
        [RequireAuthentication]
        public async Task<IActionResult> GetSuppliersPageds(
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

                var query = new GetSuppliersPagedQuery(pageNumber, pageSize, includeInactive, searchTerm);
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener proveedores paginados: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener proveedores",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener proveedor por ID
        /// </summary>
        [HttpGet("{id}")]
        [RequireAuthentication]
        public async Task<IActionResult> GetSupplierById(int id)
        {
            try
            {
                var query = new GetSupplierByIdQuery(id);
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Proveedor no encontrado",
                        error = 1
                    });
                }

                return Ok(new
                {
                    message = "Proveedor obtenido exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener proveedor: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener proveedor",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener proveedor por código
        /// </summary>
        [HttpGet("code/{code}")]
        [RequireAuthentication]
        public async Task<IActionResult> GetSupplierByCode(string code)
        {
            try
            {
                var query = new GetSupplierByCodeQuery(code);
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Proveedor no encontrado",
                        error = 1
                    });
                }

                return Ok(new
                {
                    message = "Proveedor obtenido exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener proveedor: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener proveedor",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ? CREAR

        /// <summary>
        /// Crear un nuevo proveedor (el código se genera automáticamente)
        /// </summary>
        [HttpPost]
        [RequireAuthentication]
        public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierDto supplierDto)
        {
            try
            {
                var command = new CreateSupplierCommand(supplierDto);
                var result = await _mediator.Send(command);

                Console.WriteLine($"? Proveedor creado: {result.Code} - {result.Name}");

                return Ok(new
                {
                    message = "Proveedor creado exitosamente",
                    error = 0,
                    data = result
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
                Console.WriteLine($"? Error al crear proveedor: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al crear proveedor",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ?? ACTUALIZAR

        /// <summary>
        /// Actualizar proveedor existente
        /// </summary>
        [HttpPut("{id}")]
        [RequireAuthentication]
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] UpdateSupplierDto supplierDto)
        {
            try
            {
                var command = new UpdateSupplierCommand(id, supplierDto);
                var result = await _mediator.Send(command);

                Console.WriteLine($"? Proveedor actualizado: {result.Code} - {result.Name}");

                return Ok(new
                {
                    message = "Proveedor actualizado exitosamente",
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
                Console.WriteLine($"? Error al actualizar proveedor: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al actualizar proveedor",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ??? ELIMINAR

        /// <summary>
        /// Eliminar (desactivar) proveedor
        /// </summary>
        [HttpDelete("{id}")]
        [RequireAuthentication]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            try
            {
                var command = new DeleteSupplierCommand(id);
                var result = await _mediator.Send(command);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Proveedor no encontrado",
                        error = 1
                    });
                }

                Console.WriteLine($"? Proveedor desactivado: ID {id}");

                return Ok(new
                {
                    message = "Proveedor desactivado exitosamente",
                    error = 0,
                    supplierId = id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al eliminar proveedor: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al eliminar proveedor",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion
    }
}
