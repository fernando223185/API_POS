namespace Application.DTOs.Inventory
{
    /// <summary>
    /// DTO para consultar el kardex de inventario
    /// </summary>
    public class GetKardexRequestDto
    {
        /// <summary>
        /// ID del producto (opcional)
        /// </summary>
        public int? ProductId { get; set; }

        /// <summary>
        /// Búsqueda por código, nombre o referencia del producto
        /// </summary>
        public string? ProductSearch { get; set; }

        /// <summary>
        /// ID del almacén (opcional)
        /// </summary>
        public int? WarehouseId { get; set; }

        /// <summary>
        /// Tipo de movimiento: IN (entrada), OUT (salida), ADJUSTMENT (ajuste)
        /// </summary>
        public string? MovementType { get; set; }

        /// <summary>
        /// Fecha inicial
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Fecha final
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Página actual
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Tamaño de página
        /// </summary>
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// DTO de respuesta del kardex
    /// </summary>
    public class KardexResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public List<KardexMovementDto> Data { get; set; } = new();
        
        /// <summary>
        /// Estadísticas del kardex
        /// </summary>
        public KardexStatisticsDto? Statistics { get; set; }

        /// <summary>
        /// Información de paginación
        /// </summary>
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    /// <summary>
    /// DTO de un movimiento de kardex
    /// </summary>
    public class KardexMovementDto
    {
        public int Id { get; set; }
        public string MovementCode { get; set; } = string.Empty;
        public DateTime MovementDate { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public string MovementTypeName { get; set; } = string.Empty;

        // Producto
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        // Almacén
        public int? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public string? WarehouseCode { get; set; }

        // Cantidades
        public decimal Quantity { get; set; }
        public decimal StockBefore { get; set; }
        public decimal StockAfter { get; set; }

        // Costos
        public decimal? UnitCost { get; set; }
        public decimal? TotalCost { get; set; }

        // Referencias
        public int? PurchaseOrderReceivingId { get; set; }
        public string? PurchaseOrderReceivingCode { get; set; }
        public int? SaleId { get; set; }
        public string? SaleCode { get; set; }

        // Metadatos
        public string? Notes { get; set; }
        public string? CreatedByUserName { get; set; }
    }

    /// <summary>
    /// Estadísticas del kardex
    /// </summary>
    public class KardexStatisticsDto
    {
        /// <summary>
        /// Total de movimientos en el período
        /// </summary>
        public int TotalMovements { get; set; }

        /// <summary>
        /// Entradas hoy
        /// </summary>
        public int EntriesToday { get; set; }

        /// <summary>
        /// Salidas hoy
        /// </summary>
        public int ExitsToday { get; set; }

        /// <summary>
        /// Valor total movido (en el período)
        /// </summary>
        public decimal TotalValue { get; set; }

        /// <summary>
        /// Total de entradas (cantidad)
        /// </summary>
        public decimal TotalEntriesQuantity { get; set; }

        /// <summary>
        /// Total de salidas (cantidad)
        /// </summary>
        public decimal TotalExitsQuantity { get; set; }

        /// <summary>
        /// Valor de entradas
        /// </summary>
        public decimal TotalEntriesValue { get; set; }

        /// <summary>
        /// Valor de salidas
        /// </summary>
        public decimal TotalExitsValue { get; set; }
    }

    /// <summary>
    /// DTO para exportar kardex a Excel
    /// </summary>
    public class ExportKardexRequestDto
    {
        public int? ProductId { get; set; }
        public string? ProductSearch { get; set; }
        public int? WarehouseId { get; set; }
        public string? MovementType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Format { get; set; } = "Excel"; // Excel o PDF
    }
}
