using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Queries;

/// <summary>
/// Query para obtener reporte de facturas vencidas
/// </summary>
public class GetOverdueInvoicesReportQuery : IRequest<OverdueInvoicesReportDto>
{
    public int CompanyId { get; set; }
    public int? MinDaysOverdue { get; set; }
}
