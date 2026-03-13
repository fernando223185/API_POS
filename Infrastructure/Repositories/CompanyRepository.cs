using Application.Abstractions.Config;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Implementación del repositorio de empresas
    /// </summary>
    public class CompanyRepository : ICompanyRepository
    {
        private readonly POSDbContext _context;

        public CompanyRepository(POSDbContext context)
        {
            _context = context;
        }

        public async Task<Company?> GetByIdAsync(int id)
        {
            return await _context.Companies
                .Include(c => c.CreatedBy)
                .Include(c => c.UpdatedBy)
                .Include(c => c.Branches)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Company?> GetByCodeAsync(string code)
        {
            return await _context.Companies
                .Include(c => c.CreatedBy)
                .Include(c => c.UpdatedBy)
                .FirstOrDefaultAsync(c => c.Code == code);
        }

        public async Task<Company?> GetByTaxIdAsync(string taxId)
        {
            return await _context.Companies
                .FirstOrDefaultAsync(c => c.TaxId == taxId);
        }

        public async Task<List<Company>> GetAllActiveAsync()
        {
            return await _context.Companies
                .Where(c => c.IsActive)
                .OrderBy(c => c.TradeName)
                .ToListAsync();
        }

        public async Task<(List<Company> companies, int totalRecords)> GetPagedAsync(
            int page, 
            int pageSize, 
            bool? isActive = null, 
            string? searchTerm = null)
        {
            var query = _context.Companies
                .Include(c => c.CreatedBy)
                .Include(c => c.UpdatedBy)
                .AsQueryable();

            // Filtrar por estado
            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            // Búsqueda
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(c =>
                    c.Code.ToLower().Contains(search) ||
                    c.LegalName.ToLower().Contains(search) ||
                    c.TradeName.ToLower().Contains(search) ||
                    c.TaxId.ToLower().Contains(search) ||
                    (c.Email != null && c.Email.ToLower().Contains(search))
                );
            }

            var totalRecords = await query.CountAsync();

            var companies = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (companies, totalRecords);
        }

        public async Task<Company> CreateAsync(Company company)
        {
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return company;
        }

        public async Task UpdateAsync(Company company)
        {
            company.UpdatedAt = DateTime.UtcNow;
            _context.Companies.Update(company);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsByTaxIdAsync(string taxId, int? excludeId = null)
        {
            var query = _context.Companies.Where(c => c.TaxId == taxId);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<Company?> GetMainCompanyAsync()
        {
            return await _context.Companies
                .FirstOrDefaultAsync(c => c.IsMainCompany && c.IsActive);
        }

        public async Task<int> CountActiveAsync()
        {
            return await _context.Companies.CountAsync(c => c.IsActive);
        }

        public async Task<string> GetNextCodeAsync()
        {
            var lastCompany = await _context.Companies
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();

            if (lastCompany == null)
            {
                return "COMP-001";
            }

            // Extraer número del último código (formato: COMP-001)
            var lastCode = lastCompany.Code;
            var lastNumber = int.Parse(lastCode.Split('-')[1]);
            var nextNumber = lastNumber + 1;

            return $"COMP-{nextNumber:D3}";
        }
    }
}
