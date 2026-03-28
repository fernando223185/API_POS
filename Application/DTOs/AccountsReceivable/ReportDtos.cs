namespace Application.DTOs.AccountsReceivable;

/// <summary>
/// Dashboard principal de Cuentas por Cobrar
/// </summary>
public class AccountsReceivableDashboardDto
{
    public decimal TotalReceivable { get; set; }
    public decimal TotalOverdue { get; set; }
    public decimal TotalDueToday { get; set; }
    public decimal TotalNotDue { get; set; }
    public int TotalInvoicesPending { get; set; }
    public int CustomersWithBalance { get; set; }
    public int AverageCollectionDays { get; set; }
    public decimal CollectedThisMonth { get; set; }
    public decimal OverduePercentage { get; set; }
    public AgingReportDto AgingReport { get; set; } = new();
    public List<TopCustomerBalanceDto> TopCustomers { get; set; } = new();
}

/// <summary>
/// Reporte de antigüedad de saldos
/// </summary>
public class AgingReportDto
{
    public decimal Current { get; set; } // 0-30 días
    public decimal Days31To60 { get; set; }
    public decimal Days61To90 { get; set; }
    public decimal Over90Days { get; set; }
    public int CurrentCount { get; set; }
    public int Days31To60Count { get; set; }
    public int Days61To90Count { get; set; }
    public int Over90DaysCount { get; set; }
}

/// <summary>
/// Top clientes con mayor saldo
/// </summary>
public class TopCustomerBalanceDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalBalance { get; set; }
    public decimal OverdueAmount { get; set; }
    public int InvoiceCount { get; set; }
    public string CreditStatus { get; set; } = string.Empty;
}

/// <summary>
/// DTO para factura PPD (pago en parcialidades)
/// </summary>
public class InvoicePPDDto
{
    public int Id { get; set; }
    public int? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerRFC { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string Folio { get; set; } = string.Empty;
    public string Uuid { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string Moneda { get; set; } = "MXN";
    public decimal? TipoCambio { get; set; }
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal? BalanceAmount { get; set; }
    public int? NextPartialityNumber { get; set; }
    public int TotalPartialities { get; set; }
    public string? PaymentStatus { get; set; }
    public int? DaysOverdue { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Estado de cuenta del cliente
/// </summary>
public class CustomerStatementDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerRFC { get; set; } = string.Empty;
    public CustomerCreditPolicyDto? CreditPolicy { get; set; }
    public decimal TotalPending { get; set; }
    public decimal TotalOverdue { get; set; }
    public int InvoicesPending { get; set; }
    public List<InvoicePPDDto> Invoices { get; set; } = new();
    public List<PaymentDto> RecentPayments { get; set; } = new();
    public List<CustomerCreditHistoryDto> RecentHistory { get; set; } = new();
}

/// <summary>
/// Proyección de cobranza
/// </summary>
public class CollectionForecastDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<CollectionForecastPeriod> Periods { get; set; } = new();
    public decimal TotalProjected { get; set; }
}

/// <summary>
/// Periodo de proyección
/// </summary>
public class CollectionForecastPeriod
{
    public string PeriodName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Amount { get; set; }
    public int InvoiceCount { get; set; }
}

/// <summary>
/// Métricas de CxC
/// </summary>
public class AccountsReceivableMetricsDto
{
    public int DSO { get; set; } // Days Sales Outstanding
    public decimal DelinquencyRate { get; set; } // Tasa de morosidad
    public decimal CollectionEffectiveness { get; set; } // Efectividad de cobranza
    public int AveragePortfolioAge { get; set; } // Edad promedio de cartera
    public decimal RecoveryRate { get; set; } // Tasa de recuperación
    public DateTime CalculatedAt { get; set; }
}

/// <summary>
/// Reporte de facturas vencidas
/// </summary>
public class OverdueInvoicesReportDto
{
    public decimal TotalOverdue { get; set; }
    public int TotalInvoices { get; set; }
    public List<OverdueCustomerSummary> CustomerSummaries { get; set; } = new();
}

/// <summary>
/// Resumen de vencidos por cliente
/// </summary>
public class OverdueCustomerSummary
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int OverdueInvoicesCount { get; set; }
    public decimal TotalOverdue { get; set; }
    public DateTime? OldestDueDate { get; set; }
    public int MaxDaysOverdue { get; set; }
    public string CreditStatus { get; set; } = string.Empty;
}

/// <summary>
/// Response paginado de facturas PPD
/// </summary>
public class InvoicePPDPagedResultDto
{
    public List<InvoicePPDDto> Data { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    
    // Resumen
    public InvoicePPDSummaryDto? Summary { get; set; }
}

/// <summary>
/// Resumen de facturas PPD
/// </summary>
public class InvoicePPDSummaryDto
{
    public int TotalInvoices { get; set; }
    public int PendingInvoices { get; set; }
    public int PartiallyPaidInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public int OverdueInvoices { get; set; }
    
    public decimal TotalAmount { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalBalance { get; set; }
    public decimal TotalOverdueAmount { get; set; }
    
    public int AverageDaysOverdue { get; set; }
}

/// <summary>
/// Detalle completo de una factura PPD incluyendo historial de pagos
/// </summary>
public class InvoicePPDDetailDto
{
    // Información básica de la factura
    public int Id { get; set; }
    public int? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerRFC { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string Folio { get; set; } = string.Empty;
    public string SerieAndFolio => $"{Serie}-{Folio}";
    public string Uuid { get; set; } = string.Empty;
    
    // Fechas
    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    
    // Montos
    public string Moneda { get; set; } = "MXN";
    public decimal? TipoCambio { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ImpuestosTrasladadosTotal { get; set; }
    public decimal? ImpuestosRetenidosTotal { get; set; }
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    
    // Estado de pago
    public int NextPartialityNumber { get; set; }
    public int TotalPartialities { get; set; }
    public string? PaymentStatus { get; set; }
    public int? DaysOverdue { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    
    // Información fiscal
    public string TipoDeComprobante { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = string.Empty;
    public string FormaPago { get; set; } = string.Empty;
    public string UsoCFDI { get; set; } = string.Empty;
    
    // Emisor
    public string EmisorRfc { get; set; } = string.Empty;
    public string EmisorNombre { get; set; } = string.Empty;
    
    // Receptor
    public string ReceptorRfc { get; set; } = string.Empty;
    public string ReceptorNombre { get; set; } = string.Empty;
    
    // Notas y auditoría
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Historial de pagos aplicados
    public List<PaymentApplicationSummaryDto> PaymentsApplied { get; set; } = new();
}

/// <summary>
/// Resumen de pago aplicado a una factura
/// </summary>
public class PaymentApplicationSummaryDto
{
    public int PaymentId { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal AmountApplied { get; set; }
    public int PartialityNumber { get; set; }
    public string PaymentMethodSAT { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? Uuid { get; set; } // UUID del complemento de pago
    public string Status { get; set; } = string.Empty;
}
