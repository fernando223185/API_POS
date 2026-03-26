using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Queries;

/// <summary>
/// Query para obtener una factura PPD por ID
/// </summary>
public class GetInvoicePPDByIdQuery : IRequest<InvoicePPDDto?>
{
    public int Id { get; set; }
}
