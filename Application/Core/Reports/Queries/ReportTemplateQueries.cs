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

    public record GetActiveTemplateByTypeQuery(
        string ReportType,
        int? CompanyId
    ) : IRequest<ReportTemplateResponseDto?>;

    public record GetReportPreviewDataQuery(int TemplateId) : IRequest<ReportPreviewDataDto>;

    public record GetReportTemplatePdfPreviewQuery(int TemplateId) : IRequest<byte[]>;

    public record GetReportTemplateHtmlPreviewQuery(int TemplateId) : IRequest<string>;

    public record GetReportFieldCatalogQuery(string ReportType) : IRequest<ReportFieldCatalogDto>;

    /// <summary>
    /// Renderiza un HTML Liquid con datos mock de un tipo de reporte sin necesidad de guardarlo.
    /// Permite al frontend hacer live-preview mientras el usuario edita la plantilla.
    /// </summary>
    public record GetLivePreviewHtmlQuery(
        string ReportType,
        string HtmlTemplate
    ) : IRequest<string>;

    public record GenerateReportPdfQuery(
        GenerateReportDto Data,
        int UserId
    ) : IRequest<byte[]>;
}
