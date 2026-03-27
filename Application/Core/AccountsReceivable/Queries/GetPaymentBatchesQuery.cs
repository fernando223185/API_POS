using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Queries;

/// <summary>
/// Query para obtener listado paginado de lotes de pago con filtros
/// </summary>
public class GetPaymentBatchesQuery : IRequest<PaymentBatchPagedResultDto>
{
    public int CompanyId { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SearchTerm { get; set; }
    public bool? HasErrors { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string OrderBy { get; set; } = "CreatedAt";
    public bool Ascending { get; set; } = false;
}
