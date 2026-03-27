using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Commands;

/// <summary>
/// Command para generar complementos de pago SAT para un pago específico
/// Genera un complemento por cada aplicación de pago (una por factura)
/// </summary>
public class GeneratePaymentComplementsCommand : IRequest<GenerateComplementsResultDto>
{
    public int PaymentId { get; set; }
    public bool SendEmailsAutomatically { get; set; } = false;
    public int ExecutedBy { get; set; }
}
