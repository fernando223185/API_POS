using Application.Abstractions.Reports;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ReportTemplateRepository : IReportTemplateRepository
    {
        private readonly POSDbContext _context;

        public ReportTemplateRepository(POSDbContext context)
            => _context = context;

        public async Task<ReportTemplate?> GetByIdAsync(int id)
            => await _context.ReportTemplates
                .Include(t => t.CreatedBy)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

        public async Task<ReportTemplate?> GetDefaultByTypeAsync(string reportType, int? companyId)
            => await _context.ReportTemplates
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.ReportType == reportType &&
                    t.IsDefault &&
                    t.IsActive &&
                    (t.CompanyId == companyId || t.CompanyId == null));

        public async Task<List<ReportTemplate>> GetByTypeAsync(string reportType, int? companyId)
            => await _context.ReportTemplates
                .Where(t =>
                    t.ReportType == reportType &&
                    t.IsActive &&
                    (companyId == null || t.CompanyId == null || t.CompanyId == companyId))
                .OrderByDescending(t => t.IsDefault)
                .ThenBy(t => t.Name)
                .AsNoTracking()
                .ToListAsync();

        public async Task<List<ReportTemplate>> GetAllAsync(int? companyId)
            => await _context.ReportTemplates
                .Where(t =>
                    t.IsActive &&
                    (companyId == null || t.CompanyId == null || t.CompanyId == companyId))
                .OrderBy(t => t.ReportType)
                .ThenByDescending(t => t.IsDefault)
                .ThenBy(t => t.Name)
                .AsNoTracking()
                .ToListAsync();

        public async Task<ReportTemplate> CreateAsync(ReportTemplate template)
        {
            _context.ReportTemplates.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<ReportTemplate> UpdateAsync(ReportTemplate template)
        {
            _context.ReportTemplates.Update(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task DeleteAsync(int id)
        {
            await _context.ReportTemplates
                .Where(t => t.Id == id)
                .ExecuteDeleteAsync();
        }

        public async Task ClearDefaultForTypeAsync(string reportType, int? companyId)
        {
            await _context.ReportTemplates
                .Where(t =>
                    t.ReportType == reportType &&
                    (t.CompanyId == companyId || (companyId == null && t.CompanyId == null)))
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(t => t.IsDefault, false)
                     .SetProperty(t => t.IsActive, false));
        }
    }
}
