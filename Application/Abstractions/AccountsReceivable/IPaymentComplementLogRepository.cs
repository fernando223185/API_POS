using Domain.Entities;

namespace Application.Abstractions.AccountsReceivable;

/// <summary>
/// Interfaz del repositorio de Logs de Complementos
/// </summary>
public interface IPaymentComplementLogRepository
{
    Task<PaymentComplementLog> CreateAsync(PaymentComplementLog log);
    Task<List<PaymentComplementLog>> GetByPaymentApplicationIdAsync(int paymentApplicationId);
    Task<List<PaymentComplementLog>> GetByPaymentIdAsync(int paymentId);
    Task<List<PaymentComplementLog>> GetErrorLogsAsync(int? batchId = null, int? limit = null);
}
