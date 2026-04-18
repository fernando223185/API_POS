using Domain.Entities;

namespace Application.Abstractions.Quotations
{
    public interface IQuotationRepository
    {
        Task<Quotation> CreateAsync(Quotation quotation);
        Task<Quotation?> GetByIdAsync(int id);
        Task<Quotation?> GetByCodeAsync(string code);
        Task<Quotation> UpdateAsync(Quotation quotation);
        Task<(IEnumerable<Quotation> Items, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            int? warehouseId = null,
            int? customerId = null,
            int? userId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? status = null
        );
    }
}
