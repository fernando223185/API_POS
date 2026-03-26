using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Queries;

/// <summary>
/// Query para obtener un batch por ID
/// </summary>
public class GetPaymentBatchByIdQuery : IRequest<PaymentBatchDto?>
{
    public int Id { get; set; }
}
