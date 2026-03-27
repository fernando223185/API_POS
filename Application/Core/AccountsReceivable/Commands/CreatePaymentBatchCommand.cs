using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Commands;

/// <summary>
/// Command para crear un lote de pagos masivo con creación automática de pagos
/// </summary>
public class CreatePaymentBatchCommand : IRequest<PaymentBatchDto>
{
    public int CompanyId { get; set; }
    public DateTime PaymentDate { get; set; }
    /// <summary>Folio personalizado. Si null, se genera automáticamente.</summary>
    public string? CustomBatchNumber { get; set; }
    /// <summary>Prefijo para generación automática. Default: LOTE</summary>
    public string BatchPrefix { get; set; } = "LOTE";
    public string? DefaultPaymentMethodSAT { get; set; }
    public string? DefaultPaymentFormSAT { get; set; }
    public string? DefaultBankDestination { get; set; }
    public string? DefaultAccountDestination { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public int CreatedBy { get; set; }
    public List<BatchPaymentItem> Payments { get; set; } = new();
}
