using Application.Abstractions.AccountsReceivable;
using Application.Abstractions.Billing;
using Application.Abstractions.Config;
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
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ICompanyRepository _companyRepository;

    public CreatePaymentBatchCommandHandler(
        IPaymentBatchRepository batchRepository,
        IPaymentRepository paymentRepository,
        IPaymentApplicationRepository applicationRepository,
        IInvoiceRepository invoiceRepository,
        ICompanyRepository companyRepository)
    {
        _batchRepository = batchRepository;
        _paymentRepository = paymentRepository;
        _applicationRepository = applicationRepository;
        _invoiceRepository = invoiceRepository;
        _companyRepository = companyRepository;
    }

    public async Task<PaymentBatchDto> Handle(CreatePaymentBatchCommand command, CancellationToken cancellationToken)
    {
        // 0. Obtener la empresa para usar su código en el folio del lote
        var company = await _companyRepository.GetByIdAsync(command.CompanyId)
            ?? throw new InvalidOperationException($"Empresa con ID {command.CompanyId} no encontrada.");

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
            // Generar folio automático: BTCP-[CompanyCode]-[DDMMYY]-[Consecutivo]
            // Ejemplo: BTCP-COMP001-260326-001
            batchNumber = await _batchRepository.GenerateBatchNumberAsync(
                company.Code,
                command.PaymentDate,
                "BTCP" // Prefijo fijo para lotes de complementos de pago
            );
        }

        // 2. Crear entidad de lote
        var batch = new PaymentBatch
        {
            BatchNumber = batchNumber,
            CompanyId = command.CompanyId,
            PaymentDate = command.PaymentDate,
            PaymentFormSAT = command.PaymentFormSAT,
            BankDestination = command.BankDestination,
            AccountDestination = command.AccountDestination,
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

            // Obtener la primera factura para extraer datos del receptor
            var firstInvoice = await _invoiceRepository.GetByIdAsync(paymentItem.Invoices.First().InvoicePPDId);
            if (firstInvoice == null)
                throw new InvalidOperationException($"Factura {paymentItem.Invoices.First().InvoicePPDId} no encontrada.");

            var payment = new Payment
            {
                PaymentNumber = paymentNumber,
                CustomerId = paymentItem.CustomerId,
                CustomerName = firstInvoice.ReceptorNombre,
                CompanyId = command.CompanyId,
                IsBatchPayment = true,
                PaymentDate = paymentItem.PaymentDate ?? command.PaymentDate,
                // Forma de pago del lote
                PaymentFormSAT = command.PaymentFormSAT ?? "03",
                Currency = "MXN",
                ExchangeRate = 1.0M,
                // Banco/cuenta destino del lote
                BankDestination = command.BankDestination,
                BankAccountDestination = command.AccountDestination,
                Reference = paymentItem.Reference,
                // Snapshot de datos del emisor (Company) al momento de creación
                EmisorRfc = company.TaxId,
                EmisorNombre = company.LegalName,
                EmisorRegimenFiscal = company.SatTaxRegime,
                LugarExpedicion = company.FiscalZipCode,
                // Snapshot de datos del receptor desde la factura PPD (datos con los que se timbró)
                ReceptorRfc = firstInvoice.ReceptorRfc,
                ReceptorNombre = firstInvoice.ReceptorNombre,
                ReceptorDomicilioFiscal = firstInvoice.ReceptorDomicilioFiscal,
                ReceptorRegimenFiscal = firstInvoice.ReceptorRegimenFiscal ?? string.Empty,
                ReceptorUsoCfdi = "CP01", // CP01 = Pagos (fijo para complementos de pago)
                Status = "Draft",
                CreatedAt = DateTime.UtcNow
            };

            // Procesar facturas del pago con montos específicos
            decimal paymentTotal = 0;
            var applications = new List<PaymentApplication>();

            foreach (var invoiceItem in paymentItem.Invoices)
            {
                var invoice = await _invoiceRepository.GetByIdAsync(invoiceItem.InvoicePPDId);
                if (invoice == null) continue;

                // Usar el monto especificado, o el saldo total si no se proporcionó
                var amountToPay = (invoiceItem.AmountToPay.HasValue && invoiceItem.AmountToPay.Value > 0)
                    ? Math.Min(invoiceItem.AmountToPay.Value, invoice.BalanceAmount ?? 0) // No pagar más del saldo
                    : (invoice.BalanceAmount ?? 0);

                paymentTotal += amountToPay;

                // Calcular impuestos proporcionalmente al monto pagado
                decimal proportionPaid = invoice.Total > 0 ? amountToPay / invoice.Total : 0;
                decimal taxBase = Math.Round(invoice.SubTotal * proportionPaid, 6);
                decimal taxAmount = Math.Round((invoice.Total - invoice.SubTotal) * proportionPaid, 6);

                var application = new PaymentApplication
                {
                    InvoiceId = invoiceItem.InvoicePPDId,
                    CustomerId = invoice.CustomerId ?? 0,
                    CustomerName = invoice.ReceptorNombre,
                    FolioUUID = invoice.Uuid,
                    SerieAndFolio = $"{invoice.Serie}-{invoice.Folio}",
                    OriginalInvoiceAmount = invoice.Total,
                    PaymentType = amountToPay >= (invoice.BalanceAmount ?? 0) ? "FullPayment" : "PartialPayment",
                    PartialityNumber = invoice.NextPartialityNumber ?? 1,
                    PreviousBalance = invoice.BalanceAmount ?? 0,
                    AmountApplied = amountToPay,
                    NewBalance = (invoice.BalanceAmount ?? 0) - amountToPay,
                    // Datos de moneda del documento relacionado
                    DocumentCurrency = invoice.Moneda,
                    DocumentExchangeRate = invoice.TipoCambio,
                    TaxObject = "02", // 02 = Sí objeto de impuestos
                    // Impuestos calculados proporcionalmente
                    TaxBase = taxBase,
                    TaxCode = "002", // 002 = IVA
                    TaxFactorType = "Tasa",
                    TaxRate = 0.160000M, // IVA 16%
                    TaxAmount = taxAmount,
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
