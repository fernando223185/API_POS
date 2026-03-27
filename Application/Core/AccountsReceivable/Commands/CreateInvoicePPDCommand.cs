using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Commands;

/// <summary>
/// Command para crear una factura PPD desde una venta/factura existente
/// Se ejecuta cuando se timbre una factura con método de pago PPD
/// </summary>
public class CreateInvoicePPDCommand : IRequest<InvoicePPDDto>
{
    public int InvoiceId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerRFC { get; set; } = string.Empty;
    public string? CustomerZipCode { get; set; }
    public string? CustomerTaxRegime { get; set; }
    public string? CustomerCfdiUse { get; set; } = "CP01"; // CP01 para complemento de pago
    public int CompanyId { get; set; }
    public string FolioUUID { get; set; } = string.Empty;
    public string? Serie { get; set; }
    public string? Folio { get; set; }
    public DateTime InvoiceDate { get; set; }
    public int CreditDays { get; set; } = 30;
    public string Currency { get; set; } = "MXN";
    public decimal ExchangeRate { get; set; } = 1.0M;
    public decimal TotalAmount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public int CreatedBy { get; set; }
    public string? Notes { get; set; }
}
