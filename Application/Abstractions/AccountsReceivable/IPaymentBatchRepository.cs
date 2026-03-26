using Domain.Entities;

namespace Application.Abstractions.AccountsReceivable;

/// <summary>
/// Interfaz del repositorio de Lotes de Pago
/// </summary>
public interface IPaymentBatchRepository
{
    Task<PaymentBatch> CreateAsync(PaymentBatch batch);
    Task<PaymentBatch?> GetByIdAsync(int id);
    Task<PaymentBatch?> GetByBatchNumberAsync(string batchNumber);
    Task<PaymentBatch> UpdateAsync(PaymentBatch batch);
    Task<string> GenerateBatchNumberAsync(string prefix = "LOTE");
    Task<List<PaymentBatch>> GetRecentBatchesAsync(int companyId, int limit = 10);
}
