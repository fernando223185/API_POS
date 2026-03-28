using Application.Abstractions.AccountsReceivable;
using Application.Abstractions.Billing;
using Application.Core.AccountsReceivable.Queries;
using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.QueryHandlers;

/// <summary>
/// Handler para obtener la política de crédito de un cliente
/// </summary>
public class GetCustomerCreditPolicyQueryHandler : IRequestHandler<GetCustomerCreditPolicyQuery, CustomerCreditPolicyDto?>
{
    private readonly ICustomerCreditPolicyRepository _policyRepository;
    private readonly IInvoiceRepository _invoiceRepository;

    public GetCustomerCreditPolicyQueryHandler(
        ICustomerCreditPolicyRepository policyRepository,
        IInvoiceRepository invoiceRepository)
    {
        _policyRepository = policyRepository;
        _invoiceRepository = invoiceRepository;
    }

    public async Task<CustomerCreditPolicyDto?> Handle(GetCustomerCreditPolicyQuery request, CancellationToken cancellationToken)
    {
        // Obtener política del cliente
        var policy = await _policyRepository.GetByCustomerIdAsync(request.CustomerId);
        
        if (policy == null)
            return null;

        // Calcular métricas actuales
        var (pendingAmount, overdueAmount) = await _invoiceRepository.GetCustomerBalanceSummaryAsync(
            request.CustomerId, 
            policy.CompanyId);

        var availableCredit = policy.CreditLimit - pendingAmount;

        return new CustomerCreditPolicyDto
        {
            Id = policy.Id,
            CustomerId = policy.CustomerId,
            CustomerName = "",
            CompanyId = policy.CompanyId,
            CreditLimit = policy.CreditLimit,
            CreditDays = policy.CreditDays,
            OverdueGraceDays = policy.OverdueGraceDays,
            TotalPendingAmount = pendingAmount,
            TotalOverdueAmount = overdueAmount,
            AvailableCredit = availableCredit,
            Status = policy.Status,
            BlockReason = policy.BlockReason,
            AutoBlockOnOverdue = policy.AutoBlockOnOverdue,
            Notes = policy.Notes,
            CreatedAt = policy.CreatedAt,
            UpdatedAt = policy.UpdatedAt
        };
    }
}
