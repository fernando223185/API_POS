using Application.DTOs.Reports;
using MediatR;

namespace Application.Core.Reports.Commands
{
    public record CreateReportTemplateCommand(
        CreateReportTemplateDto Data,
        int UserId,
        int? CompanyId
    ) : IRequest<ReportTemplateResponseDto>;

    public record UpdateReportTemplateCommand(
        int TemplateId,
        UpdateReportTemplateDto Data,
        int UserId
    ) : IRequest<ReportTemplateResponseDto>;

    public record DeleteReportTemplateCommand(
        int TemplateId,
        int UserId
    ) : IRequest<bool>;

    public record SetDefaultTemplateCommand(
        int TemplateId,
        int UserId
    ) : IRequest<bool>;
}
