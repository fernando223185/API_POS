using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Products
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        [HttpGet]
        [RequirePermission("Product", "ViewCatalog")]
        public async Task<IActionResult> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                
                // Aquí implementarías la lógica real
                return Ok(new
                {
                    message = "Products retrieved successfully",
                    error = 0,
                    userId = userId,
                    data = new[] {
                        new { id = 1, name = "Producto 1", code = "PROD001", price = 100.00 },
                        new { id = 2, name = "Producto 2", code = "PROD002", price = 150.00 }
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
        [RequirePermission("Product", "ViewCatalog")]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                return Ok(new
                {
                    message = "Product retrieved successfully",
                    error = 0,
                    data = new { id, name = $"Producto {id}", code = $"PROD{id:D3}", price = 100.00 * id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpPost]
        [RequirePermission("Product", "Create")]
        public async Task<IActionResult> CreateProduct([FromBody] dynamic productData)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                return Ok(new
                {
                    message = "Product created successfully",
                    error = 0,
                    productId = 123,
                    createdBy = userName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [RequirePermission("Product", "Update")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] dynamic productData)
        {
            try
            {
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                return Ok(new
                {
                    message = "Product updated successfully",
                    error = 0,
                    productId = id,
                    updatedBy = userName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [RequirePermission("Product", "Delete")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                return Ok(new
                {
                    message = "Product deleted successfully",
                    error = 0,
                    productId = id,
                    deletedBy = userName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpGet("categories")]
        [RequirePermission("Product", "ManageCategories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                return Ok(new
                {
                    message = "Categories retrieved successfully",
                    error = 0,
                    data = new[] {
                        new { id = 1, name = "Electrónicos", description = "Productos electrónicos" },
                        new { id = 2, name = "Ropa", description = "Prendas de vestir" }
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpGet("low-stock")]
        [RequirePermission("Product", "ManageAlerts")]
        public async Task<IActionResult> GetLowStockProducts()
        {
            try
            {
                return Ok(new
                {
                    message = "Low stock products retrieved successfully",
                    error = 0,
                    data = new[] {
                        new { id = 1, name = "Producto 1", currentStock = 2, minimumStock = 5 },
                        new { id = 3, name = "Producto 3", currentStock = 1, minimumStock = 10 }
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