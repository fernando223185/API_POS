using Application.Abstractions.AccountsReceivable;
using Application.Abstractions.Billing;
using Application.Core.AccountsReceivable.Queries;
using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.QueryHandlers;

/// <summary>
/// Handler para obtener reporte de facturas vencidas
/// </summary>
public class GetOverdueInvoicesReportQueryHandler : IRequestHandler<GetOverdueInvoicesReportQuery, OverdueInvoicesReportDto>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetOverdueInvoicesReportQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<OverdueInvoicesReportDto> Handle(GetOverdueInvoicesReportQuery request, CancellationToken cancellationToken)
    {
        // Obtener facturas vencidas
        var overdueInvoices = await _invoiceRepository.GetOverduePPDInvoicesAsync(
            request.CompanyId,
            request.MinDaysOverdue);

        var totalOverdue = overdueInvoices.Sum(i => i.BalanceAmount ?? 0);

        // Agrupar por cliente
        var customerSummaries = overdueInvoices
            .GroupBy(i => new { i.CustomerId, CustomerName = i.ReceptorNombre })
            .Select(g => new OverdueCustomerSummary
            {
                CustomerId = g.Key.CustomerId ?? 0,
                CustomerName = g.Key.CustomerName,
                OverdueInvoicesCount = g.Count(),
                TotalOverdue = g.Sum(i => i.BalanceAmount ?? 0),
                OldestDueDate = g.Min(i => i.DueDate) ?? DateTime.Today,
                MaxDaysOverdue = g.Max(i => i.DaysOverdue) ?? 0,
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
