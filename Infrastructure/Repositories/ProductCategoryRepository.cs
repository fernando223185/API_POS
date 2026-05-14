using Application.Abstractions.Catalogue;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private readonly POSDbContext _context;

        public ProductCategoryRepository(POSDbContext context) => _context = context;

        public async Task<ProductCategory?> GetByIdAsync(int id, bool includeSubcategories = false)
        {
            var query = _context.ProductCategories.AsQueryable();
            if (includeSubcategories)
                query = query.Include(c => c.Subcategories);
            return await query.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<ProductCategory>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.ProductCategories
                .Include(c => c.Subcategories)
                .AsQueryable();

            if (!includeInactive)
                query = query.Where(c => c.IsActive);

            return await query.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<ProductCategory> CreateAsync(ProductCategory category)
        {
            _context.ProductCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<ProductCategory> UpdateAsync(ProductCategory category)
        {
            _context.ProductCategories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.ProductCategories.FirstOrDefaultAsync(c => c.Id == id);
            if (entity is null) return false;

            entity.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
        {
            var q = _context.ProductCategories.Where(c => c.Code == code);
            if (excludeId.HasValue)
                q = q.Where(c => c.Id != excludeId.Value);
            return await q.AnyAsync();
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var q = _context.ProductCategories.Where(c => c.Name == name);
            if (excludeId.HasValue)
                q = q.Where(c => c.Id != excludeId.Value);
            return await q.AnyAsync();
        }

        public async Task<bool> HasProductsAsync(int categoryId)
        {
            return await _context.Products.AnyAsync(p => p.CategoryId == categoryId);
        }

        public async Task<int> CountProductsAsync(int categoryId, bool onlyActive = false)
        {
            var q = _context.Products.Where(p => p.CategoryId == categoryId);
            if (onlyActive)
                q = q.Where(p => p.IsActive);
            return await q.CountAsync();
        }

        public async Task<int> CountSubcategoriesAsync(int categoryId, bool onlyActive = true)
        {
            var q = _context.ProductSubcategories.Where(s => s.CategoryId == categoryId);
            if (onlyActive)
                q = q.Where(s => s.IsActive);
            return await q.CountAsync();
        }

        public async Task<List<CategoryStatsRow>> GetStatsAsync()
        {
            return await _context.ProductCategories
                .Where(c => c.IsActive)
                .Select(c => new CategoryStatsRow
                {
                    CategoryId         = c.Id,
                    CategoryName       = c.Name,
                    CategoryCode       = c.Code,
                    ProductsCount      = c.Products.Count(p => p.IsActive),
                    TotalProducts      = c.Products.Count(),
                    SubcategoriesCount = c.Subcategories.Count(s => s.IsActive),
                    AvgPrice           = c.Products.Any() ? c.Products.Average(p => p.price) : 0,
                    TotalValue         = c.Products.Sum(p => p.price * (p.MinimumStock + p.MaximumStock) / 2),
                })
                .ToListAsync();
        }
    }
}
