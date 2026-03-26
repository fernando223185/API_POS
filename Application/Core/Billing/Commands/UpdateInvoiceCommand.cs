using Application.DTOs.Billing;
using MediatR;

namespace Application.Core.Billing.Commands
{
    /// <summary>
    /// Comando para actualizar una factura en estado Borrador
    /// </summary>
    public class UpdateInvoiceCommand : IRequest<InvoiceResponseDto>
    {
        public int InvoiceId { get; set; }
        public UpdateInvoiceRequestDto Request { get; set; } = null!;
        public int UserId { get; set; }
    }
}
