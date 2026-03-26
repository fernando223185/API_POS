using Domain.Entities;

namespace Application.Abstractions.AccountsReceivable;

/// <summary>
/// Interfaz del repositorio de Políticas de Crédito de Clientes
/// </summary>
public interface ICustomerCreditPolicyRepository
{
    Task<CustomerCreditPolicy> CreateAsync(CustomerCreditPolicy policy);
    Task<CustomerCreditPolicy?> GetByCustomerIdAsync(int customerId);
    Task<CustomerCreditPolicy> UpdateAsync(CustomerCreditPolicy policy);
    Task<bool> UpdateBalancesAsync(int customerId, decimal pendingAmount, decimal overdueAmount);
    Task<bool> ValidateCreditAvailabilityAsync(int customerId, decimal amount);
    Task<List<CustomerCreditPolicy>> GetBlockedCustomersAsync(int companyId);
    Task<List<CustomerCreditPolicy>> GetCustomersNearLimitAsync(int companyId, decimal percentage = 0.9M);
}
