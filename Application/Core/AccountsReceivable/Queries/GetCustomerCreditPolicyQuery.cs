using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Queries;

/// <summary>
/// Query para obtener la política de crédito de un cliente
/// </summary>
public class GetCustomerCreditPolicyQuery : IRequest<CustomerCreditPolicyDto?>
{
    public int CustomerId { get; set; }
}
