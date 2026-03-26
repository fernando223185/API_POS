using Application.Abstractions.AccountsReceivable;
using Application.Core.AccountsReceivable.Queries;
using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.QueryHandlers;

/// <summary>
/// Handler para obtener facturas PPD con paginación y filtros
/// </summary>
public class GetInvoicesPPDQueryHandler : IRequestHandler<GetInvoicesPPDQuery, InvoicePPDPageResponse>
{
    private readonly IInvoicePPDRepository _repository;

    public GetInvoicesPPDQueryHandler(IInvoicePPDRepository repository)
    {
        _repository = repository;
    }

    public async Task<InvoicePPDPageResponse> Handle(GetInvoicesPPDQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            customerId: request.CustomerId,
            companyId: request.CompanyId,
            branchId: request.BranchId,
            status: request.Status,
            fromDate: request.FromDate,
            toDate: request.ToDate,
            minDaysOverdue: request.MinDaysOverdue,
            minAmount: request.MinAmount,
            searchTerm: request.SearchTerm);

        var dtos = items.Select(invoice => new InvoicePPDDto
        {
            Id = invoice.Id,
            InvoiceId = invoice.InvoiceId,
            CustomerId = invoice.CustomerId,
            CustomerName = invoice.CustomerName,
            CustomerRFC = invoice.CustomerRFC,
            FolioUUID = invoice.FolioUUID,
            Serie = invoice.Serie,
            Folio = invoice.Folio,
            SerieAndFolio = invoice.SerieAndFolio,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            Currency = invoice.Currency,
            ExchangeRate = invoice.ExchangeRate,
            OriginalAmount = invoice.OriginalAmount,
            PaidAmount = invoice.PaidAmount,
            BalanceAmount = invoice.BalanceAmount,
            NextPartialityNumber = invoice.NextPartialityNumber,
            TotalPartialities = invoice.TotalPartialities,
            Status = invoice.Status,
            DaysOverdue = invoice.DaysOverdue,
            LastPaymentDate = invoice.LastPaymentDate,
            Notes = invoice.Notes,
            CreatedAt = invoice.CreatedAt
        }).ToList();

        return new InvoicePPDPageResponse
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalAmount = dtos.Sum(d => d.OriginalAmount),
            TotalBalance = dtos.Sum(d => d.BalanceAmount)
        };
    }
}
