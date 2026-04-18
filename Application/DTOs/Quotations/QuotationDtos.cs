using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Quotations
{
    // ========================================
    // REQUEST DTOs
    // ========================================

    public class CreateQuotationRequestDto
    {
        public int? CustomerId { get; set; }

        [Required(ErrorMessage = "El almacén es requerido")]
        public int WarehouseId { get; set; }

        public int? PriceListId { get; set; }

        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100")]
        public decimal DiscountPercentage { get; set; }

        public bool RequiresInvoice { get; set; }

        /// <summary>
        /// Fecha de vencimiento de la cotización (opcional)
        /// </summary>
        public DateTime? ValidUntil { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Debe agregar al menos un producto")]
        [MinLength(1, ErrorMessage = "Debe agregar al menos un producto")]
        public List<CreateQuotationDetailDto> Details { get; set; } = new();
    }

    public class CreateQuotationDetailDto
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
    }

    /// <summary>
    /// DTO para convertir la cotización en venta al escanear el QR.
    /// SaleType indica si la venta resultante es POS o Delivery.
    /// </summary>
    public class ConvertQuotationToSaleDto
    {
        /// <summary>POS | Delivery</summary>
        [MaxLength(20)]
        public string SaleType { get; set; } = "POS";

        [MaxLength(500)]
        public string? DeliveryAddress { get; set; }

        public DateTime? ScheduledDeliveryDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    // ========================================
    // RESPONSE DTOs
    // ========================================

    public class QuotationResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime QuotationDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        public bool IsExpired => ValidUntil.HasValue && ValidUntil.Value < DateTime.UtcNow && Status != "Converted";

        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }

        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

        public int? PriceListId { get; set; }
        public string? PriceListName { get; set; }

        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }

        public string Status { get; set; } = string.Empty;
        public bool RequiresInvoice { get; set; }

        /// <summary>
        /// ID de la Sale generada al escanear el QR
        /// </summary>
        public int? SaleId { get; set; }
        public string? SaleCode { get; set; }
        public DateTime? ConvertedAt { get; set; }

        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }

        public List<QuotationDetailResponseDto> Details { get; set; } = new();
        public int TotalItems { get; set; }
        public decimal TotalQuantity { get; set; }
    }

    public class QuotationDetailResponseDto
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
        public string? Notes { get; set; }
    }

    public class QuotationSummaryDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime QuotationDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        public string? CustomerName { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string? BranchName { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool RequiresInvoice { get; set; }
        public int TotalItems { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int? SaleId { get; set; }
        public string? SaleCode { get; set; }
    }

    public class QuotationsPagedResponseDto
    {
        public List<QuotationSummaryDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class ConvertQuotationResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public ConvertQuotationDataDto? Data { get; set; }
    }

    public class ConvertQuotationDataDto
    {
        public QuotationResponseDto Quotation { get; set; } = null!;
        public int SaleId { get; set; }
        public string SaleCode { get; set; } = string.Empty;
    }
}
