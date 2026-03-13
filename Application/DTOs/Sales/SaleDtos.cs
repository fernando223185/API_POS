using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Sales
{
    // ========================================
    // REQUEST DTOs
    // ========================================

    /// <summary>
    /// DTO para crear una nueva venta
    /// </summary>
    public class CreateSaleRequestDto
    {
        public int? CustomerId { get; set; }

        [Required(ErrorMessage = "El almacén es requerido")]
        public int WarehouseId { get; set; }

        public int? PriceListId { get; set; }

        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100")]
        public decimal DiscountPercentage { get; set; }

        public bool RequiresInvoice { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Debe agregar al menos un producto")]
        [MinLength(1, ErrorMessage = "Debe agregar al menos un producto")]
        public List<CreateSaleDetailDto> Details { get; set; } = new();
    }

    /// <summary>
    /// DTO para agregar un producto a la venta
    /// </summary>
    public class CreateSaleDetailDto
    {
        [Required(ErrorMessage = "El producto es requerido")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal Quantity { get; set; }

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a 0")]
        public decimal UnitPrice { get; set; }

        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100")]
        public decimal DiscountPercentage { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        [MaxLength(100)]
        public string? LotNumber { get; set; }
    }

    /// <summary>
    /// DTO para procesar los pagos de una venta
    /// </summary>
    public class ProcessSalePaymentsRequestDto
    {
        [Required(ErrorMessage = "Debe agregar al menos una forma de pago")]
        [MinLength(1, ErrorMessage = "Debe agregar al menos una forma de pago")]
        public List<CreateSalePaymentDto> Payments { get; set; } = new();
    }

    /// <summary>
    /// DTO para una forma de pago
    /// </summary>
    public class CreateSalePaymentDto
    {
        [Required(ErrorMessage = "El método de pago es requerido")]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }

        // ===== DATOS DE TARJETA =====
        [MaxLength(4)]
        public string? CardNumber { get; set; }

        [MaxLength(20)]
        public string? CardType { get; set; }

        [MaxLength(50)]
        public string? AuthorizationCode { get; set; }

        [MaxLength(100)]
        public string? TransactionReference { get; set; }

        [MaxLength(50)]
        public string? TerminalId { get; set; }

        [MaxLength(100)]
        public string? BankName { get; set; }

        // ===== TRANSFERENCIAS =====
        [MaxLength(100)]
        public string? TransferReference { get; set; }

        // ===== CHEQUES =====
        [MaxLength(50)]
        public string? CheckNumber { get; set; }

        [MaxLength(100)]
        public string? CheckBank { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO para cancelar una venta
    /// </summary>
    public class CancelSaleRequestDto
    {
        [Required(ErrorMessage = "La razón de cancelación es requerida")]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    // ========================================
    // RESPONSE DTOs
    // ========================================

    /// <summary>
    /// DTO de respuesta de venta completa
    /// </summary>
    public class SaleResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }

        // Cliente
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }

        // Ubicación
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string? BranchName { get; set; }

        // Usuario
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

        // Lista de precios
        public int? PriceListId { get; set; }
        public string? PriceListName { get; set; }

        // Montos
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }

        // Pago
        public decimal AmountPaid { get; set; }
        public decimal ChangeAmount { get; set; }
        public bool IsPaid { get; set; }

        // Estado
        public string Status { get; set; } = string.Empty;
        public bool IsPostedToInventory { get; set; }
        public DateTime? PostedToInventoryDate { get; set; }

        // Facturación
        public bool RequiresInvoice { get; set; }
        public string? InvoiceUuid { get; set; }

        // Metadatos
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }

        // Relaciones
        public List<SaleDetailResponseDto> Details { get; set; } = new();
        public List<SalePaymentResponseDto> Payments { get; set; } = new();

        // Totales calculados
        public int TotalItems { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal? TotalCost { get; set; }
        public decimal? GrossProfit { get; set; }
        public decimal? ProfitMarginPercentage { get; set; }
    }

    /// <summary>
    /// DTO de detalle de venta
    /// </summary>
    public class SaleDetailResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal TaxAmount { get; set; }

        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }

        public decimal? UnitCost { get; set; }
        public decimal? TotalCost { get; set; }
        public decimal? LineProfit { get; set; }

        public string? Notes { get; set; }
        public string? SerialNumber { get; set; }
        public string? LotNumber { get; set; }
    }

    /// <summary>
    /// DTO de pago de venta
    /// </summary>
    public class SalePaymentResponseDto
    {
        public int Id { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }

        // Tarjeta
        public string? CardNumber { get; set; }
        public string? CardType { get; set; }
        public string? AuthorizationCode { get; set; }
        public string? TransactionReference { get; set; }
        public string? TerminalId { get; set; }
        public string? BankName { get; set; }

        // Transferencia/Cheque
        public string? TransferReference { get; set; }
        public string? CheckNumber { get; set; }
        public string? CheckBank { get; set; }

        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO resumido para listados
    /// </summary>
    public class SaleSummaryDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public string? CustomerName { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsPaid { get; set; }
        public bool RequiresInvoice { get; set; }
        public int TotalItems { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Respuesta de procesamiento de pagos
    /// </summary>
    public class ProcessSalePaymentsResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public ProcessSalePaymentsDataDto? Data { get; set; }
    }

    public class ProcessSalePaymentsDataDto
    {
        public int SaleId { get; set; }
        public string SaleCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public decimal ChangeAmount { get; set; }
        public int TotalPayments { get; set; }
        public int TotalMovements { get; set; }
        public List<SalePaymentResponseDto> Payments { get; set; } = new();
        public List<InventoryMovementSummaryDto> InventoryMovements { get; set; } = new();
    }

    /// <summary>
    /// Resumen de movimiento de inventario
    /// </summary>
    public class InventoryMovementSummaryDto
    {
        public string MovementCode { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? WarehouseCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal StockBefore { get; set; }
        public decimal StockAfter { get; set; }
    }

    /// <summary>
    /// Respuesta paginada de ventas
    /// </summary>
    public class SalesPagedResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public List<SaleSummaryDto> Data { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
        public SalesStatisticsDto? Statistics { get; set; }
    }

    /// <summary>
    /// Estadísticas de ventas
    /// </summary>
    public class SalesStatisticsDto
    {
        public int TotalSales { get; set; }
        public int CompletedSales { get; set; }
        public int CancelledSales { get; set; }
        public int DraftSales { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal AverageSaleAmount { get; set; }
        public decimal AverageProfitMargin { get; set; }
    }
}
