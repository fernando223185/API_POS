using Application.DTOs.Reports;
using MediatR;

namespace Application.Core.Reports.Queries
{
    public record GetReportTemplateByIdQuery(int TemplateId) : IRequest<ReportTemplateResponseDto?>;

    public record GetReportTemplatesByTypeQuery(
        string ReportType,
        int? CompanyId
    ) : IRequest<List<ReportTemplateSummaryDto>>;

    public record GetReportFieldCatalogQuery(string ReportType) : IRequest<ReportFieldCatalogDto>;

    public record GenerateReportPdfQuery(
        GenerateReportDto Data,
        int UserId
    ) : IRequest<byte[]>;
}
