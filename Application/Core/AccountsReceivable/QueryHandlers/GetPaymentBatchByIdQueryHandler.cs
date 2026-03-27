using Application.Abstractions.AccountsReceivable;
using Application.Core.AccountsReceivable.Queries;
using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.QueryHandlers;

/// <summary>
/// Handler para obtener un lote de pagos por ID
/// </summary>
public class GetPaymentBatchByIdQueryHandler : IRequestHandler<GetPaymentBatchByIdQuery, PaymentBatchDto?>
{
    private readonly IPaymentBatchRepository _repository;

    public GetPaymentBatchByIdQueryHandler(IPaymentBatchRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaymentBatchDto?> Handle(GetPaymentBatchByIdQuery request, CancellationToken cancellationToken)
    {
        var batch = await _repository.GetByIdAsync(request.Id);
        
        if (batch == null)
            return null;

        return new PaymentBatchDto
        {
            Id = batch.Id,
            BatchNumber = batch.BatchNumber,
            PaymentDate = batch.PaymentDate,
            TotalPayments = batch.TotalPayments,
            TotalInvoices = batch.TotalInvoices,
            TotalAmount = batch.TotalAmount,
            Status = batch.Status,
            ProcessingProgress = batch.ProcessingProgress,
            ComplementsGenerated = batch.ComplementsGenerated,
            ComplementsWithError = batch.ComplementsWithError,
            Description = batch.Description,
            CreatedAt = batch.CreatedAt,
            CompletedAt = batch.CompletedAt,
            Payments = batch.Payments?.Select(p => new PaymentDto
            {
                Id = p.Id,
                PaymentNumber = p.PaymentNumber,
                CustomerId = p.CustomerId,
                CustomerName = "",
                CompanyId = p.CompanyId,
                BatchId = p.BatchId,
                IsBatchPayment = p.IsBatchPayment,
                PaymentDate = p.PaymentDate,
                TotalAmount = p.TotalAmount,
                Currency = p.Currency,
                PaymentFormSAT = p.PaymentFormSAT,
                Status = p.Status,
                AppliedToInvoices = p.AppliedToInvoices,
                ComplementsGenerated = p.ComplementsGenerated,
                ComplementsWithError = p.ComplementsWithError,
                CreatedAt = p.CreatedAt,
                Applications = new List<PaymentApplicationDto>()
            }).ToList() ?? new List<PaymentDto>()
        };
    }
}
