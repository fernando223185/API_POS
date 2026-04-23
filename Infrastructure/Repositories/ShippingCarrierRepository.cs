using Application.Abstractions.Shipping;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ShippingCarrierRepository : IShippingCarrierRepository
    {
        private readonly POSDbContext _context;

        public ShippingCarrierRepository(POSDbContext context) => _context = context;

        public async Task<ShippingCarrier?> GetByIdAsync(int id)
            => await _context.ShippingCarriers.FirstOrDefaultAsync(c => c.Id == id);

        public async Task<ShippingCarrier?> GetByCodeAsync(string code)
            => await _context.ShippingCarriers.FirstOrDefaultAsync(c => c.Code == code);

        public async Task<List<ShippingCarrier>> GetAllAsync(int? companyId, bool includeInactive = false)
        {
            var q = _context.ShippingCarriers.AsQueryable();
            if (companyId.HasValue)
                q = q.Where(c => c.CompanyId == companyId || c.CompanyId == null);
            if (!includeInactive)
                q = q.Where(c => c.IsActive);
            return await q.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
        {
            var q = _context.ShippingCarriers.Where(c => c.Code == code);
            if (excludeId.HasValue)
                q = q.Where(c => c.Id != excludeId.Value);
            return await q.AnyAsync();
        }

        public async Task<ShippingCarrier> CreateAsync(ShippingCarrier carrier)
        {
            _context.ShippingCarriers.Add(carrier);
            await _context.SaveChangesAsync();
            return carrier;
        }

        public async Task<ShippingCarrier> UpdateAsync(ShippingCarrier carrier)
        {
            _context.ShippingCarriers.Update(carrier);
            await _context.SaveChangesAsync();
            return carrier;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity is null) return false;
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
