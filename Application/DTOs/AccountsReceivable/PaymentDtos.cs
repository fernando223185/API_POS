namespace Application.DTOs.AccountsReceivable;

/// <summary>
/// DTO para registrar un pago
/// </summary>
public class CreatePaymentRequest
{
    public int CustomerId { get; set; }
    public int CompanyId { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentFormSAT { get; set; } = string.Empty; // 01=Efectivo, 02=Cheque, 03=Transferencia, 04=Tarjeta,  28=Tarjeta débito, 99=Por definir
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
    public string PaymentFormSAT { get; set; } = string.Empty;
    public string? Reference { get; set; }
    
    // Datos del emisor (snapshot al timbrar)
    public string? EmisorRfc { get; set; }
    public string? EmisorNombre { get; set; }
    public string? EmisorRegimenFiscal { get; set; }
    public string? LugarExpedicion { get; set; }
    
    // Datos del receptor (snapshot al timbrar)
    public string? ReceptorRfc { get; set; }
    public string? ReceptorNombre { get; set; }
    public string? ReceptorDomicilioFiscal { get; set; }
    public string? ReceptorRegimenFiscal { get; set; }
    public string? ReceptorUsoCfdi { get; set; }
    
    public string Status { get; set; } = string.Empty;
    public int AppliedToInvoices { get; set; }
    public int ComplementsGenerated { get; set; }
    public int ComplementsWithError { get; set; }
    
    // Datos de timbrado del complemento de pago
    public string? Uuid { get; set; }
    public DateTime? TimbradoAt { get; set; }
    public string? XmlCfdi { get; set; }
    public string? CadenaOriginalSat { get; set; }
    public string? SelloCfdi { get; set; }
    public string? SelloSat { get; set; }
    public string? NoCertificadoCfdi { get; set; }
    public string? NoCertificadoSat { get; set; }
    public string? QrCode { get; set; }
    
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
    // Datos de impuestos del documento relacionado
    public string DocumentCurrency { get; set; } = "MXN";
    public decimal DocumentExchangeRate { get; set; } = 1.0M;
    public string TaxObject { get; set; } = "02";
    public decimal TaxBase { get; set; }
    public string TaxCode { get; set; } = "002";
    public string TaxFactorType { get; set; } = "Tasa";
    public decimal TaxRate { get; set; } = 0.160000M;
    public decimal TaxAmount { get; set; }
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
    /// <summary>Folio personalizado del lote. Si no se envía, se genera automáticamente (BTCP-COMP001-260326-001)</summary>
    public string? CustomBatchNumber { get; set; }
    /// <summary>Forma de pago común para todos los pagos del lote (01=Efectivo, 02=Cheque, 03=Transferencia). Se puede sobreescribir por pago.</summary>
    public string? PaymentFormSAT { get; set; } = "03";
    /// <summary>Banco destino común (tu banco donde reciben los pagos). Se puede sobreescribir por pago.</summary>
    public string? BankDestination { get; set; }
    /// <summary>Cuenta destino común (tu cuenta donde reciben los pagos). Se puede sobreescribir por pago.</summary>
    public string? AccountDestination { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public List<BatchPaymentItem> Payments { get; set; } = new();
}

/// <summary>
/// Item de pago dentro de un batch: un pago por cliente con sus facturas.
/// Todos los pagos del lote comparten: forma de pago, banco destino, cuenta destino (nivel batch).
/// </summary>
public class BatchPaymentItem
{
    public int CustomerId { get; set; }
    /// <summary>Referencia del pago (número de transferencia, SPEI, cheque, etc.)</summary>
    public string? Reference { get; set; }
    /// <summary>Fecha del pago. Si es null se usa la fecha del lote</summary>
    public DateTime? PaymentDate { get; set; }
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

