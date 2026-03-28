using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Queries;

/// <summary>
/// Query para obtener facturas PPD pendientes de cobro con filtros
/// </summary>
public class GetInvoicesPPDQuery : IRequest<InvoicePPDPagedResultDto>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    
    // Filtros
    public int? CompanyId { get; set; }
    public int? CustomerId { get; set; }
    public string? PaymentStatus { get; set; } // Pending (sin pagar o pago parcial), Paid, Overdue, PartiallyPaid
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? MinDaysOverdue { get; set; }
    public string? SearchTerm { get; set; } // Buscar por folio, serie, nombre cliente, RFC
}
