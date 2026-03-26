using Domain.Entities;

namespace Application.Abstractions.AccountsReceivable;

/// <summary>
/// Interfaz del repositorio de Historial de Crédito
/// </summary>
public interface ICustomerCreditHistoryRepository
{
    Task<CustomerCreditHistory> CreateAsync(CustomerCreditHistory history);
    Task<List<CustomerCreditHistory>> GetByCustomerIdAsync(int customerId, int? limit = null);
    Task<List<CustomerCreditHistory>> GetByEventTypeAsync(int customerId, string eventType, int? limit = null);
}
