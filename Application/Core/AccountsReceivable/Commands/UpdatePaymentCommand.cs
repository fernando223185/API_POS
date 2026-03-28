using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Commands;

/// <summary>
/// Command para actualizar datos de un pago antes de timbrar
/// Solo permite editar datos fiscales y bancarios, NO las aplicaciones
/// </summary>
public class UpdatePaymentCommand : IRequest<PaymentDto>
{
    public int PaymentId { get; set; }
    
    // Datos del emisor
    public string? EmisorRfc { get; set; }
    public string? EmisorNombre { get; set; }
    public string? EmisorRegimenFiscal { get; set; }
    public string? LugarExpedicion { get; set; }
    
    // Datos del receptor
    public string? ReceptorRfc { get; set; }
    public string? ReceptorNombre { get; set; }
    public string? ReceptorDomicilioFiscal { get; set; }
    public string? ReceptorRegimenFiscal { get; set; }
    public string? ReceptorUsoCfdi { get; set; }
    
    // Datos bancarios
    public string? BankDestination { get; set; }
    public string? AccountDestination { get; set; }
    public string? Reference { get; set; }
    
    // Notas
    public string? Notes { get; set; }
    
    public int UpdatedBy { get; set; }
}
