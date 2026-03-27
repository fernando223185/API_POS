using Application.Abstractions.AccountsReceivable;
using Application.Core.AccountsReceivable.Queries;
using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.QueryHandlers;

/// <summary>
/// Handler para obtener un pago por ID con todas sus aplicaciones
/// </summary>
public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDto?>
{
    private readonly IPaymentRepository _repository;

    public GetPaymentByIdQueryHandler(IPaymentRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaymentDto?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _repository.GetByIdAsync(request.Id);
        
        if (payment == null)
            return null;

        return new PaymentDto
        {
            Id = payment.Id,
            PaymentNumber = payment.PaymentNumber,
            CustomerId = payment.CustomerId,
            CustomerName = "", // Se debe cargar desde Customer si es necesario
            CompanyId = payment.CompanyId,
            BatchId = payment.BatchId,
            BatchNumber = payment.Batch?.BatchNumber,
            IsBatchPayment = payment.IsBatchPayment,
            PaymentDate = payment.PaymentDate,
            TotalAmount = payment.TotalAmount,
            Currency = payment.Currency,
            PaymentMethodSAT = payment.PaymentMethodSAT,
            Reference = payment.Reference,
            Status = payment.Status,
            AppliedToInvoices = payment.AppliedToInvoices,
            ComplementsGenerated = payment.ComplementsGenerated,
            ComplementsWithError = payment.ComplementsWithError,
            Notes = payment.Notes,
            CreatedAt = payment.CreatedAt,
            AppliedAt = payment.AppliedAt,
            CompletedAt = payment.CompletedAt,
            Applications = payment.PaymentApplications?.Select(app => new PaymentApplicationDto
            {
                Id = app.Id,
                PaymentId = app.PaymentId,
                InvoicePPDId = app.InvoicePPDId,
                FolioUUID = app.InvoicePPD?.FolioUUID ?? "",
                SerieAndFolio = app.InvoicePPD?.SerieAndFolio ?? "",
                PaymentType = app.PaymentType,
                PartialityNumber = app.PartialityNumber,
                PreviousBalance = app.PreviousBalance,
                AmountApplied = app.AmountApplied,
                NewBalance = app.NewBalance,
                ComplementUUID = app.ComplementUUID,
                ComplementSerieAndFolio = app.ComplementSerieAndFolio,
                ComplementStatus = app.ComplementStatus,
                ComplementError = app.ComplementError,
                XmlPath = app.XmlPath,
                PdfPath = app.PdfPath,
                EmailSent = app.EmailSent,
                CreatedAt = app.CreatedAt,
                GeneratedAt = app.GeneratedAt
            }).ToList() ?? new List<PaymentApplicationDto>()
        };
    }
}
