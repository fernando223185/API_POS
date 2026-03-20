using Application.DTOs.Billing;
using MediatR;

namespace Application.Core.Billing.Commands
{
    /// <summary>
    /// Comando para crear una factura (borrador) a partir de una venta o manualmente
    /// Opcionalmente puede timbrar inmediatamente después de crear
    /// </summary>
    public class CreateInvoiceCommand : IRequest<InvoiceResponseDto>
    {
        public CreateInvoiceRequestDto Request { get; set; } = null!;
        public int UserId { get; set; }
    }
}
