using Domain.Entities;

namespace Application.Abstractions.AccountsReceivable;

/// <summary>
/// Interfaz del repositorio de Pagos
/// </summary>
public interface IPaymentRepository
{
    Task<Payment> CreateAsync(Payment payment);
    Task<Payment?> GetByIdAsync(int id);
    Task<Payment?> GetByPaymentNumberAsync(string paymentNumber);
    Task<List<Payment>> GetByCustomerAsync(int customerId, int? limit = null);
    Task<List<Payment>> GetByBatchIdAsync(int batchId);
    Task<Payment> UpdateAsync(Payment payment);
    Task<string> GeneratePaymentNumberAsync(string prefix = "PAG");
    
    /// <summary>
    /// Obtener pagos paginados con filtros
    /// </summary>
    Task<(List<Payment> items, int totalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        int? customerId = null,
        int? companyId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        bool? hasComplement = null,
        string? searchTerm = null);
}
