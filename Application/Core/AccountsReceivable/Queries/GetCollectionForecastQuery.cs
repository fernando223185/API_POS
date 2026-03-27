using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Queries;

/// <summary>
/// Query para obtener proyección de cobranza
/// </summary>
public class GetCollectionForecastQuery : IRequest<CollectionForecastDto>
{
    public int CompanyId { get; set; }
    public DateTime? FromDate { get; set; }
    public int Days { get; set; } = 90;
}
