using Application.Abstractions.Config;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly POSDbContext _context;

        public WarehouseRepository(POSDbContext context)
        {
            _context = context;
        }

        public async Task<Warehouse?> GetByIdAsync(int id)
        {
            return await _context.Warehouses
                .Include(w => w.Branch)
                .Include(w => w.CreatedBy)
                .Include(w => w.UpdatedBy)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<Warehouse?> GetByCodeAsync(string code)
        {
            return await _context.Warehouses
                .Include(w => w.Branch)
                .FirstOrDefaultAsync(w => w.Code == code);
        }

        public async Task<Warehouse?> GetMainByBranchIdAsync(int branchId)
        {
            return await _context.Warehouses
                .Include(w => w.Branch)
                .Where(w => w.BranchId == branchId && w.IsActive && w.IsMainWarehouse)
                .OrderBy(w => w.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Warehouse>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.Warehouses
                .Include(w => w.Branch)
                .Include(w => w.CreatedBy)
                .Include(w => w.UpdatedBy)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(w => w.IsActive);
            }

            return await query
                .OrderBy(w => w.Branch.Code)
                .ThenBy(w => w.Code)
                .ToListAsync();
        }

        public async Task<List<Warehouse>> GetByBranchIdAsync(int branchId, bool includeInactive = false)
        {
            var query = _context.Warehouses
                .Include(w => w.Branch)
                .Include(w => w.CreatedBy)
                .Include(w => w.UpdatedBy)
                .Where(w => w.BranchId == branchId)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(w => w.IsActive);
            }

            return await query
                .OrderBy(w => w.Code)
                .ToListAsync();
        }

        public async Task<(List<Warehouse> warehouses, int totalRecords)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            bool includeInactive = false,
            string? searchTerm = null,
            int? branchId = null)
        {
            var query = _context.Warehouses
                .Include(w => w.Branch)
                .Include(w => w.CreatedBy)
                .Include(w => w.UpdatedBy)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(w => w.IsActive);
            }

            if (branchId.HasValue)
            {
                query = query.Where(w => w.BranchId == branchId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(w =>
                    w.Code.Contains(searchTerm) ||
                    w.Name.Contains(searchTerm) ||
                    w.Branch.Name.Contains(searchTerm) ||
                    w.WarehouseType.Contains(searchTerm));
            }

            var totalRecords = await query.CountAsync();

            var warehouses = await query
                .OrderBy(w => w.Branch.Code)
                .ThenBy(w => w.Code)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (warehouses, totalRecords);
        }

        public async Task<Warehouse> CreateAsync(Warehouse warehouse)
        {
            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();
            
            // Recargar con navegación
            return await GetByIdAsync(warehouse.Id) ?? warehouse;
        }

        public async Task UpdateAsync(Warehouse warehouse)
        {
            warehouse.UpdatedAt = DateTime.UtcNow;
            _context.Warehouses.Update(warehouse);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Warehouses.AnyAsync(w => w.Id == id);
        }

        public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
        {
            var query = _context.Warehouses.Where(w => w.Code == code);

            if (excludeId.HasValue)
            {
                query = query.Where(w => w.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> BranchExistsAsync(int branchId)
        {
            return await _context.Branches.AnyAsync(b => b.Id == branchId && b.IsActive);
        }

        public async Task<string> GenerateNextCodeAsync()
        {
            var lastWarehouse = await _context.Warehouses
                .OrderByDescending(w => w.Id)
                .FirstOrDefaultAsync();

            if (lastWarehouse == null)
            {
                return "ALM-001";
            }

            // Extraer el número del último código (ej: "ALM-005" -> 5)
            var lastNumber = int.Parse(lastWarehouse.Code.Split('-')[1]);
            var nextNumber = lastNumber + 1;

            return $"ALM-{nextNumber:D3}";
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Warehouses.CountAsync();
        }

        public async Task<int> GetActiveCountAsync()
        {
            return await _context.Warehouses.CountAsync(w => w.IsActive);
        }

        public async Task<int> GetCountByBranchAsync(int branchId)
        {
            return await _context.Warehouses
                .CountAsync(w => w.BranchId == branchId && w.IsActive);
        }
    }
}
