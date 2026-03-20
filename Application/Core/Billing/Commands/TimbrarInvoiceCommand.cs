using Application.DTOs.Billing;
using MediatR;

namespace Application.Core.Billing.Commands
{
    /// <summary>
    /// Comando para timbrar una factura existente (borrador)
    /// </summary>
    public class TimbrarInvoiceCommand : IRequest<InvoiceResponseDto>
    {
        public TimbrarInvoiceRequestDto Request { get; set; } = null!;
        public int UserId { get; set; }
    }
}
