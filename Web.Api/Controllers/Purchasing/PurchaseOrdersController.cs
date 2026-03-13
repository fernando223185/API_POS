using Application.Core.PurchaseOrder.Commands;
using Application.Core.PurchaseOrder.Queries;
using Application.DTOs.PurchaseOrder;
using Application.Abstractions.Documents;  // ? NUEVO
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;  // ? CAMBIADO
using Web.Api.Authorization;

namespace Web.Api.Controllers.Purchasing
{
    /// <summary>
    /// Controlador para gestión de órdenes de compra
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]  // ? Aplicar a nivel de controlador
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IPurchaseDocumentService _documentService;  // ? NUEVO

        public PurchaseOrdersController(IMediator mediator, IPurchaseDocumentService documentService)
        {
            _mediator = mediator;
            _documentService = documentService;  // ? NUEVO
        }

        #region ?? CONSULTAS

        /// <summary>
        /// Obtener todas las órdenes de compra
        /// </summary>
        [HttpGet]
        [RequireAuthentication]
        public async Task<IActionResult> GetAllPurchaseOrders([FromQuery] bool includeInactive = false)
        {
            try
            {
                var query = new GetAllPurchaseOrdersQuery(includeInactive);
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener órdenes de compra: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener órdenes de compra",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener órdenes de compra paginadas con filtros
        /// </summary>
        [HttpGet("paged")]
        [RequireAuthentication]
        public async Task<IActionResult> GetPurchaseOrdersPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool includeInactive = false,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? status = null,
            [FromQuery] int? supplierId = null,
            [FromQuery] int? warehouseId = null)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var query = new GetPurchaseOrdersPagedQuery(
                    pageNumber, pageSize, includeInactive, searchTerm, status, supplierId, warehouseId);
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener órdenes paginadas: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener órdenes de compra",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener orden de compra por ID
        /// </summary>
        [HttpGet("{id}")]
        [RequireAuthentication]
        public async Task<IActionResult> GetPurchaseOrderById(int id)
        {
            try
            {
                var query = new GetPurchaseOrderByIdQuery(id);
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Orden de compra no encontrada",
                        error = 1
                    });
                }

                return Ok(new
                {
                    message = "Orden de compra obtenida exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener orden de compra: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener orden de compra",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener orden de compra por código
        /// </summary>
        [HttpGet("code/{code}")]
        [RequireAuthentication]
        public async Task<IActionResult> GetPurchaseOrderByCode(string code)
        {
            try
            {
                var query = new GetPurchaseOrderByCodeQuery(code);
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Orden de compra no encontrada",
                        error = 1
                    });
                }

                return Ok(new
                {
                    message = "Orden de compra obtenida exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener orden de compra: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener orden de compra",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener órdenes pendientes de recibir
        /// </summary>
        [HttpGet("pending-to-receive")]
        [RequireAuthentication]
        public async Task<IActionResult> GetPendingToReceive()
        {
            try
            {
                var query = new GetPendingToReceiveOrdersQuery();
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener órdenes pendientes: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener órdenes pendientes",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ? CREAR

        /// <summary>
        /// Crear una nueva orden de compra (el código se genera automáticamente)
        /// </summary>
        [HttpPost]
        [RequireAuthentication]
        public async Task<IActionResult> CreatePurchaseOrder([FromBody] CreatePurchaseOrderDto purchaseOrderDto)
        {
            try
            {
                // Obtener ID del usuario autenticado
                var userId = HttpContext.Items["UserId"] as int? ?? 0;

                var command = new CreatePurchaseOrderCommand(purchaseOrderDto, userId);
                var result = await _mediator.Send(command);

                Console.WriteLine($"? Orden de compra creada: {result.Code}");

                return Ok(new
                {
                    message = "Orden de compra creada exitosamente",
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
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    error = 1
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al crear orden de compra: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al crear orden de compra",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ?? CAMBIOS DE ESTADO

        /// <summary>
        /// Aprobar orden de compra
        /// </summary>
        [HttpPatch("{id}/approve")]
        [RequireAuthentication]
        public async Task<IActionResult> ApprovePurchaseOrder(int id, [FromBody] ChangeStatusDto? statusDto = null)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;

                var command = new ApprovePurchaseOrderCommand(id, userId, statusDto?.Notes);
                var result = await _mediator.Send(command);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Orden de compra no encontrada",
                        error = 1
                    });
                }

                Console.WriteLine($"? Orden de compra aprobada: ID {id}");

                return Ok(new
                {
                    message = "Orden de compra aprobada exitosamente",
                    error = 0,
                    purchaseOrderId = id
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
                Console.WriteLine($"? Error al aprobar orden: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al aprobar orden de compra",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Marcar orden como en tránsito
        /// </summary>
        [HttpPatch("{id}/in-transit")]
        [RequireAuthentication]
        public async Task<IActionResult> MarkAsInTransit(int id, [FromBody] ChangeStatusDto? statusDto = null)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;

                var command = new MarkAsInTransitCommand(id, userId, statusDto?.Notes);
                var result = await _mediator.Send(command);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Orden de compra no encontrada",
                        error = 1
                    });
                }

                Console.WriteLine($"? Orden marcada como en tránsito: ID {id}");

                return Ok(new
                {
                    message = "Orden marcada como en tránsito exitosamente",
                    error = 0,
                    purchaseOrderId = id
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
                Console.WriteLine($"? Error al marcar orden: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al marcar orden como en tránsito",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Cancelar orden de compra
        /// </summary>
        [HttpDelete("{id}")]
        [RequireAuthentication]
        public async Task<IActionResult> CancelPurchaseOrder(int id, [FromBody] ChangeStatusDto? statusDto = null)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;

                var command = new CancelPurchaseOrderCommand(id, userId, statusDto?.Notes);
                var result = await _mediator.Send(command);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Orden de compra no encontrada",
                        error = 1
                    });
                }

                Console.WriteLine($"? Orden de compra cancelada: ID {id}");

                return Ok(new
                {
                    message = "Orden de compra cancelada exitosamente",
                    error = 0,
                    purchaseOrderId = id
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
                Console.WriteLine($"? Error al cancelar orden: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al cancelar orden de compra",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ?? DOCUMENTOS PDF

        /// <summary>
        /// Generar y descargar PDF de Orden de Compra
        /// </summary>
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> DownloadPurchaseOrderPdf(int id)
        {
            try
            {
                var pdfBytes = await _documentService.GeneratePurchaseOrderPdfAsync(id);

                return File(pdfBytes, "application/pdf", $"OrdenCompra-{id}.pdf");
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
                Console.WriteLine($"? Error al generar PDF: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al generar PDF",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Generar PDF y guardar en S3 (retorna URL)
        /// </summary>
        [HttpPost("{id}/pdf/save")]
        public async Task<IActionResult> SavePurchaseOrderPdfToS3(int id)
        {
            try
            {
                var pdfUrl = await _documentService.SavePurchaseOrderPdfToS3Async(id);

                Console.WriteLine($"? PDF guardado en S3: {pdfUrl}");

                return Ok(new
                {
                    message = "PDF generado y guardado exitosamente",
                    error = 0,
                    pdfUrl
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
                Console.WriteLine($"? Error al guardar PDF: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al guardar PDF",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion
    }
}
