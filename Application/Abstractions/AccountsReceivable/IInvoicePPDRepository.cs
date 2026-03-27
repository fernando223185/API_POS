using Domain.Entities;

namespace Application.Abstractions.AccountsReceivable;

/// <summary>
/// Interfaz del repositorio de Facturas PPD
/// </summary>
public interface IInvoicePPDRepository
{
    Task<InvoicePPD> CreateAsync(InvoicePPD invoice);
    Task<InvoicePPD?> GetByIdAsync(int id);
    Task<InvoicePPD?> GetByUUIDAsync(string folioUUID);
    Task<InvoicePPD?> GetByInvoiceIdAsync(int invoiceId);
    Task<List<InvoicePPD>> GetPendingByCustomerAsync(int customerId);
    Task<List<InvoicePPD>> GetOverdueAsync(int companyId, int? minDaysOverdue = null);
    Task<(List<InvoicePPD> items, int totalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize,
        int? customerId = null,
        int? companyId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? minDaysOverdue = null,
        decimal? minAmount = null,
        string? searchTerm = null);
    
    Task<InvoicePPD> UpdateAsync(InvoicePPD invoice);
    Task<bool> UpdateStatusAsync(int id, string status);
    Task<decimal> GetTotalPendingByCustomerAsync(int customerId);
    Task<decimal> GetTotalOverdueByCustomerAsync(int customerId);
    Task<Dictionary<string, decimal>> GetAgingReportAsync(int companyId);
    Task<(decimal pendingAmount, decimal overdueAmount)> GetCustomerBalanceSummaryAsync(int customerId, int companyId);
}
