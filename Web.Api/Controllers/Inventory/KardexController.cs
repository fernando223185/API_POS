using Application.Core.Inventory.Queries;
using Application.DTOs.Inventory;
using Application.Abstractions.Documents;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Inventory
{
    /// <summary>
    /// Controlador para consultar el kardex de inventario
    /// Historial completo de movimientos de entrada, salida y ajustes
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class KardexController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IKardexDocumentService _kardexDocumentService;

        public KardexController(IMediator mediator, IKardexDocumentService kardexDocumentService)
        {
            _mediator = mediator;
            _kardexDocumentService = kardexDocumentService;
        }

        /// <summary>
        /// Obtener movimientos del kardex con filtros y paginación
        /// </summary>
        [HttpGet]
        [RequirePermission("Inventory", "ViewKardex")]
        public async Task<IActionResult> GetKardex(
            [FromQuery] int? productId = null,
            [FromQuery] string? productSearch = null,
            [FromQuery] int? warehouseId = null,
            [FromQuery] string? movementType = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                Console.WriteLine($"?? Consultando kardex - Producto: {productId}, Almacén: {warehouseId}, Tipo: {movementType}");

                var request = new GetKardexRequestDto
                {
                    ProductId = productId,
                    ProductSearch = productSearch,
                    WarehouseId = warehouseId,
                    MovementType = movementType,
                    FromDate = fromDate,
                    ToDate = toDate,
                    Page = page,
                    PageSize = pageSize
                };

                var query = new GetKardexQuery(request);
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error consultando kardex: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al consultar kardex",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener estadísticas del kardex
        /// </summary>
        [HttpGet("statistics")]
        [RequirePermission("Inventory", "ViewKardex")]
        public async Task<IActionResult> GetStatistics(
            [FromQuery] int? productId = null,
            [FromQuery] int? warehouseId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var query = new GetKardexStatisticsQuery(productId, warehouseId, fromDate, toDate);
                var result = await _mediator.Send(query);

                return Ok(new
                {
                    message = "Estadísticas del kardex obtenidas exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error obteniendo estadísticas: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener estadísticas",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Exportar kardex a Excel
        /// </summary>
        [HttpGet("export/excel")]
        [RequirePermission("Inventory", "ViewKardex")]
        public async Task<IActionResult> ExportExcel(
            [FromQuery] int? productId = null,
            [FromQuery] int? warehouseId = null,
            [FromQuery] string? movementType = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                Console.WriteLine($"?? Exportando kardex a Excel - Producto: {productId}, Almacén: {warehouseId}");

                var excelBytes = await _kardexDocumentService.GenerateKardexExcelAsync(
                    productId,
                    warehouseId,
                    movementType,
                    fromDate,
                    toDate
                );

                var fileName = $"kardex_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error exportando kardex a Excel: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al exportar kardex a Excel",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Exportar kardex a PDF
        /// </summary>
        [HttpGet("export/pdf")]
        [RequirePermission("Inventory", "ViewKardex")]
        public async Task<IActionResult> ExportPdf(
            [FromQuery] int? productId = null,
            [FromQuery] int? warehouseId = null,
            [FromQuery] string? movementType = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                Console.WriteLine($"?? Exportando kardex a PDF - Producto: {productId}, Almacén: {warehouseId}");

                var pdfBytes = await _kardexDocumentService.GenerateKardexPdfAsync(
                    productId,
                    warehouseId,
                    movementType,
                    fromDate,
                    toDate
                );

                var fileName = $"kardex_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error exportando kardex a PDF: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al exportar kardex a PDF",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener movimientos de un producto específico
        /// </summary>
        [HttpGet("product/{productId}")]
        [RequirePermission("Inventory", "ViewKardex")]
        public async Task<IActionResult> GetProductKardex(
            int productId,
            [FromQuery] int? warehouseId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var request = new GetKardexRequestDto
                {
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    Page = page,
                    PageSize = pageSize
                };

                var query = new GetKardexQuery(request);
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error consultando kardex del producto {productId}: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al consultar kardex del producto",
                    error = 2,
                    details = ex.Message
                });
            }
        }
    }
}
