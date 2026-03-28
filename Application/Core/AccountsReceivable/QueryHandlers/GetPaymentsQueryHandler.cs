using Application.Abstractions.AccountsReceivable;
using Application.Core.AccountsReceivable.Queries;
using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.QueryHandlers;

/// <summary>
/// Handler para obtener pagos paginados con filtros
/// </summary>
public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, PaymentPagedResultDto>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentsQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentPagedResultDto> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        // Obtener pagos paginados
        var (payments, totalRecords) = await _paymentRepository.GetPagedAsync(
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            customerId: request.CustomerId,
            companyId: request.CompanyId,
            status: request.Status,
            fromDate: request.FromDate,
            toDate: request.ToDate,
            hasComplement: request.HasComplement,
            searchTerm: request.SearchTerm
        );

        // Mapear a DTOs
        var data = payments.Select(p => new PaymentDto
        {
            Id = p.Id,
            PaymentNumber = p.PaymentNumber,
            CustomerId = p.CustomerId,
            CustomerName = p.CustomerName,
            CompanyId = p.CompanyId,
            BatchId = p.BatchId,
            BatchNumber = p.Batch?.BatchNumber,
            IsBatchPayment = p.BatchId.HasValue,
            PaymentDate = p.PaymentDate,
            TotalAmount = p.TotalAmount,
            Currency = p.Currency,
            PaymentFormSAT = p.PaymentFormSAT,
            Reference = p.Reference,
            BankDestination = p.BankDestination,
            AccountDestination = p.BankAccountDestination,
            
            // Datos del emisor
            EmisorRfc = p.EmisorRfc,
            EmisorNombre = p.EmisorNombre,
            EmisorRegimenFiscal = p.EmisorRegimenFiscal,
            LugarExpedicion = p.LugarExpedicion,
            
            // Datos del receptor
            ReceptorRfc = p.ReceptorRfc,
            ReceptorNombre = p.ReceptorNombre,
            ReceptorDomicilioFiscal = p.ReceptorDomicilioFiscal,
            ReceptorRegimenFiscal = p.ReceptorRegimenFiscal,
            ReceptorUsoCfdi = p.ReceptorUsoCfdi,
            
            // Estado
            Status = p.Status,
            AppliedToInvoices = p.PaymentApplications?.Count ?? 0,
            ComplementsGenerated = p.Uuid != null ? 1 : 0,
            ComplementsWithError = !string.IsNullOrEmpty(p.ComplementError) ? 1 : 0,
            
            // Datos del complemento
            ComplementSerie = p.ComplementSerie,
            ComplementFolio = p.ComplementFolio,
            Uuid = p.Uuid,
            TimbradoAt = p.TimbradoAt,
            XmlCfdi = p.XmlCfdi,
            CadenaOriginalSat = p.CadenaOriginalSat,
            SelloCfdi = p.SelloCfdi,
            SelloSat = p.SelloSat,
            NoCertificadoCfdi = p.NoCertificadoCfdi,
            NoCertificadoSat = p.NoCertificadoSat,
            QrCode = p.QrCode,
            XmlPath = p.XmlPath,
            PdfPath = p.PdfPath,
            EmailSent = p.EmailSent,
            ComplementError = p.ComplementError,
            
            // Metadatos
            Notes = p.Notes,
            CreatedAt = p.CreatedAt,
            AppliedAt = p.AppliedAt,
            CompletedAt = p.CompletedAt,
            
            // Aplicaciones (sin incluir para listado, solo en detalle)
            Applications = new List<PaymentApplicationDto>()
        }).ToList();

        // Calcular resumen (usando todos los pagos sin paginar)
        var (allPayments, _) = await _paymentRepository.GetPagedAsync(
            pageNumber: 1,
            pageSize: int.MaxValue,
            customerId: request.CustomerId,
            companyId: request.CompanyId
        );

        var summary = new PaymentSummaryDto
        {
            TotalPayments = allPayments.Count,
            AppliedPayments = allPayments.Count(p => p.Status == "Applied"),
            PendingPayments = allPayments.Count(p => p.Status == "Pending"),
            CancelledPayments = allPayments.Count(p => p.Status == "Cancelled"),
            WithComplement = allPayments.Count(p => p.Uuid != null),
            WithoutComplement = allPayments.Count(p => p.Uuid == null),
            
            TotalAmount = allPayments.Sum(p => p.TotalAmount),
            AverageAmount = allPayments.Any() ? allPayments.Average(p => p.TotalAmount) : 0
        };

        return new PaymentPagedResultDto
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
