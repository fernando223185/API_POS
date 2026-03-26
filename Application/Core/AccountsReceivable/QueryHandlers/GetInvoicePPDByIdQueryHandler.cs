using Application.Abstractions.AccountsReceivable;
using Application.Core.AccountsReceivable.Queries;
using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.QueryHandlers;

/// <summary>
/// Handler para obtener una factura PPD por ID
/// </summary>
public class GetInvoicePPDByIdQueryHandler : IRequestHandler<GetInvoicePPDByIdQuery, InvoicePPDDto?>
{
    private readonly IInvoicePPDRepository _repository;

    public GetInvoicePPDByIdQueryHandler(IInvoicePPDRepository repository)
    {
        _repository = repository;
    }

    public async Task<InvoicePPDDto?> Handle(GetInvoicePPDByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _repository.GetByIdAsync(request.Id);

        if (invoice == null)
            return null;

        // Mapear entidad a DTO
        var dto = new InvoicePPDDto
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
        };

        return dto;
    }
}
