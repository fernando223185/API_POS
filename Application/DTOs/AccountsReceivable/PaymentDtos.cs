namespace Application.DTOs.AccountsReceivable;

/// <summary>
/// DTO para registrar un pago
/// </summary>
public class CreatePaymentRequest
{
    public int CustomerId { get; set; }
    public int CompanyId { get; set; }
    public int BranchId { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethodSAT { get; set; } = string.Empty; // 03, 02, 01, etc.
    public string? PaymentFormSAT { get; set; }
    public string Currency { get; set; } = "MXN";
    public decimal ExchangeRate { get; set; } = 1.0M;
    public string? BankOrigin { get; set; }
    public string? BankAccountOrigin { get; set; }
    public string? BankDestination { get; set; }
    public string? BankAccountDestination { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public List<PaymentInvoiceItem> Invoices { get; set; } = new();
}

/// <summary>
/// Item individual de factura dentro del pago
/// </summary>
public class PaymentInvoiceItem
{
    public int InvoicePPDId { get; set; }
    public decimal AmountToPay { get; set; }
}

/// <summary>
/// DTO de respuesta de un pago
/// </summary>
public class PaymentDto
{
    public int Id { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public int BranchId { get; set; }
    public int? BatchId { get; set; }
    public string? BatchNumber { get; set; }
    public bool IsBatchPayment { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentMethodSAT { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string Status { get; set; } = string.Empty;
    public int AppliedToInvoices { get; set; }
    public int ComplementsGenerated { get; set; }
    public int ComplementsWithError { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AppliedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<PaymentApplicationDto> Applications { get; set; } = new();
}

/// <summary>
/// DTO de aplicación de pago
/// </summary>
public class PaymentApplicationDto
{
    public int Id { get; set; }
    public int PaymentId { get; set; }
    public int InvoicePPDId { get; set; }
    public string FolioUUID { get; set; } = string.Empty;
    public string SerieAndFolio { get; set; } = string.Empty;
    public string PaymentType { get; set; } = string.Empty;
    public int PartialityNumber { get; set; }
    public decimal PreviousBalance { get; set; }
    public decimal AmountApplied { get; set; }
    public decimal NewBalance { get; set; }
    public string? ComplementUUID { get; set; }
    public string? ComplementSerieAndFolio { get; set; }
    public string ComplementStatus { get; set; } = string.Empty;
    public string? ComplementError { get; set; }
    public string? XmlPath { get; set; }
    public string? PdfPath { get; set; }
    public bool EmailSent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? GeneratedAt { get; set; }
}

/// <summary>
/// Request para generar complementos de pago
/// </summary>
public class GenerateComplementsRequest
{
    public int PaymentId { get; set; }
    public bool SendEmailsAutomatically { get; set; } = false;
}

/// <summary>
/// Request para crear un batch de pagos
/// </summary>
public class CreatePaymentBatchRequest
{
    public int CompanyId { get; set; }
    public int BranchId { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? DefaultPaymentMethodSAT { get; set; }
    public string? DefaultBankDestination { get; set; }
    public string? DefaultAccountDestination { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public List<BatchPaymentItem> Payments { get; set; } = new();
}

/// <summary>
/// Item de pago dentro de un batch
/// </summary>
public class BatchPaymentItem
{
    public int CustomerId { get; set; }
    public List<int> InvoiceIds { get; set; } = new();
    public string? Reference { get; set; }
}

/// <summary>
/// DTO del batch de pagos
/// </summary>
public class PaymentBatchDto
{
    public int Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public int TotalPayments { get; set; }
    public int TotalInvoices { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ProcessingProgress { get; set; }
    public int ComplementsGenerated { get; set; }
    public int ComplementsWithError { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<PaymentDto> Payments { get; set; } = new();
}
