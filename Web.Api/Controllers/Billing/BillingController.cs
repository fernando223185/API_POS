using Application.Core.Billing.Commands;
using Application.Core.Billing.Queries;
using Application.DTOs.Billing;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Billing
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BillingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtener ventas pendientes de timbrar (facturar)
        /// </summary>
        /// <param name="page">N�mero de p�gina</param>
        /// <param name="pageSize">Tama�o de p�gina</param>
        /// <param name="onlyRequiresInvoice">Filtrar solo ventas que requieren factura (true), solo las que no requieren (false), o todas (null)</param>
        /// <param name="warehouseId">Filtrar por almac�n</param>
        /// <param name="branchId">Filtrar por sucursal</param>
        /// <param name="companyId">Filtrar por empresa</param>
        /// <param name="fromDate">Fecha desde</param>
        /// <param name="toDate">Fecha hasta</param>
        [HttpGet("pending-sales")]
        [RequirePermission("CFDI", "View")]
        public async Task<IActionResult> GetPendingInvoiceSales(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? onlyRequiresInvoice = null,
            [FromQuery] int? warehouseId = null,
            [FromQuery] int? branchId = null,
            [FromQuery] int? companyId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var query = new GetPendingInvoiceSalesQuery(
                    page,
                    pageSize,
                    onlyRequiresInvoice,
                    warehouseId,
                    branchId,
                    companyId,
                    fromDate,
                    toDate
                );

                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al obtener ventas pendientes de timbrar",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener una venta espec�fica para facturaci�n
        /// Incluye toda la informaci�n necesaria para generar CFDI: empresa, cliente, productos, pagos
        /// </summary>
        /// <param name="saleId">ID de la venta</param>
        [HttpGet("sale/{saleId}")]
        [RequirePermission("CFDI", "View")]
        public async Task<IActionResult> GetSaleForInvoicing(int saleId)
        {
            try
            {
                var query = new GetSaleForInvoicingQuery(saleId);
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
                return StatusCode(500, new
                {
                    message = "Error al obtener venta para facturaci�n",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Timbrar un CFDI con el PAC Sapiens
        /// Genera el CFDI 4.0 a partir de una venta y lo env�a a timbrar
        /// </summary>
        /// <param name="requestData">Datos del timbrado (SaleId, Serie, Folio, FormaPago, etc.)</param>
        [HttpPost("timbrar")]
        [RequirePermission("CFDI", "Create")]
        public async Task<IActionResult> TimbrarCfdi([FromBody] TimbrarCfdiRequestDto requestData)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;

                var command = new TimbrarCfdiCommand(requestData, userId);
                var result = await _mediator.Send(command);

                if (result.Error > 0)
                {
                    if (result.Error == 1)
                    {
                        return BadRequest(result);
                    }
                    return StatusCode(500, result);
                }

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
                return StatusCode(500, new
                {
                    message = "Error al timbrar CFDI",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        [HttpGet("invoices")]
        [RequirePermission("CFDI", "View")]
        public async Task<IActionResult> GetInvoices([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                return Ok(new
                {
                    message = "Invoices retrieved successfully",
                    error = 0,
                    data = new[] {
                        new {
                            id = 1,
                            folio = "A-001",
                            serie = "A",
                            customerName = "Cliente 1",
                            rfc = "XAXX010101000",
                            total = 1160.00,
                            status = "Timbrada",
                            createdAt = DateTime.UtcNow.AddDays(-2)
                        },
                        new {
                            id = 2,
                            folio = "A-002",
                            serie = "A",
                            customerName = "Cliente 2",
                            rfc = "YAYY020202000",
                            total = 2320.00,
                            status = "Pendiente",
                            createdAt = DateTime.UtcNow.AddDays(-1)
                        }
                    },
                    pagination = new { page, pageSize, totalItems = 2 }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Crear una nueva factura (borrador o timbrada)
        /// Puede crear desde una venta existente o manualmente
        /// </summary>
        /// <param name="request">Datos de la factura (SaleId opcional, TimbrarInmediatamente, etc.)</param>
        [HttpPost("invoices")]
        [RequirePermission("CFDI", "Create")]
        public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequestDto request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;

                var command = new CreateInvoiceCommand
                {
                    Request = request,
                    UserId = userId
                };

                var result = await _mediator.Send(command);

                if (result.Error > 0)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al crear factura",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        [HttpGet("pending")]
        [RequirePermission("CFDI", "View")]
        public async Task<IActionResult> GetPendingInvoices()
        {
            try
            {
                return Ok(new
                {
                    message = "Pending invoices retrieved successfully",
                    error = 0,
                    data = new[] {
                        new {
                            id = 2,
                            folio = "A-002",
                            customerName = "Cliente 2",
                            total = 2320.00,
                            daysWaiting = 1
                        }
                    },
                    totalPending = 1
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Timbrar una factura borrador existente con el PAC Sapiens
        /// </summary>
        /// <param name="id">ID de la factura a timbrar</param>
        /// <param name="request">Parámetros adicionales (versión de respuesta)</param>
        [HttpPost("invoices/{id}/stamp")]
        [RequirePermission("CFDI", "Create")]
        public async Task<IActionResult> StampInvoice(int id, [FromBody] TimbrarInvoiceRequestDto? request = null)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;

                var timbrarRequest = request ?? new TimbrarInvoiceRequestDto
                {
                    InvoiceId = id,
                    Version = "v4"
                };

                // Asegurar que el ID del request coincida con el de la ruta
                timbrarRequest.InvoiceId = id;

                var command = new TimbrarInvoiceCommand
                {
                    Request = timbrarRequest,
                    UserId = userId
                };

                var result = await _mediator.Send(command);

                if (result.Error > 0)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al timbrar factura",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        [HttpGet("stamped")]
        [RequirePermission("CFDI", "View")]
        public async Task<IActionResult> GetStampedInvoices([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                return Ok(new
                {
                    message = "Stamped invoices retrieved successfully",
                    error = 0,
                    data = new[] {
                        new {
                            id = 1,
                            folio = "A-001",
                            customerName = "Cliente 1",
                            total = 1160.00,
                            uuid = "87654321-4321-4321-4321-210987654321",
                            stampedAt = DateTime.UtcNow.AddDays(-2)
                        }
                    },
                    totalStamped = 1,
                    period = $"{startDate?.ToString("yyyy-MM-dd")} to {endDate?.ToString("yyyy-MM-dd")}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpPost("invoices/{id}/cancel")]
        [RequirePermission("CFDI", "Delete")]
        public async Task<IActionResult> CancelInvoice(int id, [FromBody] dynamic cancelData)
        {
            try
            {
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                return Ok(new
                {
                    message = "Invoice cancelled successfully",
                    error = 0,
                    invoiceId = id,
                    cancelledBy = userName,
                    cancelledAt = DateTime.UtcNow,
                    reason = "Cliente solicit� cancelaci�n"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpGet("reports/summary")]
        [RequirePermission("CFDI", "View")]
        public async Task<IActionResult> GetBillingSummary([FromQuery] int year = 2025, [FromQuery] int month = 1)
        {
            try
            {
                return Ok(new
                {
                    message = "Billing summary retrieved successfully",
                    error = 0,
                    period = $"{year}-{month:D2}",
                    data = new {
                        totalInvoices = 45,
                        totalAmount = 125000.00,
                        stampedInvoices = 42,
                        pendingInvoices = 2,
                        cancelledInvoices = 1,
                        averageInvoiceAmount = 2777.78
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpGet("companies")]
        [RequirePermission("CFDI", "View")]
        public async Task<IActionResult> GetCompanies()
        {
            try
            {
                return Ok(new
                {
                    message = "Companies retrieved successfully",
                    error = 0,
                    data = new[] {
                        new {
                            id = 1,
                            rfc = "AAA010101AAA",
                            businessName = "Mi Empresa SA de CV",
                            isActive = true,
                            certificateExpiry = DateTime.UtcNow.AddMonths(6)
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }
    }
}