namespace Application.DTOs.AccountsReceivable;

/// <summary>
/// DTO para mostrar facturas PPD pendientes de pago
/// </summary>
public class InvoicePPDDto
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerRFC { get; set; } = string.Empty;
    public string FolioUUID { get; set; } = string.Empty;
    public string? Serie { get; set; }
    public string? Folio { get; set; }
    public string SerieAndFolio { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Currency { get; set; } = "MXN";
    public decimal ExchangeRate { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public int NextPartialityNumber { get; set; }
    public int TotalPartialities { get; set; }
    public string Status { get; set; } = string.Empty;
    public int DaysOverdue { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO para crear una factura PPD desde una venta/factura existente
/// </summary>
public class CreateInvoicePPDRequest
{
    public int InvoiceId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerRFC { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public int BranchId { get; set; }
    public string FolioUUID { get; set; } = string.Empty;
    public string? Serie { get; set; }
    public string? Folio { get; set; }
    public DateTime InvoiceDate { get; set; }
    public int CreditDays { get; set; } = 30;
    public string Currency { get; set; } = "MXN";
    public decimal ExchangeRate { get; set; } = 1.0M;
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO para listar facturas PPD con paginación
/// </summary>
public class InvoicePPDPageRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? CustomerId { get; set; }
    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }
    public string? Status { get; set; } // Pending, PartiallyPaid, Paid, Overdue
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? MinDaysOverdue { get; set; }
    public decimal? MinAmount { get; set; }
    public string? SearchTerm { get; set; } // Busca por folio, cliente, UUID
}

/// <summary>
/// Respuesta paginada de facturas PPD
/// </summary>
public class InvoicePPDPageResponse
{
    public List<InvoicePPDDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public decimal TotalAmount { get; set; }
    public decimal TotalBalance { get; set; }
}
