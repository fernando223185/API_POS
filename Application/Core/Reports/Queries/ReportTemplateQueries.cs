using Application.DTOs.Reports;
using MediatR;

namespace Application.Core.Reports.Queries
{
    public record GetReportTemplateByIdQuery(int TemplateId) : IRequest<ReportTemplateResponseDto?>;

    public record GetReportTemplatesByTypeQuery(
        string ReportType,
        int? CompanyId
    ) : IRequest<List<ReportTemplateSummaryDto>>;

    public record GetAllReportTemplatesQuery(int? CompanyId) : IRequest<List<ReportTemplateSummaryDto>>;

    public record GetReportPreviewDataQuery(int TemplateId) : IRequest<ReportPreviewDataDto>;

    public record GetReportTemplatePdfPreviewQuery(int TemplateId) : IRequest<byte[]>;

    public record GetReportTemplateHtmlPreviewQuery(int TemplateId) : IRequest<string>;

    public record GetReportFieldCatalogQuery(string ReportType) : IRequest<ReportFieldCatalogDto>;

    public record GenerateReportPdfQuery(
        GenerateReportDto Data,
        int UserId
    ) : IRequest<byte[]>;
}
