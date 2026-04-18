using Application.Abstractions.Quotations;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class QuotationRepository : IQuotationRepository
    {
        private readonly POSDbContext _context;

        public QuotationRepository(POSDbContext context)
        {
            _context = context;
        }

        public async Task<Quotation> CreateAsync(Quotation quotation)
        {
            _context.Quotations.Add(quotation);
            await _context.SaveChangesAsync();
            return quotation;
        }

        public async Task<Quotation?> GetByIdAsync(int id)
        {
            return await _context.Quotations
                .Include(q => q.Details)
                .Include(q => q.Customer)
                .Include(q => q.Warehouse)
                    .ThenInclude(w => w.Branch)
                .Include(q => q.Branch)
                .Include(q => q.Company)
                .Include(q => q.User)
                .Include(q => q.PriceList)
                .Include(q => q.CreatedBy)
                .Include(q => q.Sale)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<Quotation?> GetByCodeAsync(string code)
        {
            return await _context.Quotations
                .Include(q => q.Details)
                .Include(q => q.Customer)
                .Include(q => q.Warehouse)
                    .ThenInclude(w => w.Branch)
                .Include(q => q.Branch)
                .Include(q => q.Company)
                .Include(q => q.User)
                .Include(q => q.PriceList)
                .Include(q => q.CreatedBy)
                .Include(q => q.Sale)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Code == code);
        }

        public async Task<Quotation> UpdateAsync(Quotation quotation)
        {
            // Usar ExecuteUpdate para evitar conflictos de tracking con navigation properties
            await _context.Quotations
                .Where(q => q.Id == quotation.Id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(q => q.Status, quotation.Status)
                    .SetProperty(q => q.SaleId, quotation.SaleId)
                    .SetProperty(q => q.ConvertedAt, quotation.ConvertedAt)
                    .SetProperty(q => q.UpdatedAt, quotation.UpdatedAt)
                    .SetProperty(q => q.CancelledAt, quotation.CancelledAt)
                    .SetProperty(q => q.CancelledByUserId, quotation.CancelledByUserId)
                    .SetProperty(q => q.CancellationReason, quotation.CancellationReason)
                );
            return quotation;
        }

        public async Task<(IEnumerable<Quotation> Items, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            int? warehouseId = null,
            int? customerId = null,
            int? userId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? status = null)
        {
            var query = _context.Quotations
                .Include(q => q.Customer)
                .Include(q => q.Warehouse)
                .Include(q => q.Branch)
                .Include(q => q.User)
                .Include(q => q.Details)
                .Include(q => q.Sale)
                .AsQueryable();

            if (warehouseId.HasValue)
                query = query.Where(q => q.WarehouseId == warehouseId.Value);

            if (customerId.HasValue)
                query = query.Where(q => q.CustomerId == customerId.Value);

            if (userId.HasValue)
                query = query.Where(q => q.UserId == userId.Value);

            if (fromDate.HasValue)
                query = query.Where(q => q.QuotationDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(q => q.QuotationDate <= toDate.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(q => q.Status == status);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(q => q.QuotationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
