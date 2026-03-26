using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Commands;

/// <summary>
/// Command para registrar un pago a una o varias facturas PPD
/// </summary>
public class CreatePaymentCommand : IRequest<PaymentDto>
{
    public int CustomerId { get; set; }
    public int CompanyId { get; set; }
    public int BranchId { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethodSAT { get; set; } = string.Empty;
    public string? PaymentFormSAT { get; set; }
    public string Currency { get; set; } = "MXN";
    public decimal ExchangeRate { get; set; } = 1.0M;
    public string? BankOrigin { get; set; }
    public string? BankAccountOrigin { get; set; }
    public string? BankDestination { get; set; }
    public string? BankAccountDestination { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public int CreatedBy { get; set; }
    public List<PaymentInvoiceItem> Invoices { get; set; } = new();
}
