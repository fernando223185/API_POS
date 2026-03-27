using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Queries;

/// <summary>
/// Query para obtener métricas de CxC (DSO, tasa de morosidad, etc.)
/// </summary>
public class GetAccountsReceivableMetricsQuery : IRequest<AccountsReceivableMetricsDto>
{
    public int CompanyId { get; set; }
}
