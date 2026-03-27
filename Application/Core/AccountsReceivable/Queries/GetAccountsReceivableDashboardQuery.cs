using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Queries;

/// <summary>
/// Query para obtener dashboard de cuentas por cobrar
/// </summary>
public class GetAccountsReceivableDashboardQuery : IRequest<AccountsReceivableDashboardDto>
{
    public int CompanyId { get; set; }
}
