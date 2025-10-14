using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Billing
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillingController : ControllerBase
    {
        [HttpGet("invoices")]
        [RequirePermission("Billing", "ViewInvoices")]
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

        [HttpPost("invoices")]
        [RequirePermission("Billing", "CreateInvoice")]
        public async Task<IActionResult> CreateInvoice([FromBody] dynamic invoiceData)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                return Ok(new
                {
                    message = "Invoice created successfully",
                    error = 0,
                    invoiceId = 101,
                    folio = "A-003",
                    serie = "A",
                    createdBy = userName,
                    status = "Borrador"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpGet("pending")]
        [RequirePermission("Billing", "ViewPending")]
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

        [HttpPost("invoices/{id}/stamp")]
        [RequirePermission("Billing", "ProcessStamping")]
        public async Task<IActionResult> StampInvoice(int id)
        {
            try
            {
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                return Ok(new
                {
                    message = "Invoice stamped successfully",
                    error = 0,
                    invoiceId = id,
                    uuid = "12345678-1234-1234-1234-123456789012",
                    stampedBy = userName,
                    stampedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpGet("stamped")]
        [RequirePermission("Billing", "ViewStamped")]
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
        [RequirePermission("Billing", "ManageCancellations")]
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
                    reason = "Cliente solicitó cancelación"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpGet("reports/summary")]
        [RequirePermission("Billing", "ViewReports")]
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
        [RequirePermission("Billing", "ManageCompanies")]
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