using Application.Abstractions.Catalogue;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PriceListRepository : IPriceListRepository
    {
        private readonly POSDbContext _context;

        public PriceListRepository(POSDbContext context) => _context = context;

        public async Task<List<PriceList>> GetAllAsync(bool? isActive = null)
        {
            var query = _context.PriceLists.AsQueryable();
            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);
            return await query.OrderBy(p => p.Name).ToListAsync();
        }

        public Task<PriceList?> GetByIdAsync(int id)
            => _context.PriceLists.FirstOrDefaultAsync(p => p.Id == id);

        public Task<PriceList?> GetDefaultAsync()
            => _context.PriceLists.FirstOrDefaultAsync(p => p.IsDefault && p.IsActive);

        public async Task<PriceList> CreateAsync(PriceList priceList)
        {
            _context.PriceLists.Add(priceList);
            await _context.SaveChangesAsync();
            return priceList;
        }

        public async Task<PriceList> UpdateAsync(PriceList priceList)
        {
            _context.PriceLists.Update(priceList);
            await _context.SaveChangesAsync();
            return priceList;
        }

        public async Task<bool> SetActiveAsync(int id, bool isActive)
        {
            var entity = await _context.PriceLists.FirstOrDefaultAsync(p => p.Id == id);
            if (entity is null) return false;

            entity.IsActive = isActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<bool> CodeExistsAsync(string code, int? excludeId = null)
        {
            var q = _context.PriceLists.Where(p => p.Code == code);
            if (excludeId.HasValue)
                q = q.Where(p => p.Id != excludeId.Value);
            return q.AnyAsync();
        }

        public async Task<bool> HasActiveDependenciesAsync(int id)
        {
            var hasCustomers = await _context.Customer.AnyAsync(c => c.PriceListId == id);
            if (hasCustomers) return true;

            var hasSales = await _context.SalesNew.AnyAsync(s => s.PriceListId == id);
            if (hasSales) return true;

            var hasQuotations = await _context.Quotations.AnyAsync(q => q.PriceListId == id);
            if (hasQuotations) return true;

            var hasProductPrices = await _context.ProductPrices.AnyAsync(pp => pp.PriceListId == id && pp.IsActive);
            return hasProductPrices;
        }

        public async Task ClearDefaultFlagExceptAsync(int keepId)
        {
            var others = await _context.PriceLists
                .Where(p => p.IsDefault && p.Id != keepId)
                .ToListAsync();

            if (others.Count == 0) return;

            foreach (var p in others)
                p.IsDefault = false;

            await _context.SaveChangesAsync();
        }
    }
}
