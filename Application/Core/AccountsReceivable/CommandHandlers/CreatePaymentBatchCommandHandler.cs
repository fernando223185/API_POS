using Application.Abstractions.AccountsReceivable;
using Application.Core.AccountsReceivable.Commands;
using Application.DTOs.AccountsReceivable;
using Domain.Entities;
using MediatR;

namespace Application.Core.AccountsReceivable.CommandHandlers;

/// <summary>
/// Handler para crear un lote de pagos masivo
/// </summary>
public class CreatePaymentBatchCommandHandler : IRequestHandler<CreatePaymentBatchCommand, PaymentBatchDto>
{
    private readonly IPaymentBatchRepository _batchRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentApplicationRepository _applicationRepository;
    private readonly IInvoicePPDRepository _invoicePPDRepository;

    public CreatePaymentBatchCommandHandler(
        IPaymentBatchRepository batchRepository,
        IPaymentRepository paymentRepository,
        IPaymentApplicationRepository applicationRepository,
        IInvoicePPDRepository invoicePPDRepository)
    {
        _batchRepository = batchRepository;
        _paymentRepository = paymentRepository;
        _applicationRepository = applicationRepository;
        _invoicePPDRepository = invoicePPDRepository;
    }

    public async Task<PaymentBatchDto> Handle(CreatePaymentBatchCommand command, CancellationToken cancellationToken)
    {
        // 1. Determinar número de lote: personalizado o autogenerado
        string batchNumber;
        if (!string.IsNullOrWhiteSpace(command.CustomBatchNumber))
        {
            // Validar que el folio personalizado no exista ya
            var exists = await _batchRepository.ExistsByBatchNumberAsync(command.CustomBatchNumber);
            if (exists)
                throw new InvalidOperationException($"El folio de lote '{command.CustomBatchNumber}' ya existe. Por favor usa uno diferente.");

            batchNumber = command.CustomBatchNumber.Trim().ToUpper();
        }
        else
        {
            batchNumber = await _batchRepository.GenerateBatchNumberAsync(command.BatchPrefix ?? "LOTE");
        }

        // 2. Crear entidad de lote
        var batch = new PaymentBatch
        {
            BatchNumber = batchNumber,
            CompanyId = command.CompanyId,
            PaymentDate = command.PaymentDate,
            DefaultPaymentMethodSAT = command.DefaultPaymentMethodSAT,
            DefaultBankDestination = command.DefaultBankDestination,
            DefaultAccountDestination = command.DefaultAccountDestination,
            Description = command.Description,
            Notes = command.Notes,
            Status = "Draft",
            CreatedBy = command.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        // 3. Procesar cada pago del lote
        decimal totalAmount = 0;
        int totalInvoices = 0;
        var payments = new List<Payment>();

        foreach (var paymentItem in command.Payments)
        {
            // Generar número de pago
            var paymentNumber = await _paymentRepository.GeneratePaymentNumberAsync();

            var payment = new Payment
            {
                PaymentNumber = paymentNumber,
                CustomerId = paymentItem.CustomerId,
                CompanyId = command.CompanyId,
                IsBatchPayment = true,
                PaymentDate = paymentItem.PaymentDate ?? command.PaymentDate,
                // Método/forma de pago: específico del pago o default del lote
                PaymentMethodSAT = paymentItem.PaymentMethodSAT ?? command.DefaultPaymentMethodSAT ?? "03",
                PaymentFormSAT = paymentItem.PaymentFormSAT ?? command.DefaultPaymentFormSAT,
                Currency = "MXN",
                ExchangeRate = 1.0M,
                BankOrigin = paymentItem.BankOrigin,
                // Banco/cuenta destino: específico del pago o default del lote
                BankDestination = paymentItem.BankDestination ?? command.DefaultBankDestination,
                BankAccountDestination = paymentItem.AccountDestination ?? command.DefaultAccountDestination,
                Reference = paymentItem.Reference,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow
            };

            // Procesar facturas del pago con montos específicos
            decimal paymentTotal = 0;
            var applications = new List<PaymentApplication>();

            foreach (var invoiceItem in paymentItem.Invoices)
            {
                var invoice = await _invoicePPDRepository.GetByIdAsync(invoiceItem.InvoicePPDId);
                if (invoice == null) continue;

                // Usar el monto especificado, o el saldo total si no se proporcionó
                var amountToPay = (invoiceItem.AmountToPay.HasValue && invoiceItem.AmountToPay.Value > 0)
                    ? Math.Min(invoiceItem.AmountToPay.Value, invoice.BalanceAmount) // No pagar más del saldo
                    : invoice.BalanceAmount;

                paymentTotal += amountToPay;

                var application = new PaymentApplication
                {
                    InvoicePPDId = invoiceItem.InvoicePPDId,
                    PaymentType = amountToPay >= invoice.BalanceAmount ? "Total" : "Parcial",
                    PartialityNumber = invoice.NextPartialityNumber,
                    PreviousBalance = invoice.BalanceAmount,
                    AmountApplied = amountToPay,
                    NewBalance = invoice.BalanceAmount - amountToPay,
                    ComplementStatus = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                applications.Add(application);
                totalInvoices++;
            }

            payment.TotalAmount = paymentTotal;
            payment.AppliedToInvoices = applications.Count;
            payment.PaymentApplications = applications;
            
            payments.Add(payment);
            totalAmount += paymentTotal;
        }

        batch.TotalPayments = payments.Count;
        batch.TotalInvoices = totalInvoices;
        batch.TotalAmount = totalAmount;
        batch.Payments = payments;

        // 4. Guardar lote con todos sus pagos
        var savedBatch = await _batchRepository.CreateAsync(batch);

        // 5. Retornar DTO
        return new PaymentBatchDto
        {
            Id = savedBatch.Id,
            BatchNumber = savedBatch.BatchNumber,
            PaymentDate = savedBatch.PaymentDate,
            TotalPayments = savedBatch.TotalPayments,
            TotalInvoices = savedBatch.TotalInvoices,
            TotalAmount = savedBatch.TotalAmount,
            Status = savedBatch.Status,
            ProcessingProgress = savedBatch.ProcessingProgress,
            ComplementsGenerated = savedBatch.ComplementsGenerated,
            ComplementsWithError = savedBatch.ComplementsWithError,
            Description = savedBatch.Description,
            CreatedAt = savedBatch.CreatedAt,
            CompletedAt = savedBatch.CompletedAt,
            Payments = new List<PaymentDto>()
        };
    }
}
