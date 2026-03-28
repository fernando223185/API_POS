using Application.Abstractions.AccountsReceivable;
using Application.Abstractions.Billing;
using Application.Core.AccountsReceivable.Queries;
using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.QueryHandlers;

/// <summary>
/// Handler para obtener proyección de cobranza
/// </summary>
public class GetCollectionForecastQueryHandler : IRequestHandler<GetCollectionForecastQuery, CollectionForecastDto>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetCollectionForecastQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<CollectionForecastDto> Handle(GetCollectionForecastQuery request, CancellationToken cancellationToken)
    {
        var fromDate = request.FromDate ?? DateTime.Today;
        var toDate = fromDate.AddDays(request.Days);

        // Obtener facturas pendientes
        var (invoices, _) = await _invoiceRepository.GetPPDPagedAsync(
            pageNumber: 1,
            pageSize: int.MaxValue,
            companyId: request.CompanyId,
            paymentStatus: "Pending");

        // Filtrar por rango de fechas de vencimiento
        var forecastInvoices = invoices
            .Where(i => i.DueDate >= fromDate && i.DueDate <= toDate)
            .ToList();

        // Agrupar por semanas
        var periods = new List<CollectionForecastPeriod>();
        var currentDate = fromDate;

        while (currentDate <= toDate)
        {
            var periodEnd = currentDate.AddDays(7);
            if (periodEnd > toDate)
                periodEnd = toDate;

            var periodInvoices = forecastInvoices
                .Where(i => i.DueDate >= currentDate && i.DueDate < periodEnd)
                .ToList();

            periods.Add(new CollectionForecastPeriod
            {
                PeriodName = $"{currentDate:dd/MM} - {periodEnd:dd/MM}",
                StartDate = currentDate,
                EndDate = periodEnd,
                Amount = periodInvoices.Sum(i => i.BalanceAmount ?? 0),
                InvoiceCount = periodInvoices.Count
            });

            currentDate = periodEnd;
        }

        return new CollectionForecastDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            Periods = periods,
            TotalProjected = periods.Sum(p => p.Amount)
        };
    }
}
