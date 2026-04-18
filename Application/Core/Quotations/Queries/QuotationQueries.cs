using Application.DTOs.Quotations;
using MediatR;

namespace Application.Core.Quotations.Queries
{
    public record GetQuotationByIdQuery(int QuotationId) : IRequest<QuotationResponseDto>;

    public record GetQuotationByCodeQuery(string Code) : IRequest<QuotationResponseDto>;

    public record GetQuotationsPagedQuery(
        int Page = 1,
        int PageSize = 20,
        int? WarehouseId = null,
        int? CustomerId = null,
        int? UserId = null,
        DateTime? FromDate = null,
        DateTime? ToDate = null,
        string? Status = null
    ) : IRequest<QuotationsPagedResponseDto>;
}
