using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Queries;

/// <summary>
/// Query para obtener un pago por ID con todas sus aplicaciones
/// </summary>
public class GetPaymentByIdQuery : IRequest<PaymentDto?>
{
    public int Id { get; set; }
}
