using Application.Abstractions.AccountsReceivable;
using Application.Abstractions.Billing;
using Application.Core.AccountsReceivable.Queries;
using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.QueryHandlers;

/// <summary>
/// Handler para obtener el dashboard principal de Cuentas por Cobrar
/// </summary>
public class GetAccountsReceivableDashboardQueryHandler : IRequestHandler<GetAccountsReceivableDashboardQuery, AccountsReceivableDashboardDto>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetAccountsReceivableDashboardQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<AccountsReceivableDashboardDto> Handle(GetAccountsReceivableDashboardQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.Today;
        
        // Obtener todas las facturas pendientes
        var (invoices, _) = await _invoiceRepository.GetPPDPagedAsync(
            pageNumber: 1,
            pageSize: int.MaxValue,
            companyId: request.CompanyId,
            paymentStatus: "Pending");

        // Calcular métricas principales
        var totalReceivable = invoices.Sum(i => i.BalanceAmount ?? 0);
        var totalOverdue = invoices.Where(i => i.DueDate < today).Sum(i => i.BalanceAmount ?? 0);
        var totalDueToday = invoices.Where(i => i.DueDate == today).Sum(i => i.BalanceAmount ?? 0);
        var totalNotDue = invoices.Where(i => i.DueDate > today).Sum(i => i.BalanceAmount ?? 0);

        // Reporte de antigüedad
        var aging = new AgingReportDto
        {
            Current = invoices.Where(i => (i.DaysOverdue ?? 0) <= 0).Sum(i => i.BalanceAmount ?? 0),
            Days31To60 = invoices.Where(i => (i.DaysOverdue ?? 0) >= 31 && (i.DaysOverdue ?? 0) <= 60).Sum(i => i.BalanceAmount ?? 0),
            Days61To90 = invoices.Where(i => (i.DaysOverdue ?? 0) >= 61 && (i.DaysOverdue ?? 0) <= 90).Sum(i => i.BalanceAmount ?? 0),
            Over90Days = invoices.Where(i => (i.DaysOverdue ?? 0) > 90).Sum(i => i.BalanceAmount ?? 0),
            CurrentCount = invoices.Count(i => (i.DaysOverdue ?? 0) <= 0),
            Days31To60Count = invoices.Count(i => (i.DaysOverdue ?? 0) >= 31 && (i.DaysOverdue ?? 0) <= 60),
            Days61To90Count = invoices.Count(i => (i.DaysOverdue ?? 0) >= 61 && (i.DaysOverdue ?? 0) <= 90),
            Over90DaysCount = invoices.Count(i => (i.DaysOverdue ?? 0) > 90)
        };

        // Top clientes
        var topCustomers = invoices
            .GroupBy(i => new { i.CustomerId, CustomerName = i.ReceptorNombre })
            .Select(g => new TopCustomerBalanceDto
            {
                CustomerId = g.Key.CustomerId ?? 0,
                CustomerName = g.Key.CustomerName,
                TotalBalance = g.Sum(i => i.BalanceAmount ?? 0),
                OverdueAmount = g.Where(i => (i.DaysOverdue ?? 0) > 0).Sum(i => i.BalanceAmount ?? 0),
                InvoiceCount = g.Count(),
                CreditStatus = "Active"
            })
            .OrderByDescending(c => c.TotalBalance)
            .Take(10)
            .ToList();

        // Clientes únicos con saldo
        var customersWithBalance = invoices.Select(i => i.CustomerId).Distinct().Count();

        // Porcentaje de facturas vencidas
        var overduePercentage = totalReceivable > 0 ? (totalOverdue / totalReceivable * 100) : 0;

        return new AccountsReceivableDashboardDto
        {
            TotalReceivable = totalReceivable,
            TotalOverdue = totalOverdue,
            TotalDueToday = totalDueToday,
            TotalNotDue = totalNotDue,
            TotalInvoicesPending = invoices.Count,
            CustomersWithBalance = customersWithBalance,
            AverageCollectionDays = invoices.Any() ? (int)invoices.Average(i => (today - i.InvoiceDate).Days) : 0,
            CollectedThisMonth = 0, // Se calcularía desde Payment repository
            OverduePercentage = overduePercentage,
            AgingReport = aging,
            TopCustomers = topCustomers
        };
    }
}
