using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Queries;

/// <summary>
/// Query para obtener pagos paginados con filtros
/// </summary>
public class GetPaymentsQuery : IRequest<PaymentPagedResultDto>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    
    // Filtros
    public int? CompanyId { get; set; }
    public int? CustomerId { get; set; }
    public string? Status { get; set; } // Applied, Pending, Cancelled
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? HasComplement { get; set; } // Filtrar por pagos con o sin complemento timbrado
    public string? SearchTerm { get; set; } // Buscar por número de pago, referencia, nombre cliente
}
