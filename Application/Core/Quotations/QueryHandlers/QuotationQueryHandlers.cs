using Application.Abstractions.Quotations;
using Application.Core.Quotations.Queries;
using Application.DTOs.Quotations;
using MediatR;

namespace Application.Core.Quotations.QueryHandlers
{
    public class GetQuotationByIdQueryHandler : IRequestHandler<GetQuotationByIdQuery, QuotationResponseDto>
    {
        private readonly IQuotationRepository _quotationRepository;

        public GetQuotationByIdQueryHandler(IQuotationRepository quotationRepository)
        {
            _quotationRepository = quotationRepository;
        }

        public async Task<QuotationResponseDto> Handle(GetQuotationByIdQuery request, CancellationToken cancellationToken)
        {
            var quotation = await _quotationRepository.GetByIdAsync(request.QuotationId)
                ?? throw new KeyNotFoundException($"Cotización con ID {request.QuotationId} no encontrada");

            return QuotationMapper.ToResponseDto(quotation);
        }
    }

    public class GetQuotationByCodeQueryHandler : IRequestHandler<GetQuotationByCodeQuery, QuotationResponseDto>
    {
        private readonly IQuotationRepository _quotationRepository;

        public GetQuotationByCodeQueryHandler(IQuotationRepository quotationRepository)
        {
            _quotationRepository = quotationRepository;
        }

        public async Task<QuotationResponseDto> Handle(GetQuotationByCodeQuery request, CancellationToken cancellationToken)
        {
            var quotation = await _quotationRepository.GetByCodeAsync(request.Code)
                ?? throw new KeyNotFoundException($"Cotización con código '{request.Code}' no encontrada");

            return QuotationMapper.ToResponseDto(quotation);
        }
    }

    public class GetQuotationsPagedQueryHandler : IRequestHandler<GetQuotationsPagedQuery, QuotationsPagedResponseDto>
    {
        private readonly IQuotationRepository _quotationRepository;

        public GetQuotationsPagedQueryHandler(IQuotationRepository quotationRepository)
        {
            _quotationRepository = quotationRepository;
        }

        public async Task<QuotationsPagedResponseDto> Handle(GetQuotationsPagedQuery request, CancellationToken cancellationToken)
        {
            var (items, total) = await _quotationRepository.GetPagedAsync(
                page: request.Page,
                pageSize: request.PageSize,
                warehouseId: request.WarehouseId,
                customerId: request.CustomerId,
                userId: request.UserId,
                fromDate: request.FromDate,
                toDate: request.ToDate,
                status: request.Status
            );

            var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);

            return new QuotationsPagedResponseDto
            {
                Items = items.Select(QuotationMapper.ToSummaryDto).ToList(),
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };
        }
    }
}
