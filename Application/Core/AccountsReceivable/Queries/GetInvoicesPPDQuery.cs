using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Queries;

/// <summary>
/// Query para obtener facturas PPD pendientes con paginación y filtros
/// </summary>
public class GetInvoicesPPDQuery : IRequest<InvoicePPDPageResponse>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? CustomerId { get; set; }
    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? MinDaysOverdue { get; set; }
    public decimal? MinAmount { get; set; }
    public string? SearchTerm { get; set; }
}
