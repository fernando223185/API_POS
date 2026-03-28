using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Queries;

/// <summary>
/// Query para obtener una factura PPD por ID con información completa de cobranza
/// </summary>
public class GetInvoicePPDByIdQuery : IRequest<InvoicePPDDetailDto?>
{
    public int InvoiceId { get; set; }
}
