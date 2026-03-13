using Application.Core.PurchaseOrderReceiving.Commands;
using Application.Core.PurchaseOrderReceiving.Queries;
using Application.DTOs.PurchaseOrderReceiving;
using Application.Abstractions.Documents;  // ? NUEVO
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Web.Api.Controllers.Purchasing
{
    /// <summary>
    /// Controlador para gestión de recepciones de mercancía
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PurchaseOrderReceivingsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IPurchaseDocumentService _documentService;  // ? NUEVO

        public PurchaseOrderReceivingsController(IMediator mediator, IPurchaseDocumentService documentService)
        {
            _mediator = mediator;
            _documentService = documentService;  // ? NUEVO
        }

        #region ?? CONSULTAS

        /// <summary>
        /// Obtener todas las recepciones
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllReceivings([FromQuery] bool includePosted = true)
        {
            try
            {
                var query = new GetAllReceivingsQuery(includePosted);
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener recepciones: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener recepciones",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener recepciones paginadas con filtros
        /// </summary>
        [HttpGet("paged")]
        public async Task<IActionResult> GetReceivingsPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? purchaseOrderId = null,
            [FromQuery] int? warehouseId = null,
            [FromQuery] string? status = null,
            [FromQuery] bool? onlyPendingToPost = null)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var query = new GetReceivingsPagedQuery(
                    pageNumber,
                    pageSize,
                    searchTerm,
                    purchaseOrderId,
                    warehouseId,
                    status,
                    onlyPendingToPost);

                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener recepciones paginadas: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener recepciones",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener recepción por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReceivingById(int id)
        {
            try
            {
                var query = new GetReceivingByIdQuery(id);
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Recepción no encontrada",
                        error = 1
                    });
                }

                return Ok(new
                {
                    message = "Recepción obtenida exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener recepción: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener recepción",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener recepciones de una orden de compra específica
        /// </summary>
        [HttpGet("purchase-order/{purchaseOrderId}")]
        public async Task<IActionResult> GetReceivingsByPurchaseOrder(int purchaseOrderId)
        {
            try
            {
                var query = new GetReceivingsByPurchaseOrderQuery(purchaseOrderId);
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener recepciones de OC: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener recepciones",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener recepciones pendientes de aplicar al inventario
        /// </summary>
        [HttpGet("pending-to-post")]
        public async Task<IActionResult> GetPendingToPost()
        {
            try
            {
                var query = new GetPendingToPostQuery();
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener recepciones pendientes: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener recepciones pendientes",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ? CREAR

        /// <summary>
        /// Crear nueva recepción de mercancía
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateReceiving([FromBody] CreatePurchaseOrderReceivingDto receivingDto)
        {
            try
            {
                // ? Obtener userId desde Claims del token JWT
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                var command = new CreatePurchaseOrderReceivingCommand(receivingDto, userId);
                var result = await _mediator.Send(command);

                Console.WriteLine($"? Recepción creada: {result.Code} - OC: {result.PurchaseOrderCode}");

                return Ok(new
                {
                    message = "Recepción creada exitosamente",
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
                Console.WriteLine($"? Error al crear recepción: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al crear recepción",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ? APLICAR A INVENTARIO (CRÍTICO)

        /// <summary>
        /// Aplicar recepción al inventario (crear movimientos y actualizar stock)
        /// </summary>
        [HttpPost("{id}/post-to-inventory")]
        public async Task<IActionResult> PostToInventory(int id)
        {
            try
            {
                // ? Obtener userId desde Claims del token JWT
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                var command = new PostToInventoryCommand(id, userId);
                var result = await _mediator.Send(command);

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
                Console.WriteLine($"? Error al aplicar recepción a inventario: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al aplicar recepción a inventario",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ?? DOCUMENTOS PDF

        /// <summary>
        /// Generar y descargar PDF de Recibo de Mercancía
        /// </summary>
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> DownloadReceivingPdf(int id)
        {
            try
            {
                var pdfBytes = await _documentService.GenerateReceivingPdfAsync(id);

                return File(pdfBytes, "application/pdf", $"ReciboMercancia-{id}.pdf");
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
        public async Task<IActionResult> SaveReceivingPdfToS3(int id)
        {
            try
            {
                var pdfUrl = await _documentService.SaveReceivingPdfToS3Async(id);

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
