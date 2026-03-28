using Application.Abstractions.Billing;
using Application.Abstractions.AccountsReceivable;
using Application.Core.AccountsReceivable.Queries;
using Application.DTOs.AccountsReceivable;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Core.AccountsReceivable.QueryHandlers;

/// <summary>
/// Handler para obtener una factura PPD por ID con información completa de cobranza
/// </summary>
public class GetInvoicePPDByIdQueryHandler : IRequestHandler<GetInvoicePPDByIdQuery, InvoicePPDDetailDto?>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;

    public GetInvoicePPDByIdQueryHandler(
        IInvoiceRepository invoiceRepository,
        IPaymentRepository paymentRepository)
    {
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<InvoicePPDDetailDto?> Handle(GetInvoicePPDByIdQuery request, CancellationToken cancellationToken)
    {
        // Buscar factura por ID
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId);
        
        if (invoice == null)
            return null;

        // Verificar que sea PPD
        if (invoice.MetodoPago != "PPD")
            return null; // No es una factura PPD

        // Calcular saldo y estado
        var today = DateTime.UtcNow.Date;
        var balance = invoice.Total - invoice.PaidAmount;
        var daysOverdue = invoice.DueDate.HasValue && invoice.DueDate.Value < today 
            ? (today - invoice.DueDate.Value).Days 
            : (int?)null;

        // Determinar estado de pago
        string paymentStatus;
        if (balance <= 0)
            paymentStatus = "Paid";
        else if (invoice.PaidAmount > 0)
            paymentStatus = daysOverdue > 0 ? "PartiallyPaidOverdue" : "PartiallyPaid";
        else if (daysOverdue > 0)
            paymentStatus = "Overdue";
        else
            paymentStatus = "Pending";

        // Obtener historial de pagos aplicados
        var paymentsApplied = new List<PaymentApplicationSummaryDto>();
        
        if (invoice.PaymentApplications != null && invoice.PaymentApplications.Any())
        {
            foreach (var application in invoice.PaymentApplications.OrderBy(p => p.Payment != null ? p.Payment.PaymentDate : DateTime.MaxValue))
            {
                var payment = application.Payment;
                if (payment != null)
                {
                    paymentsApplied.Add(new PaymentApplicationSummaryDto
                    {
                        PaymentId = payment.Id,
                        PaymentNumber = payment.PaymentNumber,
                        PaymentDate = payment.PaymentDate,
                        AmountApplied = application.AmountApplied,
                        PartialityNumber = application.PartialityNumber,
                        PaymentMethodSAT = payment.PaymentFormSAT,
                        Reference = payment.Reference,
                        Uuid = payment.Uuid, // UUID del complemento
                        Status = payment.Status
                    });
                }
            }
        }

        // Mapear a DTO
        var result = new InvoicePPDDetailDto
        {
            Id = invoice.Id,
            CustomerId = invoice.CustomerId,
            CustomerName = invoice.Customer?.CompanyName ?? invoice.ReceptorNombre,
            CustomerRFC = invoice.Customer?.TaxId ?? invoice.ReceptorRfc,
            Serie = invoice.Serie ?? string.Empty,
            Folio = invoice.Folio ?? string.Empty,
            Uuid = invoice.Uuid ?? string.Empty,
            
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            
            Moneda = invoice.Moneda,
            TipoCambio = invoice.TipoCambio,
            Subtotal = invoice.SubTotal,
            ImpuestosTrasladadosTotal = invoice.TaxAmount,
            ImpuestosRetenidosTotal = 0, // No disponible en modelo actual
            Total = invoice.Total,
            PaidAmount = invoice.PaidAmount,
            BalanceAmount = balance,
            
            NextPartialityNumber = invoice.NextPartialityNumber ?? 1,
            TotalPartialities = invoice.TotalPartialities,
            PaymentStatus = paymentStatus,
            DaysOverdue = daysOverdue,
            LastPaymentDate = invoice.LastPaymentDate,
            
            TipoDeComprobante = invoice.TipoDeComprobante,
            MetodoPago = invoice.MetodoPago,
            FormaPago = invoice.FormaPago ?? string.Empty,
            UsoCFDI = invoice.ReceptorUsoCfdi ?? string.Empty,
            
            EmisorRfc = invoice.EmisorRfc,
            EmisorNombre = invoice.EmisorNombre,
            ReceptorRfc = invoice.ReceptorRfc,
            ReceptorNombre = invoice.ReceptorNombre,
            
            Notes = invoice.Notes,
            CreatedAt = invoice.CreatedAt,
            
            PaymentsApplied = paymentsApplied
        };

        return result;
    }
}
