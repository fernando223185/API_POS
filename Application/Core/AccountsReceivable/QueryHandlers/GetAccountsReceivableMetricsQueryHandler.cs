using Application.Abstractions.AccountsReceivable;
using Application.Abstractions.Billing;
using Application.Core.AccountsReceivable.Queries;
using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.QueryHandlers;

/// <summary>
/// Handler para calcular métricas de Cuentas por Cobrar
/// </summary>
public class GetAccountsReceivableMetricsQueryHandler : IRequestHandler<GetAccountsReceivableMetricsQuery, AccountsReceivableMetricsDto>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;

    public GetAccountsReceivableMetricsQueryHandler(
        IInvoiceRepository invoiceRepository,
        IPaymentRepository paymentRepository)
    {
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<AccountsReceivableMetricsDto> Handle(GetAccountsReceivableMetricsQuery request, CancellationToken cancellationToken)
    {
        // Obtener todas las facturas (pendientes y pagadas)
        var (allInvoices, _) = await _invoiceRepository.GetPPDPagedAsync(
            pageNumber: 1,
            pageSize: int.MaxValue,
            companyId: request.CompanyId);

        var pendingInvoices = allInvoices.Where(i => i.PaymentStatus == "Pending").ToList();
        var overdueInvoices = pendingInvoices.Where(i => (i.DaysOverdue ?? 0) > 0).ToList();

        // 1. DSO (Days Sales Outstanding)
        // DSO = (Cuentas por Cobrar / Ventas diarias promedio)
        var totalReceivable = pendingInvoices.Sum(i => i.BalanceAmount ?? 0);
        var totalSales = allInvoices.Sum(i => i.Total);
        var daysInPeriod = 90; // últimos 90 días
        var averageDailySales = totalSales / daysInPeriod;
        var dso = averageDailySales > 0 ? (int)(totalReceivable / averageDailySales) : 0;

        // 2. Tasa de Morosidad
        // % de facturas vencidas sobre total
        var delinquencyRate = pendingInvoices.Any()
            ? (decimal)overdueInvoices.Count / pendingInvoices.Count * 100
            : 0;

        // 3. Efectividad de Cobranza
        // % de facturas cobradas a tiempo
        var paidOnTime = allInvoices.Count(i => i.Status == "Paid" && i.LastPaymentDate <= i.DueDate);
        var totalPaid = allInvoices.Count(i => i.Status == "Paid");
        var collectionEffectiveness = totalPaid > 0
            ? (decimal)paidOnTime / totalPaid * 100
            : 0;

        // 4. Edad Promedio de Cartera
        var today = DateTime.Today;
        var averagePortfolioAge = pendingInvoices.Any()
            ? (int)pendingInvoices.Average(i => (today - i.InvoiceDate).Days)
            : 0;

        // 5. Tasa de Recuperación
        // % del monto total recuperado vs monto original
        var totalPaidAmount = allInvoices.Where(i => i.Status == "Paid").Sum(i => i.Total);
        var totalOriginalAmount = allInvoices.Sum(i => i.Total);
        var recoveryRate = totalOriginalAmount > 0
            ? totalPaidAmount / totalOriginalAmount * 100
            : 0;

        return new AccountsReceivableMetricsDto
        {
            DSO = dso,
            DelinquencyRate = delinquencyRate,
            CollectionEffectiveness = collectionEffectiveness,
            AveragePortfolioAge = averagePortfolioAge,
            RecoveryRate = recoveryRate,
            CalculatedAt = DateTime.UtcNow
        };
    }
}
