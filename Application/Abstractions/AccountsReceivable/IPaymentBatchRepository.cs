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
    Task<bool> ExistsByBatchNumberAsync(string batchNumber);
    Task<List<PaymentBatch>> GetRecentBatchesAsync(int companyId, int limit = 10);
    Task<(List<PaymentBatch> items, int totalCount)> GetPagedAsync(
        int companyId,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null,
        bool? hasErrors = null,
        int pageNumber = 1,
        int pageSize = 20,
        string orderBy = "CreatedAt",
        bool ascending = false);
}
