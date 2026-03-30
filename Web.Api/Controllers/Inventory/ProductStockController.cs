using Application.Core.Inventory.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Inventory
{
    /// <summary>
    /// Consulta el stock actual de productos por almacén.
    /// </summary>
    [ApiController]
    [Route("api/product-stock")]
    [Authorize]
    public class ProductStockController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductStockController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Lista paginada del stock de productos con filtros opcionales.
        /// </summary>
        [HttpGet]
        [RequirePermission("Inventario", "View")]
        public async Task<IActionResult> GetStock(
            [FromQuery] int? productId = null,
            [FromQuery] string? search = null,
            [FromQuery] int? warehouseId = null,
            [FromQuery] int? companyId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                pageSize = Math.Clamp(pageSize, 1, 200);

                var result = await _mediator.Send(new GetProductStockQuery
                {
                    ProductId = productId,
                    ProductSearch = search,
                    WarehouseId = warehouseId,
                    CompanyId = companyId,
                    Page = page,
                    PageSize = pageSize
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Todos los productos con su stock en un almacén específico.
        /// Útil para el formulario de traspasos (ver qué hay disponible en el origen).
        /// </summary>
        [HttpGet("by-warehouse/{warehouseId:int}")]
        [RequirePermission("Inventario", "View")]
        public async Task<IActionResult> GetByWarehouse(int warehouseId)
        {
            try
            {
                var result = await _mediator.Send(new GetProductStockByWarehouseQuery(warehouseId));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
