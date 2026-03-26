using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Commands;

/// <summary>
/// Command para crear o actualizar la política de crédito de un cliente
/// </summary>
public class UpsertCustomerCreditPolicyCommand : IRequest<CustomerCreditPolicyDto>
{
    public int CustomerId { get; set; }
    public int CompanyId { get; set; }
    public decimal CreditLimit { get; set; }
    public int CreditDays { get; set; } = 30;
    public int OverdueGraceDays { get; set; } = 0;
    public bool AutoBlockOnOverdue { get; set; } = true;
    public string? Notes { get; set; }
    public int ExecutedBy { get; set; }
}
