using Domain.Entities;

namespace Application.Abstractions.Reports
{
    public interface IReportTemplateRepository
    {
        Task<ReportTemplate?> GetByIdAsync(int id);
        Task<ReportTemplate?> GetDefaultByTypeAsync(string reportType, int? companyId);
        Task<List<ReportTemplate>> GetByTypeAsync(string reportType, int? companyId);
        Task<ReportTemplate> CreateAsync(ReportTemplate template);
        Task<ReportTemplate> UpdateAsync(ReportTemplate template);
        Task DeleteAsync(int id);
        Task ClearDefaultForTypeAsync(string reportType, int? companyId);
    }
}
