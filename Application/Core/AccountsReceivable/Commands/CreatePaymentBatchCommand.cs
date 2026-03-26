using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Commands;

/// <summary>
/// Command para crear un lote de pagos masivo
/// </summary>
public class CreatePaymentBatchCommand : IRequest<PaymentBatchDto>
{
    public int CompanyId { get; set; }
    public int BranchId { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? DefaultPaymentMethodSAT { get; set; }
    public string? DefaultBankDestination { get; set; }
    public string? DefaultAccountDestination { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public int CreatedBy { get; set; }
    public List<BatchPaymentItem> Payments { get; set; } = new();
}
