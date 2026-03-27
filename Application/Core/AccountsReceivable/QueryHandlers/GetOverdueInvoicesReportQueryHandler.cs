using Application.Abstractions.AccountsReceivable;
using Application.Core.AccountsReceivable.Queries;
using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.QueryHandlers;

/// <summary>
/// Handler para obtener reporte de facturas vencidas
/// </summary>
public class GetOverdueInvoicesReportQueryHandler : IRequestHandler<GetOverdueInvoicesReportQuery, OverdueInvoicesReportDto>
{
    private readonly IInvoicePPDRepository _invoiceRepository;

    public GetOverdueInvoicesReportQueryHandler(IInvoicePPDRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<OverdueInvoicesReportDto> Handle(GetOverdueInvoicesReportQuery request, CancellationToken cancellationToken)
    {
        // Obtener facturas vencidas
        var overdueInvoices = await _invoiceRepository.GetOverdueAsync(
            request.CompanyId,
            request.MinDaysOverdue);

        var totalOverdue = overdueInvoices.Sum(i => i.BalanceAmount);

        // Agrupar por cliente
        var customerSummaries = overdueInvoices
            .GroupBy(i => new { i.CustomerId, i.CustomerName })
            .Select(g => new OverdueCustomerSummary
            {
                CustomerId = g.Key.CustomerId,
                CustomerName = g.Key.CustomerName,
                OverdueInvoicesCount = g.Count(),
                TotalOverdue = g.Sum(i => i.BalanceAmount),
                OldestDueDate = g.Min(i => i.DueDate),
                MaxDaysOverdue = g.Max(i => i.DaysOverdue),
                CreditStatus = "Active" // Se puede mejorar consultando CustomerCreditPolicy
            })
            .OrderByDescending(c => c.TotalOverdue)
            .ToList();

        return new OverdueInvoicesReportDto
        {
            TotalOverdue = totalOverdue,
            TotalInvoices = overdueInvoices.Count,
            CustomerSummaries = customerSummaries
        };
    }
}
