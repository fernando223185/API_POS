namespace Application.DTOs.Inventory
{
    /// <summary>
    /// DTO para crear un nuevo ajuste de inventario
    /// </summary>
    public class CreateStockAdjustmentDto
    {
        public int WarehouseId { get; set; }
        public DateTime AdjustmentDate { get; set; }
        
        /// <summary>
        /// Razón del ajuste: PHYSICAL_COUNT, DAMAGE, LOSS, EXPIRATION, ERROR, SAMPLE, PRODUCTION_WASTE, OTHER
        /// </summary>
        public string Reason { get; set; } = string.Empty;
        
        public string? Notes { get; set; }
        public List<StockAdjustmentDetailDto> Details { get; set; } = new();
    }

    /// <summary>
    /// DTO para el detalle de un producto a ajustar
    /// </summary>
    public class StockAdjustmentDetailDto
    {
        public int ProductId { get; set; }
        
        /// <summary>
        /// Cantidad que mostraba el sistema antes del ajuste
        /// </summary>
        public decimal SystemQuantity { get; set; }
        
        /// <summary>
        /// Cantidad física real (conteo, corrección)
        /// </summary>
        public decimal PhysicalQuantity { get; set; }
        
        /// <summary>
        /// Notas específicas del producto (opcional)
        /// </summary>
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO de respuesta con el ajuste creado
    /// </summary>
    public class StockAdjustmentResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public DateTime AdjustmentDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string ReasonLabel { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        
        public int TotalProducts { get; set; }
        public decimal TotalAdjustmentCost { get; set; }
        
        public List<StockAdjustmentDetailResponseDto> Details { get; set; } = new();
    }

    /// <summary>
    /// DTO de respuesta del detalle
    /// </summary>
    public class StockAdjustmentDetailResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal SystemQuantity { get; set; }
        public decimal PhysicalQuantity { get; set; }
        public decimal AdjustmentQuantity { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? TotalCost { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO para listado paginado de ajustes
    /// </summary>
    public class PagedStockAdjustmentsResponseDto
    {
        public List<StockAdjustmentSummaryDto> Items { get; set; } = new();
        public int TotalRecords { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// DTO resumido para listado
    /// </summary>
    public class StockAdjustmentSummaryDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public DateTime AdjustmentDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string ReasonLabel { get; set; } = string.Empty;
        public string CreatedByUserName { get; set; } = string.Empty;
        public int TotalProducts { get; set; }
        public decimal TotalAdjustmentCost { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Constantes para las razones de ajuste
    /// </summary>
    public static class AdjustmentReason
    {
        public const string PHYSICAL_COUNT = "PHYSICAL_COUNT";
        public const string DAMAGE = "DAMAGE";
        public const string LOSS = "LOSS";
        public const string EXPIRATION = "EXPIRATION";
        public const string ERROR = "ERROR";
        public const string SAMPLE = "SAMPLE";
        public const string PRODUCTION_WASTE = "PRODUCTION_WASTE";
        public const string OTHER = "OTHER";

        public static Dictionary<string, string> Labels = new()
        {
            [PHYSICAL_COUNT] = "Inventario Físico",
            [DAMAGE] = "Daño / Deterioro",
            [LOSS] = "Pérdida / Robo",
            [EXPIRATION] = "Producto Vencido",
            [ERROR] = "Corrección de Error",
            [SAMPLE] = "Muestras / Degustaciones",
            [PRODUCTION_WASTE] = "Merma de Producción",
            [OTHER] = "Otros"
        };

        public static string GetLabel(string reason)
        {
            return Labels.TryGetValue(reason, out var label) ? label : reason;
        }
    }
}
