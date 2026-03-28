using Application.Abstractions.AccountsReceivable;
using Application.Core.AccountsReceivable.Commands;
using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.CommandHandlers;

/// <summary>
/// Handler para actualizar datos de un pago antes de timbrar
/// </summary>
public class UpdatePaymentCommandHandler : IRequestHandler<UpdatePaymentCommand, PaymentDto>
{
    private readonly IPaymentRepository _paymentRepository;

    public UpdatePaymentCommandHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentDto> Handle(UpdatePaymentCommand command, CancellationToken cancellationToken)
    {
        // 1. Cargar pago
        var payment = await _paymentRepository.GetByIdAsync(command.PaymentId)
            ?? throw new InvalidOperationException($"Pago {command.PaymentId} no encontrado.");

        // 2. Validar que NO esté timbrado
        if (!string.IsNullOrEmpty(payment.Uuid))
        {
            throw new InvalidOperationException(
                $"No se puede editar el pago {payment.PaymentNumber} porque ya está timbrado (UUID: {payment.Uuid}).");
        }

        // 3. Actualizar datos del emisor
        if (!string.IsNullOrEmpty(command.EmisorRfc))
            payment.EmisorRfc = command.EmisorRfc;
        
        if (!string.IsNullOrEmpty(command.EmisorNombre))
            payment.EmisorNombre = command.EmisorNombre;
        
        if (!string.IsNullOrEmpty(command.EmisorRegimenFiscal))
            payment.EmisorRegimenFiscal = command.EmisorRegimenFiscal;
        
        if (!string.IsNullOrEmpty(command.LugarExpedicion))
            payment.LugarExpedicion = command.LugarExpedicion;

        // 4. Actualizar datos del receptor
        if (!string.IsNullOrEmpty(command.ReceptorRfc))
            payment.ReceptorRfc = command.ReceptorRfc;
        
        if (!string.IsNullOrEmpty(command.ReceptorNombre))
            payment.ReceptorNombre = command.ReceptorNombre;
        
        if (!string.IsNullOrEmpty(command.ReceptorDomicilioFiscal))
            payment.ReceptorDomicilioFiscal = command.ReceptorDomicilioFiscal;
        
        if (!string.IsNullOrEmpty(command.ReceptorRegimenFiscal))
            payment.ReceptorRegimenFiscal = command.ReceptorRegimenFiscal;
        
        if (!string.IsNullOrEmpty(command.ReceptorUsoCfdi))
            payment.ReceptorUsoCfdi = command.ReceptorUsoCfdi;

        // 5. Actualizar datos bancarios
        payment.BankDestination = command.BankDestination;
        payment.BankAccountDestination = command.AccountDestination;
        payment.Reference = command.Reference;
        payment.Notes = command.Notes;

        // 6. Guardar cambios
        var updated = await _paymentRepository.UpdateAsync(payment);

        // 7. Retornar DTO
        return new PaymentDto
        {
            Id = updated.Id,
            PaymentNumber = updated.PaymentNumber,
            CustomerId = updated.CustomerId,
            CustomerName = updated.CustomerName,
            CompanyId = updated.CompanyId,
            BatchId = updated.BatchId,
            BatchNumber = updated.Batch?.BatchNumber,
            IsBatchPayment = updated.IsBatchPayment,
            PaymentDate = updated.PaymentDate,
            TotalAmount = updated.TotalAmount,
            Currency = updated.Currency,
            PaymentFormSAT = updated.PaymentFormSAT,
            Reference = updated.Reference,
            
            EmisorRfc = updated.EmisorRfc,
            EmisorNombre = updated.EmisorNombre,
            EmisorRegimenFiscal = updated.EmisorRegimenFiscal,
            LugarExpedicion = updated.LugarExpedicion,
            
            ReceptorRfc = updated.ReceptorRfc,
            ReceptorNombre = updated.ReceptorNombre,
            ReceptorDomicilioFiscal = updated.ReceptorDomicilioFiscal,
            ReceptorRegimenFiscal = updated.ReceptorRegimenFiscal,
            ReceptorUsoCfdi = updated.ReceptorUsoCfdi,
            
            Status = updated.Status,
            AppliedToInvoices = updated.AppliedToInvoices,
            ComplementsGenerated = updated.ComplementsGenerated,
            ComplementsWithError = updated.ComplementsWithError,
            
            ComplementSerie = updated.ComplementSerie,
            ComplementFolio = updated.ComplementFolio,
            Uuid = updated.Uuid,
            TimbradoAt = updated.TimbradoAt,
            XmlCfdi = updated.XmlCfdi,
            CadenaOriginalSat = updated.CadenaOriginalSat,
            SelloCfdi = updated.SelloCfdi,
            SelloSat = updated.SelloSat,
            NoCertificadoCfdi = updated.NoCertificadoCfdi,
            NoCertificadoSat = updated.NoCertificadoSat,
            QrCode = updated.QrCode,
            XmlPath = updated.XmlPath,
            PdfPath = updated.PdfPath,
            EmailSent = updated.EmailSent,
            ComplementError = updated.ComplementError,
            
            Notes = updated.Notes,
            CreatedAt = updated.CreatedAt,
            AppliedAt = updated.AppliedAt,
            CompletedAt = updated.CompletedAt,
            
            Applications = updated.PaymentApplications?.Select(app => new PaymentApplicationDto
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
