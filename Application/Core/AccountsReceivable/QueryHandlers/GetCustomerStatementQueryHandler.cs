using Application.Abstractions.AccountsReceivable;
using Application.Core.AccountsReceivable.Queries;
using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.QueryHandlers;

/// <summary>
/// Handler para obtener el estado de cuenta completo de un cliente
/// </summary>
public class GetCustomerStatementQueryHandler : IRequestHandler<GetCustomerStatementQuery, CustomerStatementDto?>
{
    private readonly IInvoicePPDRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ICustomerCreditPolicyRepository _policyRepository;
    private readonly ICustomerCreditHistoryRepository _historyRepository;

    public GetCustomerStatementQueryHandler(
        IInvoicePPDRepository invoiceRepository,
        IPaymentRepository paymentRepository,
        ICustomerCreditPolicyRepository policyRepository,
        ICustomerCreditHistoryRepository historyRepository)
    {
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
        _policyRepository = policyRepository;
        _historyRepository = historyRepository;
    }

    public async Task<CustomerStatementDto?> Handle(GetCustomerStatementQuery request, CancellationToken cancellationToken)
    {
        // Obtener facturas pendientes del cliente
        var (invoices, _) = await _invoiceRepository.GetPagedAsync(
            pageNumber: 1,
            pageSize: int.MaxValue,
            customerId: request.CustomerId,
            companyId: request.CompanyId,
            status: "Pending");

        if (!invoices.Any())
            return null; // Cliente no encontrado o sin facturas

        var firstInvoice = invoices.First();
        var totalPending = invoices.Sum(i => i.BalanceAmount);
        var totalOverdue = invoices.Where(i => i.DaysOverdue > 0).Sum(i => i.BalanceAmount);

        // Obtener política de crédito
        var policy = await _policyRepository.GetByCustomerIdAsync(request.CustomerId);
        CustomerCreditPolicyDto? policyDto = null;

        if (policy != null)
        {
            var (pendingAmount, overdueAmount) = await _invoiceRepository.GetCustomerBalanceSummaryAsync(
                request.CustomerId, 
                request.CompanyId);

            policyDto = new CustomerCreditPolicyDto
            {
                Id = policy.Id,
                CustomerId = policy.CustomerId,
                CustomerName = firstInvoice.CustomerName,
                CompanyId = policy.CompanyId,
                CreditLimit = policy.CreditLimit,
                CreditDays = policy.CreditDays,
                OverdueGraceDays = policy.OverdueGraceDays,
                TotalPendingAmount = pendingAmount,
                TotalOverdueAmount = overdueAmount,
                AvailableCredit = policy.CreditLimit - pendingAmount,
                Status = policy.Status,
                BlockReason = policy.BlockReason,
                AutoBlockOnOverdue = policy.AutoBlockOnOverdue,
                Notes = policy.Notes,
                CreatedAt = policy.CreatedAt,
                UpdatedAt = policy.UpdatedAt
            };
        }

        // Obtener pagos recientes
        var recentPayments = await _paymentRepository.GetByCustomerAsync(request.CustomerId, limit: 10);

        // Obtener historial de crédito
        var history = new List<CustomerCreditHistoryDto>();
        if (request.IncludeHistory)
        {
            var historyRecords = await _historyRepository.GetByCustomerIdAsync(
                request.CustomerId, 
                limit: request.MaxHistoryRecords);

            history = historyRecords.Select(h => new CustomerCreditHistoryDto
            {
                Id = h.Id,
                CustomerId = h.CustomerId,
                EventType = h.EventType,
                EventDate = h.EventDate,
                Amount = h.Amount,
                RelatedEntity = h.RelatedEntity,
                Description = h.Description,
                PreviousValue = h.PreviousValue,
                NewValue = h.NewValue,
                CreatedAt = h.CreatedAt
            }).ToList();
        }

        return new CustomerStatementDto
        {
            CustomerId = request.CustomerId,
            CustomerName = firstInvoice.CustomerName,
            CustomerRFC = firstInvoice.CustomerRFC,
            CreditPolicy = policyDto,
            TotalPending = totalPending,
            TotalOverdue = totalOverdue,
            InvoicesPending = invoices.Count,
            Invoices = invoices.Select(i => new InvoicePPDDto
            {
                Id = i.Id,
                InvoiceId = i.InvoiceId,
                CustomerId = i.CustomerId,
                CustomerName = i.CustomerName,
                CustomerRFC = i.CustomerRFC,
                FolioUUID = i.FolioUUID,
                Serie = i.Serie,
                Folio = i.Folio,
                SerieAndFolio = i.SerieAndFolio,
                InvoiceDate = i.InvoiceDate,
                DueDate = i.DueDate,
                Currency = i.Currency,
                ExchangeRate = i.ExchangeRate,
                OriginalAmount = i.OriginalAmount,
                PaidAmount = i.PaidAmount,
                BalanceAmount = i.BalanceAmount,
                NextPartialityNumber = i.NextPartialityNumber,
                TotalPartialities = i.TotalPartialities,
                Status = i.Status,
                DaysOverdue = i.DaysOverdue,
                LastPaymentDate = i.LastPaymentDate,
                Notes = i.Notes,
                CreatedAt = i.CreatedAt
            }).ToList(),
            RecentPayments = recentPayments.Select(p => new PaymentDto
            {
                Id = p.Id,
                PaymentNumber = p.PaymentNumber,
                CustomerId = p.CustomerId,
                CompanyId = p.CompanyId,
                PaymentDate = p.PaymentDate,
                TotalAmount = p.TotalAmount,
                Currency = p.Currency,
                PaymentFormSAT = p.PaymentFormSAT,
                Status = p.Status,
                CreatedAt = p.CreatedAt
            }).ToList(),
            RecentHistory = history
        };
    }
}
