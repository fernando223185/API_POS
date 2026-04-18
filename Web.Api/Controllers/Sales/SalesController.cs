using Application.Core.Sales.Commands;
using Application.Core.Sales.Queries;
using Application.DTOs.Sales;
using Application.Abstractions.Documents;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Sales
{
    /// <summary>
    /// Controlador de ventas con cobranza multi-forma de pago
    /// </summary>
    [Route("api/sales")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IThermalTicketService _thermalTicketService;
        private readonly ISaleDocumentService _saleDocumentService;

        public SalesController(
            IMediator mediator,
            IThermalTicketService thermalTicketService,
            ISaleDocumentService saleDocumentService)
        {
            _mediator = mediator;
            _thermalTicketService = thermalTicketService;
            _saleDocumentService = saleDocumentService;
        }

        /// <summary>
        /// Crear una nueva venta (estado Draft)
        /// </summary>
        [HttpPost]
        [RequirePermission("Ventas", "Create")]
        public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequestDto request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                Console.WriteLine($"?? Creando venta - Usuario: {userName}, Almac�n: {request.WarehouseId}, " +
                                $"Productos: {request.Details.Count}");

                var command = new CreateSaleCommand(request, userId);
                var result = await _mediator.Send(command);

                return Ok(new
                {
                    message = "Venta creada exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"? Not found: {ex.Message}");
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"? Validation error: {ex.Message}");
                return BadRequest(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error creating sale: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    message = "Error al crear venta",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Procesar pagos y completar la venta
        /// Descuenta inventario autom�ticamente
        /// </summary>
        [HttpPost("{saleId}/payments")]
        [RequirePermission("Ventas", "Create")]
        public async Task<IActionResult> ProcessPayments(
            int saleId,
            [FromBody] ProcessSalePaymentsRequestDto request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                Console.WriteLine($"?? Procesando pagos para venta {saleId} - Usuario: {userName}, " +
                                $"Formas de pago: {request.Payments.Count}");

                var command = new ProcessSalePaymentsCommand(saleId, request, userId);
                var result = await _mediator.Send(command);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"? Not found: {ex.Message}");
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"? Validation error: {ex.Message}");
                return BadRequest(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error processing payments: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    message = "Error al procesar pagos",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener venta por ID
        /// </summary>
        [HttpGet("{id}")]
        [RequirePermission("Ventas", "View")]
        public async Task<IActionResult> GetSale(int id)
        {
            try
            {
                var query = new GetSaleByIdQuery(id);
                var result = await _mediator.Send(query);

                return Ok(new
                {
                    message = "Venta obtenida exitosamente",
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
                Console.WriteLine($"? Error getting sale: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener venta",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener ventas paginadas con filtros
        /// </summary>
        [HttpGet]
        [RequirePermission("Ventas", "View")]
        public async Task<IActionResult> GetSales(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? warehouseId = null,
            [FromQuery] int? customerId = null,
            [FromQuery] int? userId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? status = null,
            [FromQuery] bool? isPaid = null,
            [FromQuery] bool? requiresInvoice = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                Console.WriteLine($"?? Getting sales - Page: {page}, PageSize: {pageSize}, " +
                                $"Warehouse: {warehouseId}, Status: {status}");

                var query = new GetSalesPagedQuery(
                    page,
                    pageSize,
                    warehouseId,
                    customerId,
                    userId,
                    fromDate,
                    toDate,
                    status,
                    isPaid,
                    requiresInvoice
                );

                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error getting sales: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener ventas",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener estad�sticas de ventas
        /// </summary>
        [HttpGet("statistics")]
        [RequirePermission("Ventas", "View")]
        public async Task<IActionResult> GetStatistics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? warehouseId = null)
        {
            try
            {
                var query = new GetSalesStatisticsQuery(fromDate, toDate, warehouseId);
                var result = await _mediator.Send(query);

                return Ok(new
                {
                    message = "Estadísticas obtenidas exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error getting statistics: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener estadísticas",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Cancelar una venta
        /// </summary>
        [HttpPut("{id}/cancel")]
        [RequirePermission("Ventas", "Edit")]
        public async Task<IActionResult> CancelSale(
            int id,
            [FromBody] CancelSaleRequestDto request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                Console.WriteLine($"? Cancelando venta {id} - Usuario: {userName}, Raz�n: {request.Reason}");

                var command = new CancelSaleCommand(id, request.Reason, userId);
                var result = await _mediator.Send(command);

                return Ok(new
                {
                    message = "Venta cancelada exitosamente",
                    error = 0,
                    data = result
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
                Console.WriteLine($"? Error cancelling sale: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al cancelar venta",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        // ========================================
        // DELIVERY ENDPOINTS
        // ========================================

        /// <summary>
        /// Crear una venta de tipo Delivery (entrega a domicilio / foránea).
        /// El pago se registra al confirmar la entrega.
        /// </summary>
        [HttpPost("delivery")]
        public async Task<IActionResult> CreateDeliverySale([FromBody] CreateSaleDeliveryRequestDto request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                if (userId == 0)
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });

                var command = new CreateSaleDeliveryCommand(request, userId);
                var result = await _mediator.Send(command);

                return Ok(new { message = "Venta Delivery creada exitosamente", error = 0, data = result });
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
                return StatusCode(500, new { message = "Error al crear venta Delivery", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Confirmar la entrega de una venta Delivery.
        /// Registra el pago, descuenta inventario y cierra la venta.
        /// </summary>
        [HttpPut("{id}/deliver")]
        public async Task<IActionResult> ConfirmDelivery(int id, [FromBody] ConfirmDeliveryRequestDto request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                if (userId == 0)
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });

                var command = new ConfirmDeliveryCommand(id, request, userId);
                var result = await _mediator.Send(command);

                return Ok(new { message = "Entrega confirmada exitosamente", error = 0, data = result });
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
                return StatusCode(500, new { message = "Error al confirmar entrega", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Obtener pagos de una venta
        /// </summary>
        [HttpGet("{saleId}/payments")]
        [RequirePermission("Ventas", "View")]
        public async Task<IActionResult> GetSalePayments(int saleId)
        {
            try
            {
                var query = new GetSaleByIdQuery(saleId);
                var sale = await _mediator.Send(query);

                return Ok(new
                {
                    message = "Pagos obtenidos exitosamente",
                    error = 0,
                    data = new
                    {
                        saleId = sale.Id,
                        saleCode = sale.Code,
                        total = sale.Total,
                        amountPaid = sale.AmountPaid,
                        changeAmount = sale.ChangeAmount,
                        payments = sale.Payments
                    }
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error getting payments: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener pagos",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Generar ticket t�rmico de venta (formato texto para impresoras t�rmicas)
        /// </summary>
        /// <param name="id">ID de la venta</param>
        /// <param name="width">Ancho del papel (48=80mm, 32=58mm)</param>
        [HttpGet("{id}/ticket/thermal")]
        [RequirePermission("Ventas", "View")]
        public async Task<IActionResult> GetThermalTicket(int id, [FromQuery] int width = 48)
        {
            try
            {
                Console.WriteLine($"?? Generando ticket t�rmico para venta {id} - Ancho: {width}");

                var ticketContent = await _thermalTicketService.GenerateSaleTicketAsync(id, width);

                return Ok(new
                {
                    message = "Ticket térmico generado exitosamente",
                    error = 0,
                    data = new
                    {
                        saleId = id,
                        content = ticketContent,
                        width = width,
                        format = "text/plain"
                    }
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error generating thermal ticket: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al generar ticket t�rmico",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Generar ticket t�rmico de venta (formato binario ESC/POS)
        /// Para enviar directamente a impresora t�rmica
        /// </summary>
        /// <param name="id">ID de la venta</param>
        /// <param name="width">Ancho del papel (48=80mm, 32=58mm)</param>
        [HttpGet("{id}/ticket/thermal/binary")]
        [RequirePermission("Ventas", "View")]
        public async Task<IActionResult> GetThermalTicketBinary(int id, [FromQuery] int width = 48)
        {
            try
            {
                Console.WriteLine($"??? Generando ticket t�rmico binario para venta {id}");

                var ticketBytes = await _thermalTicketService.GenerateSaleTicketBinaryAsync(id, width);

                return File(ticketBytes, "application/octet-stream", $"ticket-{id}.bin");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error generating thermal ticket binary: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al generar ticket t�rmico",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Generar ticket de venta en formato PDF
        /// Para impresoras l�ser o compartir digitalmente
        /// </summary>
        /// <param name="id">ID de la venta</param>
        /// <param name="includeLogo">Incluir logo de la empresa</param>
        [HttpGet("{id}/ticket/pdf")]
        [RequirePermission("Ventas", "View")]
        public async Task<IActionResult> GetTicketPdf(int id, [FromQuery] bool includeLogo = true)
        {
            try
            {
                Console.WriteLine($"?? Generando ticket PDF para venta {id}");

                var pdfBytes = await _saleDocumentService.GenerateSaleTicketPdfAsync(id, includeLogo);

                return File(pdfBytes, "application/pdf", $"ticket-{id}.pdf");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error generating PDF ticket: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al generar ticket PDF",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Generar factura de venta en formato PDF
        /// Requiere que la venta tenga RequiresInvoice = true
        /// </summary>
        /// <param name="id">ID de la venta</param>
        [HttpGet("{id}/invoice/pdf")]
        [RequirePermission("Ventas", "View")]
        public async Task<IActionResult> GetInvoicePdf(int id)
        {
            try
            {
                Console.WriteLine($"?? Generando factura PDF para venta {id}");

                var pdfBytes = await _saleDocumentService.GenerateSaleInvoicePdfAsync(id);

                return File(pdfBytes, "application/pdf", $"factura-{id}.pdf");
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
                Console.WriteLine($"? Error generating invoice PDF: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al generar factura PDF",
                    error = 2,
                    details = ex.Message
                });
            }
        }
    }
}