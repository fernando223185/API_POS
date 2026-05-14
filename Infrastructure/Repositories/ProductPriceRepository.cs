using Application.Abstractions.Catalogue;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ProductPriceRepository : IProductPriceRepository
    {
        private readonly POSDbContext _context;

        public ProductPriceRepository(POSDbContext context) => _context = context;

        public Task<ProductPrice?> GetByIdAsync(int id)
            => _context.ProductPrices
                .Include(p => p.Product)
                .Include(p => p.PriceList)
                .Include(p => p.CreatedBy)
                .FirstOrDefaultAsync(p => p.Id == id);

        public Task<ProductPrice?> GetByProductAndListAsync(int productId, int priceListId, bool includeInactive = false)
        {
            var q = _context.ProductPrices
                .Include(p => p.Product)
                .Include(p => p.PriceList)
                .Where(p => p.ProductId == productId && p.PriceListId == priceListId);

            if (!includeInactive)
                q = q.Where(p => p.IsActive);

            return q.FirstOrDefaultAsync();
        }

        public async Task<List<ProductPrice>> ListAsync(int? productId, int? priceListId, bool onlyActive = true)
        {
            var q = _context.ProductPrices
                .Include(p => p.Product)
                .Include(p => p.PriceList)
                .AsQueryable();

            if (productId.HasValue) q = q.Where(p => p.ProductId == productId.Value);
            if (priceListId.HasValue) q = q.Where(p => p.PriceListId == priceListId.Value);
            if (onlyActive) q = q.Where(p => p.IsActive);

            return await q.OrderBy(p => p.PriceList.Name).ThenBy(p => p.Product.name).ToListAsync();
        }

        public async Task<List<ProductPrice>> GetByProductAsync(int productId, bool onlyActive = true)
        {
            var q = _context.ProductPrices
                .Include(p => p.PriceList)
                .Where(p => p.ProductId == productId);
            if (onlyActive) q = q.Where(p => p.IsActive);
            return await q.OrderBy(p => p.PriceList.Name).ToListAsync();
        }

        public async Task<List<ProductPrice>> GetByPriceListAsync(int priceListId, bool onlyActive = true)
        {
            var q = _context.ProductPrices
                .Include(p => p.Product)
                .Where(p => p.PriceListId == priceListId);
            if (onlyActive) q = q.Where(p => p.IsActive);
            return await q.OrderBy(p => p.Product.name).ToListAsync();
        }

        public async Task<ProductPrice> CreateAsync(ProductPrice entity)
        {
            _context.ProductPrices.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<ProductPrice> UpdateAsync(ProductPrice entity)
        {
            _context.ProductPrices.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> SetActiveAsync(int id, bool isActive)
        {
            var entity = await _context.ProductPrices.FirstOrDefaultAsync(p => p.Id == id);
            if (entity is null) return false;
            entity.IsActive = isActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<bool> ProductExistsAsync(int productId)
            => _context.Products.AnyAsync(p => p.ID == productId);

        public Task<bool> PriceListExistsAndActiveAsync(int priceListId)
            => _context.PriceLists.AnyAsync(p => p.Id == priceListId && p.IsActive);

        public async Task<(int Inserted, int Updated)> BulkUpsertAsync(
            int priceListId,
            IEnumerable<(int ProductId, decimal Price, decimal DiscountPercentage)> items,
            int createdByUserId)
        {
            var itemList = items.ToList();
            if (itemList.Count == 0) return (0, 0);

            var productIds = itemList.Select(i => i.ProductId).Distinct().ToList();

            var existing = await _context.ProductPrices
                .Where(p => p.PriceListId == priceListId && productIds.Contains(p.ProductId))
                .ToListAsync();

            var existingMap = existing.ToDictionary(p => p.ProductId);

            int inserted = 0, updated = 0;
            var now = DateTime.UtcNow;

            foreach (var item in itemList)
            {
                if (existingMap.TryGetValue(item.ProductId, out var current))
                {
                    current.Price = item.Price;
                    current.DiscountPercentage = item.DiscountPercentage;
                    current.IsActive = true;
                    updated++;
                }
                else
                {
                    _context.ProductPrices.Add(new ProductPrice
                    {
                        ProductId = item.ProductId,
                        PriceListId = priceListId,
                        Price = item.Price,
                        DiscountPercentage = item.DiscountPercentage,
                        ValidFrom = now,
                        ValidTo = null,
                        IsActive = true,
                        CreatedAt = now,
                        CreatedByUserId = createdByUserId
                    });
                    inserted++;
                }
            }

            await _context.SaveChangesAsync();
            return (inserted, updated);
        }
    }
}
