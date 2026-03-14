using Application.Abstractions.Config;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class BranchRepository : IBranchRepository
    {
        private readonly POSDbContext _context;

        public BranchRepository(POSDbContext context)
        {
            _context = context;
        }

        public async Task<Branch?> GetByIdAsync(int id)
        {
            return await _context.Branches
                .Include(b => b.Company)
                .Include(b => b.CreatedBy)
                .Include(b => b.UpdatedBy)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Branch?> GetByCodeAsync(string code)
        {
            return await _context.Branches
                .Include(b => b.Company)
                .FirstOrDefaultAsync(b => b.Code == code);
        }

        public async Task<List<Branch>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.Branches
                .Include(b => b.Company)
                .Include(b => b.CreatedBy)
                .Include(b => b.UpdatedBy)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(b => b.IsActive);
            }

            return await query
                .OrderBy(b => b.Code)
                .ToListAsync();
        }

        public async Task<(List<Branch> branches, int totalRecords)> GetPagedAsync(
            int pageNumber, 
            int pageSize, 
            bool includeInactive = false, 
            string? searchTerm = null)
        {
            var query = _context.Branches
                .Include(b => b.Company)
                .Include(b => b.CreatedBy)
                .Include(b => b.UpdatedBy)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(b => b.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(b =>
                    b.Code.Contains(searchTerm) ||
                    b.Name.Contains(searchTerm) ||
                    b.City.Contains(searchTerm) ||
                    b.State.Contains(searchTerm) ||
                    (b.Company != null && b.Company.LegalName.Contains(searchTerm)));
            }

            var totalRecords = await query.CountAsync();

            var branches = await query
                .OrderBy(b => b.Code)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (branches, totalRecords);
        }

        public async Task<Branch> CreateAsync(Branch branch)
        {
            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();
            return branch;
        }

        public async Task UpdateAsync(Branch branch)
        {
            branch.UpdatedAt = DateTime.UtcNow;
            _context.Branches.Update(branch);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Branches.AnyAsync(b => b.Id == id);
        }

        public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
        {
            var query = _context.Branches.Where(b => b.Code == code);

            if (excludeId.HasValue)
            {
                query = query.Where(b => b.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<string> GenerateNextCodeAsync()
        {
            var lastBranch = await _context.Branches
                .OrderByDescending(b => b.Id)
                .FirstOrDefaultAsync();

            if (lastBranch == null)
            {
                return "SUC-001";
            }

            // Extraer el número del último código (ej: "SUC-005" -> 5)
            var lastNumber = int.Parse(lastBranch.Code.Split('-')[1]);
            var nextNumber = lastNumber + 1;

            return $"SUC-{nextNumber:D3}";
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Branches.CountAsync();
        }

        public async Task<int> GetActiveCountAsync()
        {
            return await _context.Branches.CountAsync(b => b.IsActive);
        }
    }
}
