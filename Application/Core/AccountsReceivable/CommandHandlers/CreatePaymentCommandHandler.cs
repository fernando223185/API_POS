using Application.Abstractions.AccountsReceivable;
using Application.Abstractions.Config;
using Application.Core.AccountsReceivable.Commands;
using Application.DTOs.AccountsReceivable;
using Domain.Entities;
using MediatR;

namespace Application.Core.AccountsReceivable.CommandHandlers;

/// <summary>
/// Registra un pago y sus aplicaciones contra facturas PPD.
/// Después de esto el frontend puede llamar a generate-complements para timbrar.
/// </summary>
public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, PaymentDto>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentApplicationRepository _applicationRepository;
    private readonly IInvoicePPDRepository _invoicePPDRepository;
    private readonly ICompanyRepository _companyRepository;

    public CreatePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IPaymentApplicationRepository applicationRepository,
        IInvoicePPDRepository invoicePPDRepository,
        ICompanyRepository companyRepository)
    {
        _paymentRepository = paymentRepository;
        _applicationRepository = applicationRepository;
        _invoicePPDRepository = invoicePPDRepository;
        _companyRepository = companyRepository;
    }

    public async Task<PaymentDto> Handle(CreatePaymentCommand command, CancellationToken cancellationToken)
    {
        // 0. Obtener datos del emisor (Company)
        var company = await _companyRepository.GetByIdAsync(command.CompanyId)
            ?? throw new InvalidOperationException($"Empresa con ID {command.CompanyId} no encontrada.");

        // 1. Validar facturas y calcular total
        decimal totalAmount = 0;
        string customerName = string.Empty;
        InvoicePPD? firstInvoice = null;

        var invoicePPDs = new List<(PaymentInvoiceItem item, InvoicePPD ppd)>();

        foreach (var item in command.Invoices)
        {
            var ppd = await _invoicePPDRepository.GetByIdAsync(item.InvoicePPDId)
                ?? throw new InvalidOperationException(
                    $"Factura PPD {item.InvoicePPDId} no encontrada.");

            // Guardar la primera factura para obtener datos del receptor
            if (firstInvoice == null)
                firstInvoice = ppd;

            if (ppd.BalanceAmount <= 0)
                throw new InvalidOperationException(
                    $"La factura {ppd.SerieAndFolio} ya está liquidada.");

            if (item.AmountToPay <= 0 || item.AmountToPay > ppd.BalanceAmount)
                throw new InvalidOperationException(
                    $"Monto inválido para la factura {ppd.SerieAndFolio}. " +
                    $"Saldo disponible: {ppd.BalanceAmount:0.00}.");

            totalAmount += item.AmountToPay;
            customerName = ppd.CustomerName;
            invoicePPDs.Add((item, ppd));
        }

        // 2. Generar número de pago
        var paymentNumber = await _paymentRepository.GeneratePaymentNumberAsync("PAG");

        // 3. Validar que tenemos al menos una factura
        if (firstInvoice == null)
            throw new InvalidOperationException("Debe incluir al menos una factura para crear el pago.");

        // 4. Crear entidad Payment
        var payment = new Payment
        {
            PaymentNumber = paymentNumber,
            CustomerId = command.CustomerId,
            CustomerName = customerName,
            CompanyId = command.CompanyId,
            PaymentDate = command.PaymentDate,
            TotalAmount = totalAmount,
            Currency = command.Currency,
            ExchangeRate = command.ExchangeRate,
            PaymentFormSAT = command.PaymentFormSAT,
            BankOrigin = command.BankOrigin,
            BankAccountOrigin = command.BankAccountOrigin,
            BankDestination = command.BankDestination,
            BankAccountDestination = command.BankAccountDestination,
            Reference = command.Reference,
            Notes = command.Notes,
            // Snapshot de datos del emisor (Company) al momento de creación
            EmisorRfc = company.TaxId,
            EmisorNombre = company.LegalName,
            EmisorRegimenFiscal = company.SatTaxRegime,
            LugarExpedicion = company.FiscalZipCode,
            // Snapshot de datos del receptor desde la factura PPD (datos con los que se timbró)
            ReceptorRfc = firstInvoice.CustomerRFC,
            ReceptorNombre = firstInvoice.CustomerName,
            ReceptorDomicilioFiscal = firstInvoice.CustomerZipCode ?? string.Empty,
            ReceptorRegimenFiscal = firstInvoice.CustomerTaxRegime ?? string.Empty,
            ReceptorUsoCfdi = "CP01", // CP01 = Pagos (fijo para complementos de pago)
            Status = "Applied",
            AppliedToInvoices = command.Invoices.Count,
            ComplementsGenerated = 0,
            ComplementsWithError = 0,
            CreatedAt = DateTime.UtcNow,
            AppliedAt = DateTime.UtcNow,
            CreatedBy = command.CreatedBy
        };

        var savedPayment = await _paymentRepository.CreateAsync(payment);

        // 4. Crear una PaymentApplication por cada factura
        var savedApplications = new List<PaymentApplication>();

        foreach (var (item, ppd) in invoicePPDs)
        {
            var newBalance = Math.Round(ppd.BalanceAmount - item.AmountToPay, 2);

            // Calcular impuestos proporcionalmente al monto pagado
            // Ejemplo: Si la factura es de $1000 con $160 de IVA y se paga $500,
            // entonces la base es $431.03 y el IVA es $68.97
            decimal proportionPaid = ppd.OriginalAmount > 0 ? item.AmountToPay / ppd.OriginalAmount : 0;
            decimal taxBase = Math.Round(ppd.Subtotal * proportionPaid, 6);
            decimal taxAmount = Math.Round(ppd.TaxAmount * proportionPaid, 6);

            var application = new PaymentApplication
            {
                PaymentId = savedPayment.Id,
                InvoicePPDId = ppd.Id,
                CustomerId = ppd.CustomerId,
                CustomerName = ppd.CustomerName,
                FolioUUID = ppd.FolioUUID,
                SerieAndFolio = ppd.SerieAndFolio,
                OriginalInvoiceAmount = ppd.OriginalAmount,
                PaymentType = newBalance <= 0 ? "FullPayment" : "PartialPayment",
                PartialityNumber = ppd.NextPartialityNumber,
                PreviousBalance = ppd.BalanceAmount,
                AmountApplied = item.AmountToPay,
                NewBalance = newBalance,
                // Datos de moneda del documento relacionado
                DocumentCurrency = ppd.Currency,
                DocumentExchangeRate = ppd.ExchangeRate,
                TaxObject = "02", // 02 = Sí objeto de impuestos
                // Impuestos calculados proporcionalmente
                TaxBase = taxBase,
                TaxCode = "002", // 002 = IVA
                TaxFactorType = "Tasa",
                TaxRate = 0.160000M, // IVA 16%
                TaxAmount = taxAmount,
                ComplementStatus = "Pending",
                CreatedAt = DateTime.UtcNow,
                AppliedAt = DateTime.UtcNow
            };

            var savedApp = await _applicationRepository.CreateAsync(application);
            savedApplications.Add(savedApp);
        }

        // 5. Retornar DTO
        return new PaymentDto
        {
            Id = savedPayment.Id,
            PaymentNumber = savedPayment.PaymentNumber,
            CustomerId = savedPayment.CustomerId,
            CustomerName = savedPayment.CustomerName,
            CompanyId = savedPayment.CompanyId,
            PaymentDate = savedPayment.PaymentDate,
            TotalAmount = savedPayment.TotalAmount,
            Currency = savedPayment.Currency,
            PaymentFormSAT = savedPayment.PaymentFormSAT,
            Reference = savedPayment.Reference,
            Status = savedPayment.Status,
            AppliedToInvoices = savedPayment.AppliedToInvoices,
            ComplementsGenerated = savedPayment.ComplementsGenerated,
            ComplementsWithError = savedPayment.ComplementsWithError,
            Notes = savedPayment.Notes,
            CreatedAt = savedPayment.CreatedAt,
            AppliedAt = savedPayment.AppliedAt,
            Applications = savedApplications.Select(a => new PaymentApplicationDto
            {
                Id = a.Id,
                PaymentId = a.PaymentId,
                InvoicePPDId = a.InvoicePPDId,
                FolioUUID = a.FolioUUID,
                SerieAndFolio = a.SerieAndFolio,
                PaymentType = a.PaymentType,
                PartialityNumber = a.PartialityNumber,
                PreviousBalance = a.PreviousBalance,
                AmountApplied = a.AmountApplied,
                NewBalance = a.NewBalance,
                DocumentCurrency = a.DocumentCurrency,
                DocumentExchangeRate = a.DocumentExchangeRate,
                TaxObject = a.TaxObject,
                TaxBase = a.TaxBase,
                TaxCode = a.TaxCode,
                TaxFactorType = a.TaxFactorType,
                TaxRate = a.TaxRate,
                TaxAmount = a.TaxAmount,
                ComplementStatus = a.ComplementStatus,
                CreatedAt = a.CreatedAt,
                GeneratedAt = a.GeneratedAt
            }).ToList()
        };
    }
}
