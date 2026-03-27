using Application.Abstractions.AccountsReceivable;
using Application.Core.AccountsReceivable.Queries;
using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.QueryHandlers;

/// <summary>
/// Handler para obtener listado paginado de lotes de pago
/// </summary>
public class GetPaymentBatchesQueryHandler : IRequestHandler<GetPaymentBatchesQuery, PaymentBatchPagedResultDto>
{
    private readonly IPaymentBatchRepository _repository;

    public GetPaymentBatchesQueryHandler(IPaymentBatchRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaymentBatchPagedResultDto> Handle(GetPaymentBatchesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            companyId: request.CompanyId,
            status: request.Status,
            fromDate: request.FromDate,
            toDate: request.ToDate,
            searchTerm: request.SearchTerm,
            hasErrors: request.HasErrors,
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            orderBy: request.OrderBy,
            ascending: request.Ascending);

        var dtos = items.Select(batch => new PaymentBatchListItemDto
        {
            Id = batch.Id,
            BatchNumber = batch.BatchNumber,
            CompanyId = batch.CompanyId,
            CompanyName = batch.Company?.TradeName ?? batch.Company?.LegalName ?? "",
            PaymentDate = batch.PaymentDate,
            TotalPayments = batch.TotalPayments,
            TotalInvoices = batch.TotalInvoices,
            TotalAmount = batch.TotalAmount,
            Currency = batch.Currency,
            Status = batch.Status,
            StatusDescription = GetStatusDescription(batch.Status),
            ProcessingProgress = batch.ProcessingProgress,
            ComplementsGenerated = batch.ComplementsGenerated,
            ComplementsWithError = batch.ComplementsWithError,
            Description = batch.Description,
            Notes = batch.Notes,
            CreatedAt = batch.CreatedAt,
            ConfirmedAt = batch.ConfirmedAt,
            CompletedAt = batch.CompletedAt,
            CreatedBy = batch.CreatedBy
        }).ToList();

        return new PaymentBatchPagedResultDto
        {
            Data = dtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    private static string GetStatusDescription(string status)
    {
        return status switch
        {
            "Draft" => "Borrador",
            "Confirmed" => "Confirmado",
            "Processing" => "Procesando",
            "Completed" => "Completado",
            "PartialError" => "Completado con Errores",
            "Cancelled" => "Cancelado",
            _ => status
        };
    }
}
