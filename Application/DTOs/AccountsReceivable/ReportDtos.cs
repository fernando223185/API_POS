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
