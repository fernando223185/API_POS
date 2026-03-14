namespace Application.DTOs.Billing
{
    /// <summary>
    /// Respuesta paginada de ventas pendientes de timbrar
    /// </summary>
    public class PendingInvoiceSalesResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public List<PendingInvoiceSaleDto> Data { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
        public PendingInvoiceSummaryDto? Summary { get; set; }
    }

    /// <summary>
    /// DTO de venta pendiente de timbrar
    /// </summary>
    public class PendingInvoiceSaleDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        
        // Cliente
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerRfc { get; set; }
        public string? CustomerEmail { get; set; }
        
        // Ubicación
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        
        // Montos
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        
        // Facturación
        public bool RequiresInvoice { get; set; }
        public bool IsPaid { get; set; }
        public string Status { get; set; } = string.Empty;
        
        // Metadatos
        public DateTime CreatedAt { get; set; }
        public int DaysPending { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Resumen de ventas pendientes de timbrar
    /// </summary>
    public class PendingInvoiceSummaryDto
    {
        public int TotalSales { get; set; }
        public int SalesRequiresInvoice { get; set; }
        public int SalesNotRequiresInvoice { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public int AverageDaysPending { get; set; }
    }

    /// <summary>
    /// DTO completo de venta para facturación
    /// Incluye toda la información necesaria para generar CFDI
    /// </summary>
    public class SaleForInvoicingDto
    {
        // Información de la venta
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        
        // Empresa emisora
        public CompanyForInvoicingDto Company { get; set; } = new();
        
        // Sucursal
        public BranchForInvoicingDto? Branch { get; set; }
        
        // Cliente receptor
        public CustomerForInvoicingDto Customer { get; set; } = new();
        
        // Montos
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        
        // Estado
        public bool IsPaid { get; set; }
        public bool RequiresInvoice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? InvoiceUuid { get; set; }
        
        // Detalles de productos
        public List<SaleDetailForInvoicingDto> Details { get; set; } = new();
        
        // Formas de pago
        public List<PaymentMethodForInvoicingDto> Payments { get; set; } = new();
        
        // Metadatos
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Información de la empresa emisora
    /// </summary>
    public class CompanyForInvoicingDto
    {
        public int Id { get; set; }
        public string LegalName { get; set; } = string.Empty;
        public string Rfc { get; set; } = string.Empty;
        public string? FiscalRegime { get; set; }
        public string? PostalCode { get; set; }
        public string? Email { get; set; }
        
        // Certificados SAT
        public string? Serie { get; set; }
        public int? NextFolio { get; set; }
    }

    /// <summary>
    /// Información de la sucursal
    /// </summary>
    public class BranchForInvoicingDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
    }

    /// <summary>
    /// Información del cliente receptor
    /// </summary>
    public class CustomerForInvoicingDto
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Rfc { get; set; }
        public string? FiscalRegime { get; set; }
        public string? PostalCode { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? CfdiUse { get; set; }
    }

    /// <summary>
    /// Detalle de producto para facturación
    /// </summary>
    public class SaleDetailForInvoicingDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        
        // Cantidad y precios
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        
        // Claves SAT
        public string? SatProductKey { get; set; }
        public string? SatUnitKey { get; set; }
        
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Forma de pago para facturación
    /// </summary>
    public class PaymentMethodForInvoicingDto
    {
        public int Id { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        
        // Información adicional según método de pago
        public string? CardNumber { get; set; }
        public string? TransactionReference { get; set; }
        public string? BankName { get; set; }
    }
}
