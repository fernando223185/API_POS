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
            CustomerName = payment.CustomerName,
            CompanyId = payment.CompanyId,
            BatchId = payment.BatchId,
            BatchNumber = payment.Batch?.BatchNumber,
            IsBatchPayment = payment.IsBatchPayment,
            PaymentDate = payment.PaymentDate,
            TotalAmount = payment.TotalAmount,
            Currency = payment.Currency,
            PaymentFormSAT = payment.PaymentFormSAT,
            Reference = payment.Reference,
            BankDestination = payment.BankDestination,
            AccountDestination = payment.BankAccountDestination,
            
            // Datos del emisor (snapshot al crear pago)
            EmisorRfc = payment.EmisorRfc,
            EmisorNombre = payment.EmisorNombre,
            EmisorRegimenFiscal = payment.EmisorRegimenFiscal,
            LugarExpedicion = payment.LugarExpedicion,
            
            // Datos del receptor (snapshot al crear pago)
            ReceptorRfc = payment.ReceptorRfc,
            ReceptorNombre = payment.ReceptorNombre,
            ReceptorDomicilioFiscal = payment.ReceptorDomicilioFiscal,
            ReceptorRegimenFiscal = payment.ReceptorRegimenFiscal,
            ReceptorUsoCfdi = payment.ReceptorUsoCfdi,
            
            Status = payment.Status,
            AppliedToInvoices = payment.AppliedToInvoices,
            ComplementsGenerated = payment.ComplementsGenerated,
            ComplementsWithError = payment.ComplementsWithError,
            
            // Datos del complemento timbrado
            ComplementSerie = payment.ComplementSerie,
            ComplementFolio = payment.ComplementFolio,
            Uuid = payment.Uuid,
            TimbradoAt = payment.TimbradoAt,
            XmlCfdi = payment.XmlCfdi,
            CadenaOriginalSat = payment.CadenaOriginalSat,
            SelloCfdi = payment.SelloCfdi,
            SelloSat = payment.SelloSat,
            NoCertificadoCfdi = payment.NoCertificadoCfdi,
            NoCertificadoSat = payment.NoCertificadoSat,
            QrCode = payment.QrCode,
            XmlPath = payment.XmlPath,
            PdfPath = payment.PdfPath,
            EmailSent = payment.EmailSent,
            ComplementError = payment.ComplementError,
            
            Notes = payment.Notes,
            CreatedAt = payment.CreatedAt,
            AppliedAt = payment.AppliedAt,
            CompletedAt = payment.CompletedAt,
            
            Applications = payment.PaymentApplications?.Select(app => new PaymentApplicationDto
            {
                Id = app.Id,
                PaymentId = app.PaymentId,
                InvoiceId = app.InvoiceId,
                FolioUUID = app.FolioUUID,
                SerieAndFolio = app.SerieAndFolio,
                PaymentType = app.PaymentType,
                PartialityNumber = app.PartialityNumber,
                PreviousBalance = app.PreviousBalance,
                AmountApplied = app.AmountApplied,
                NewBalance = app.NewBalance,
                
                // Datos de moneda e impuestos para el timbrado
                DocumentCurrency = app.DocumentCurrency,
                DocumentExchangeRate = app.DocumentExchangeRate,
                TaxObject = app.TaxObject,
                TaxBase = app.TaxBase,
                TaxCode = app.TaxCode,
                TaxFactorType = app.TaxFactorType,
                TaxRate = app.TaxRate,
                TaxAmount = app.TaxAmount,
                
                CreatedAt = app.CreatedAt,
                AppliedAt = app.AppliedAt
            }).ToList() ?? new List<PaymentApplicationDto>()
        };
    }
}
