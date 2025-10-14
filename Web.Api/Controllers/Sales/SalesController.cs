using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Sales
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        [HttpPost]
        [RequirePermission("Sale", "CreateSale")]
        public async Task<IActionResult> CreateSale([FromBody] dynamic saleData)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                return Ok(new
                {
                    message = "Sale created successfully",
                    error = 0,
                    saleId = 1001,
                    saleNumber = "VTA-2025-001",
                    createdBy = userName,
                    total = 1250.00
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpGet]
        [RequirePermission("Sale", "ViewHistory")]
        public async Task<IActionResult> GetSales([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                return Ok(new
                {
                    message = "Sales retrieved successfully",
                    error = 0,
                    data = new[] {
                        new { 
                            id = 1, 
                            saleNumber = "VTA-2025-001", 
                            customerName = "Cliente 1", 
                            total = 1250.00,
                            date = DateTime.UtcNow.AddDays(-1),
                            status = "Completed"
                        },
                        new { 
                            id = 2, 
                            saleNumber = "VTA-2025-002", 
                            customerName = "Cliente 2", 
                            total = 875.50,
                            date = DateTime.UtcNow.AddHours(-3),
                            status = "Pending"
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

        [HttpGet("{id}")]
        [RequirePermission("Sale", "ViewHistory")]
        public async Task<IActionResult> GetSale(int id)
        {
            try
            {
                return Ok(new
                {
                    message = "Sale retrieved successfully",
                    error = 0,
                    data = new {
                        id,
                        saleNumber = $"VTA-2025-{id:D3}",
                        customerName = $"Cliente {id}",
                        items = new[] {
                            new { productId = 1, productName = "Producto 1", quantity = 2, unitPrice = 100.00, total = 200.00 },
                            new { productId = 2, productName = "Producto 2", quantity = 1, unitPrice = 150.00, total = 150.00 }
                        },
                        subtotal = 350.00,
                        tax = 56.00,
                        total = 406.00,
                        status = "Completed"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpPost("{id}/payment")]
        [RequirePermission("Sale", "ProcessPayment")]
        public async Task<IActionResult> ProcessPayment(int id, [FromBody] dynamic paymentData)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                return Ok(new
                {
                    message = "Payment processed successfully",
                    error = 0,
                    saleId = id,
                    paymentId = 2001,
                    processedBy = userName,
                    amount = 406.00,
                    method = "Cash"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpGet("today-summary")]
        [RequirePermission("Sale", "ViewHistory")]
        public async Task<IActionResult> GetTodaySummary()
        {
            try
            {
                return Ok(new
                {
                    message = "Today's sales summary retrieved successfully",
                    error = 0,
                    data = new {
                        totalSales = 15,
                        totalAmount = 12750.00,
                        averageTicket = 850.00,
                        cashSales = 8,
                        cardSales = 7,
                        topSellingProduct = "Producto 1"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpPost("{id}/cancel")]
        [RequirePermission("Sale", "Update")]
        public async Task<IActionResult> CancelSale(int id, [FromBody] dynamic cancelData)
        {
            try
            {
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                return Ok(new
                {
                    message = "Sale cancelled successfully",
                    error = 0,
                    saleId = id,
                    cancelledBy = userName,
                    reason = "Customer request"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }
    }
}