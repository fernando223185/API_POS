using Application.DTOs.Billing;
using MediatR;

namespace Application.Core.AccountsReceivable.Commands;

/// <summary>
/// Cancela el complemento de pago (CFDI tipo P) ante el SAT vía Sapiens
/// </summary>
public class CancelPaymentComplementCommand : IRequest<CancelInvoiceResultDto>
{
    public int PaymentId { get; set; }
    /// <summary>Motivo SAT: 01=Con sustitución, 02=Sin sustitución, 03=No se realizó, 04=Global</summary>
    public string Motivo { get; set; } = string.Empty;
    /// <summary>UUID del CFDI sustituto. Requerido solo cuando Motivo = "01"</summary>
    public string? FolioSustitucion { get; set; }
    /// <summary>Nota interna (texto libre)</summary>
    public string? Reason { get; set; }
    public int UserId { get; set; }
}
