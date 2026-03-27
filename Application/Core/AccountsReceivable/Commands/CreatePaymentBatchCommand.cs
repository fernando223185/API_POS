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
    /// <summary>Folio personalizado. Si null, se genera automáticamente con formato BTCP-[CompanyCode]-[DDMMYY]-[001]</summary>
    public string? CustomBatchNumber { get; set; }
    public string? PaymentFormSAT { get; set; }
    public string? BankDestination { get; set; }
    public string? AccountDestination { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public int CreatedBy { get; set; }
    public List<BatchPaymentItem> Payments { get; set; } = new();
}
