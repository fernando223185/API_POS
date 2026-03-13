using Application.Abstractions.Purchasing;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly POSDbContext _context;

        public SupplierRepository(POSDbContext context)
        {
            _context = context;
        }

        public async Task<Supplier?> GetByIdAsync(int id)
        {
            return await _context.Set<Supplier>()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Supplier?> GetByCodeAsync(string code)
        {
            return await _context.Set<Supplier>()
                .FirstOrDefaultAsync(s => s.Code == code);
        }

        public async Task<List<Supplier>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.Set<Supplier>().AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(s => s.IsActive);
            }

            return await query
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<(List<Supplier> suppliers, int totalRecords)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            bool includeInactive = false,
            string? searchTerm = null)
        {
            var query = _context.Set<Supplier>().AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(s => s.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(s =>
                    s.Name.Contains(searchTerm) ||
                    s.Code.Contains(searchTerm) ||
                    (s.TaxId != null && s.TaxId.Contains(searchTerm)) ||
                    (s.ContactPerson != null && s.ContactPerson.Contains(searchTerm)) ||
                    (s.Email != null && s.Email.Contains(searchTerm)));
            }

            var totalRecords = await query.CountAsync();

            var suppliers = await query
                .OrderBy(s => s.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (suppliers, totalRecords);
        }

        public async Task<Supplier> CreateAsync(Supplier supplier)
        {
            _context.Set<Supplier>().Add(supplier);
            await _context.SaveChangesAsync();
            return supplier;
        }

        public async Task UpdateAsync(Supplier supplier)
        {
            supplier.UpdatedAt = DateTime.UtcNow;
            _context.Set<Supplier>().Update(supplier);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Set<Supplier>().AnyAsync(s => s.Id == id);
        }

        public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
        {
            var query = _context.Set<Supplier>().Where(s => s.Code == code);

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> TaxIdExistsAsync(string taxId, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(taxId))
                return false;

            var query = _context.Set<Supplier>().Where(s => s.TaxId == taxId);

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Set<Supplier>().CountAsync();
        }

        public async Task<int> GetActiveCountAsync()
        {
            return await _context.Set<Supplier>().CountAsync(s => s.IsActive);
        }

        public async Task<decimal> GetTotalPurchasedAsync(int supplierId, DateTime? startDate = null)
        {
            var query = _context.PurchaseOrders
                .Where(po => po.SupplierId == supplierId && po.IsActive);

            if (startDate.HasValue)
            {
                query = query.Where(po => po.OrderDate >= startDate.Value);
            }

            return await query.SumAsync(po => po.Total);
        }

        public async Task<int> GetPurchaseOrderCountAsync(int supplierId)
        {
            return await _context.PurchaseOrders
                .CountAsync(po => po.SupplierId == supplierId && po.IsActive);
        }
    }
}
