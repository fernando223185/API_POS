using Application.Abstractions.Billing;
using Application.Core.AccountsReceivable.Queries;
using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.QueryHandlers;

/// <summary>
/// Handler para obtener facturas PPD con información de cobranza
/// </summary>
public class GetInvoicesPPDQueryHandler : IRequestHandler<GetInvoicesPPDQuery, InvoicePPDPagedResultDto>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoicesPPDQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<InvoicePPDPagedResultDto> Handle(GetInvoicesPPDQuery request, CancellationToken cancellationToken)
    {
        // Obtener facturas PPD paginadas usando el repositorio
        var (invoices, totalRecords) = await _invoiceRepository.GetPPDPagedAsync(
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            customerId: request.CustomerId,
            companyId: request.CompanyId,
            paymentStatus: request.PaymentStatus,
            fromDate: request.FromDate,
            toDate: request.ToDate,
            minDaysOverdue: request.MinDaysOverdue,
            searchTerm: request.SearchTerm
        );

        // Mapear a DTOs
        var today = DateTime.UtcNow.Date;
        var data = invoices.Select(i => new InvoicePPDDto
        {
            Id = i.Id,
            CustomerId = i.CustomerId,
            CustomerName = i.Customer?.CompanyName ?? i.ReceptorNombre,
            CustomerRFC = i.Customer?.TaxId ?? i.ReceptorRfc,
            Serie = i.Serie,
            Folio = i.Folio,
            Uuid = i.Uuid ?? string.Empty,
            InvoiceDate = i.InvoiceDate,
            DueDate = i.DueDate,
            Moneda = i.Moneda,
            TipoCambio = i.TipoCambio,
            Total = i.Total,
            PaidAmount = i.PaidAmount,
            BalanceAmount = i.BalanceAmount ?? (i.Total - i.PaidAmount),
            NextPartialityNumber = i.NextPartialityNumber,
            TotalPartialities = i.TotalPartialities,
            PaymentStatus = i.PaymentStatus,
            DaysOverdue = i.DaysOverdue,
            LastPaymentDate = i.LastPaymentDate,
            Notes = i.Notes,
            CreatedAt = i.CreatedAt
        }).ToList();

        // Calcular resumen (usando todas las facturas PPD sin paginar)
        var (allInvoices, _) = await _invoiceRepository.GetPPDPagedAsync(
            pageNumber: 1,
            pageSize: int.MaxValue,
            customerId: request.CustomerId,
            companyId: request.CompanyId
        );

        var summary = new InvoicePPDSummaryDto
        {
            TotalInvoices = allInvoices.Count,
            PendingInvoices = allInvoices.Count(i => i.PaymentStatus == "Pending"),
            PartiallyPaidInvoices = allInvoices.Count(i => i.PaymentStatus == "PartiallyPaid"),
            PaidInvoices = allInvoices.Count(i => i.PaymentStatus == "Paid"),
            OverdueInvoices = allInvoices.Count(i => i.PaymentStatus == "Overdue" || (i.DueDate.HasValue && i.DueDate.Value < today && (i.BalanceAmount ?? 0) > 0)),
            
            TotalAmount = allInvoices.Sum(i => i.Total),
            TotalPaid = allInvoices.Sum(i => i.PaidAmount),
            TotalBalance = allInvoices.Sum(i => i.BalanceAmount ?? (i.Total - i.PaidAmount)),
            TotalOverdueAmount = allInvoices
                .Where(i => i.DueDate.HasValue && i.DueDate.Value < today && (i.BalanceAmount ?? 0) > 0)
                .Sum(i => i.BalanceAmount ?? (i.Total - i.PaidAmount)),
            
            AverageDaysOverdue = allInvoices.Any(i => i.DaysOverdue.HasValue && i.DaysOverdue > 0)
                ? (int)allInvoices.Where(i => i.DaysOverdue.HasValue && i.DaysOverdue > 0).Average(i => i.DaysOverdue ?? 0)
                : 0
        };

        return new InvoicePPDPagedResultDto
        {
            Data = data,
            Page = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize),
            Summary = summary
        };
    }
}
