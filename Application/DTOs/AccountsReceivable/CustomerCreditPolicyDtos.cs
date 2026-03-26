namespace Application.DTOs.AccountsReceivable;

/// <summary>
/// DTO para mostrar/editar política de crédito del cliente
/// </summary>
public class CustomerCreditPolicyDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public decimal CreditLimit { get; set; }
    public int CreditDays { get; set; }
    public int OverdueGraceDays { get; set; }
    public decimal TotalPendingAmount { get; set; }
    public decimal TotalOverdueAmount { get; set; }
    public decimal AvailableCredit { get; set; }
    public DateTime? OldestInvoiceDate { get; set; }
    public DateTime? OldestOverdueDate { get; set; }
    public int AveragePaymentDays { get; set; }
    public decimal OnTimePaymentRate { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public decimal LastPaymentAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? BlockReason { get; set; }
    public bool AutoBlockOnOverdue { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Request para crear/actualizar política de crédito
/// </summary>
public class UpsertCustomerCreditPolicyRequest
{
    public int CustomerId { get; set; }
    public int CompanyId { get; set; }
    public decimal CreditLimit { get; set; }
    public int CreditDays { get; set; } = 30;
    public int OverdueGraceDays { get; set; } = 0;
    public bool AutoBlockOnOverdue { get; set; } = true;
    public string? Notes { get; set; }
}

/// <summary>
/// Request para bloquear/desbloquear crédito
/// </summary>
public class UpdateCreditStatusRequest
{
    public int CustomerId { get; set; }
    public string Status { get; set; } = string.Empty; // Active, Blocked, Suspended
    public string? Reason { get; set; }
}

/// <summary>
/// DTO para historial de crédito
/// </summary>
public class CustomerCreditHistoryDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public decimal? Amount { get; set; }
    public string? RelatedEntity { get; set; }
    public string? Description { get; set; }
    public string? PreviousValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedAt { get; set; }
}
