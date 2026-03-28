using Application.Abstractions.AccountsReceivable;
using Application.Abstractions.Billing;
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
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ICompanyRepository _companyRepository;

    public CreatePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IPaymentApplicationRepository applicationRepository,
        IInvoiceRepository invoiceRepository,
        ICompanyRepository companyRepository)
    {
        _paymentRepository = paymentRepository;
        _applicationRepository = applicationRepository;
        _invoiceRepository = invoiceRepository;
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
        Invoice? firstInvoice = null;

        var invoices = new List<(PaymentInvoiceItem item, Invoice invoice)>();

        foreach (var item in command.Invoices)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(item.InvoicePPDId)
                ?? throw new InvalidOperationException(
                    $"Factura {item.InvoicePPDId} no encontrada.");

            // Guardar la primera factura para obtener datos del receptor
            if (firstInvoice == null)
                firstInvoice = invoice;

            if ((invoice.BalanceAmount ?? 0) <= 0)
                throw new InvalidOperationException(
                    $"La factura {invoice.Serie}-{invoice.Folio} ya está liquidada.");

            if (item.AmountToPay <= 0 || item.AmountToPay > (invoice.BalanceAmount ?? 0))
                throw new InvalidOperationException(
                    $"Monto inválido para la factura {invoice.Serie}-{invoice.Folio}. " +
                    $"Saldo disponible: {invoice.BalanceAmount:0.00}.");

            totalAmount += item.AmountToPay;
            customerName = invoice.ReceptorNombre;
            invoices.Add((item, invoice));
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
            BankDestination = command.BankDestination,
            BankAccountDestination = command.AccountDestination,
            Reference = command.Reference,
            Notes = command.Notes,
            // Datos del emisor: usar los del command si se enviaron, sino snapshot de Company
            EmisorRfc = command.EmisorRfc ?? company.TaxId,
            EmisorNombre = command.EmisorNombre ?? company.LegalName,
            EmisorRegimenFiscal = command.EmisorRegimenFiscal ?? company.SatTaxRegime,
            LugarExpedicion = command.LugarExpedicion ?? company.FiscalZipCode,
            // Datos del receptor: usar los del command si se enviaron, sino snapshot de la factura
            ReceptorRfc = command.ReceptorRfc ?? firstInvoice.ReceptorRfc,
            ReceptorNombre = command.ReceptorNombre ?? firstInvoice.ReceptorNombre,
            ReceptorDomicilioFiscal = command.ReceptorDomicilioFiscal ?? firstInvoice.ReceptorDomicilioFiscal,
            ReceptorRegimenFiscal = command.ReceptorRegimenFiscal ?? firstInvoice.ReceptorRegimenFiscal ?? string.Empty,
            ReceptorUsoCfdi = command.ReceptorUsoCfdi ?? "CP01", // CP01 = Pagos (fijo para complementos de pago)
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

        foreach (var (item, invoice) in invoices)
        {
            var newBalance = Math.Round((invoice.BalanceAmount ?? 0) - item.AmountToPay, 2);

            // Calcular impuestos proporcionalmente al monto pagado
            // Ejemplo: Si la factura es de $1000 con $160 de IVA y se paga $500,
            // entonces la base es $431.03 y el IVA es $68.97
            decimal proportionPaid = invoice.Total > 0 ? item.AmountToPay / invoice.Total : 0;
            decimal taxBase = Math.Round(invoice.SubTotal * proportionPaid, 6);
            decimal taxAmount = Math.Round((invoice.Total - invoice.SubTotal) * proportionPaid, 6);

            var application = new PaymentApplication
            {
                PaymentId = savedPayment.Id,
                InvoiceId = invoice.Id,
                CustomerId = invoice.CustomerId ?? 0,
                CustomerName = invoice.ReceptorNombre,
                FolioUUID = invoice.Uuid,
                SerieAndFolio = $"{invoice.Serie}-{invoice.Folio}",
                OriginalInvoiceAmount = invoice.Total,
                PaymentType = newBalance <= 0 ? "FullPayment" : "PartialPayment",
                PartialityNumber = invoice.NextPartialityNumber ?? 1,
                PreviousBalance = invoice.BalanceAmount ?? 0,
                AmountApplied = item.AmountToPay,
                NewBalance = newBalance,
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
                InvoiceId = a.InvoiceId,
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
                CreatedAt = a.CreatedAt,
                AppliedAt = a.AppliedAt
            }).ToList()
        };
    }
}
