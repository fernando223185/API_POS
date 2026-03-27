namespace Application.DTOs.AccountsReceivable;

/// <summary>
/// DTO para registrar un pago
/// </summary>
public class CreatePaymentRequest
{
    public int CustomerId { get; set; }
    public int CompanyId { get; set; }
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
/// Resultado de generación de complementos
/// </summary>
public class GenerateComplementsResultDto
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public int? BatchId { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Request para crear un batch de pagos
/// </summary>
public class CreatePaymentBatchRequest
{
    public int CompanyId { get; set; }
    public DateTime PaymentDate { get; set; }
    /// <summary>Folio personalizado del lote. Si no se envía, se genera automáticamente (LOTE-2026-001)</summary>
    public string? CustomBatchNumber { get; set; }
    /// <summary>Prefijo del folio autogenerado. Default: LOTE</summary>
    public string? BatchPrefix { get; set; } = "LOTE";
    public string? DefaultPaymentMethodSAT { get; set; }
    public string? DefaultPaymentFormSAT { get; set; } // 01=Efectivo, 02=Cheque, 03=Transferencia
    public string? DefaultBankDestination { get; set; }
    public string? DefaultAccountDestination { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public List<BatchPaymentItem> Payments { get; set; } = new();
}

/// <summary>
/// Item de pago dentro de un batch: un pago por cliente con sus facturas y montos
/// </summary>
public class BatchPaymentItem
{
    public int CustomerId { get; set; }
    /// <summary>Fecha del pago. Si es null se usa la fecha del lote (útil cuando depósitos llegaron en días distintos)</summary>
    public DateTime? PaymentDate { get; set; }
    /// <summary>Referencia del pago (número de transferencia, cheque, etc.)</summary>
    public string? Reference { get; set; }
    /// <summary>Método de pago SAT específico de este pago (sobreescribe el default del lote)</summary>
    public string? PaymentMethodSAT { get; set; }
    /// <summary>Forma de pago SAT específico de este pago (sobreescribe el default del lote)</summary>
    public string? PaymentFormSAT { get; set; }
    /// <summary>Banco origen del cliente (siempre varía por pago)</summary>
    public string? BankOrigin { get; set; }
    /// <summary>Banco destino específico de este pago (sobreescribe el default del lote)</summary>
    public string? BankDestination { get; set; }
    /// <summary>Cuenta destino específica de este pago (sobreescribe el default del lote)</summary>
    public string? AccountDestination { get; set; }
    /// <summary>Facturas a aplicar con su monto específico</summary>
    public List<BatchInvoiceApplication> Invoices { get; set; } = new();
}

/// <summary>
/// Aplicación de una factura dentro de un item de batch, con monto específico
/// </summary>
public class BatchInvoiceApplication
{
    public int InvoicePPDId { get; set; }
    /// <summary>Monto a pagar. Si es null o 0, se paga el saldo total de la factura</summary>
    public decimal? AmountToPay { get; set; }
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

/// <summary>
/// DTO resumido para lista de batches (sin pagos completos)
/// </summary>
public class PaymentBatchListItemDto
{
    public int Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public int TotalPayments { get; set; }
    public int TotalInvoices { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusDescription { get; set; } = string.Empty;
    public int ProcessingProgress { get; set; }
    public int ComplementsGenerated { get; set; }
    public int ComplementsWithError { get; set; }
    public bool HasErrors => ComplementsWithError > 0;
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int CreatedBy { get; set; }
}

/// <summary>
/// Respuesta paginada de batches
/// </summary>
public class PaymentBatchPagedResultDto
{
    public List<PaymentBatchListItemDto> Data { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

