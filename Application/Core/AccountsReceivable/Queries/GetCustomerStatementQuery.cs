using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Queries;

/// <summary>
/// Query para obtener el estado de cuenta completo de un cliente
/// </summary>
public class GetCustomerStatementQuery : IRequest<CustomerStatementDto?>
{
    public int CustomerId { get; set; }
    public int CompanyId { get; set; }
    public bool IncludeHistory { get; set; } = true;
    public int MaxHistoryRecords { get; set; } = 20;
}
