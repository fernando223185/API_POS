using Application.Abstractions.AccountsReceivable;
using Application.Core.AccountsReceivable.Commands;
using Application.DTOs.AccountsReceivable;
using Domain.Entities;
using MediatR;

namespace Application.Core.AccountsReceivable.CommandHandlers;

/// <summary>
/// Handler para crear una factura PPD en el sistema de Cuentas por Cobrar
/// Se ejecuta automáticamente después de timbrar una factura con MetodoPago = PPD
/// </summary>
public class CreateInvoicePPDCommandHandler : IRequestHandler<CreateInvoicePPDCommand, InvoicePPDDto>
{
    private readonly IInvoicePPDRepository _invoiceRepository;
    private readonly ICustomerCreditPolicyRepository _creditPolicyRepository;
    private readonly ICustomerCreditHistoryRepository _historyRepository;

    public CreateInvoicePPDCommandHandler(
        IInvoicePPDRepository invoiceRepository,
        ICustomerCreditPolicyRepository creditPolicyRepository,
        ICustomerCreditHistoryRepository historyRepository)
    {
        _invoiceRepository = invoiceRepository;
        _creditPolicyRepository = creditPolicyRepository;
        _historyRepository = historyRepository;
    }

    public async Task<InvoicePPDDto> Handle(CreateInvoicePPDCommand request, CancellationToken cancellationToken)
    {
        // 1. Verificar que no exista ya una factura PPD con este UUID
        var existing = await _invoiceRepository.GetByUUIDAsync(request.FolioUUID);
        if (existing != null)
        {
            throw new InvalidOperationException($"Ya existe una factura PPD con UUID {request.FolioUUID}");
        }

        // 2. Obtener o crear política de crédito del cliente
        var creditPolicy = await _creditPolicyRepository.GetByCustomerIdAsync(request.CustomerId);
        if (creditPolicy == null)
        {
            // Si no tiene política, crear una por defecto (esto debería manejarse en otro lugar idealmente)
            throw new InvalidOperationException($"El cliente {request.CustomerId} no tiene política de crédito configurada");
        }

        // 3. Calcular fecha de vencimiento basada en días de crédito
        var dueDate = request.InvoiceDate.AddDays(request.CreditDays);

        // 4. Crear entidad InvoicePPD
        var invoicePPD = new InvoicePPD
        {
            InvoiceId = request.InvoiceId,
            CustomerId = request.CustomerId,
            CustomerName = request.CustomerName,
            CustomerRFC = request.CustomerRFC,
            CustomerZipCode = request.CustomerZipCode,
            CustomerTaxRegime = request.CustomerTaxRegime,
            CustomerCfdiUse = request.CustomerCfdiUse,
            CompanyId = request.CompanyId,
            FolioUUID = request.FolioUUID,
            Serie = request.Serie,
            Folio = request.Folio,
            SerieAndFolio = $"{request.Serie}-{request.Folio}",
            InvoiceDate = request.InvoiceDate,
            DueDate = dueDate,
            Currency = request.Currency,
            ExchangeRate = request.ExchangeRate,
            OriginalAmount = request.TotalAmount,
            Subtotal = request.Subtotal,
            TaxAmount = request.TaxAmount,
            PaidAmount = 0,
            BalanceAmount = request.TotalAmount,
            NextPartialityNumber = 1,
            TotalPartialities = 0,
            Status = "Pending",
            DaysOverdue = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy,
            IsActive = true,
            Notes = request.Notes
        };

        // 5. Guardar en base de datos
        var created = await _invoiceRepository.CreateAsync(invoicePPD);

        // 6. ⚠️ IMPORTANTE: NO actualizar CreditPolicy aquí
        // El crédito ya se afectó cuando se creó la venta (ANTES de facturar)
        // Solo registramos el evento de que la factura fue generada

        // 7. Registrar evento en historial de crédito
        var historyEvent = new CustomerCreditHistory
        {
            CustomerId = request.CustomerId,
            CompanyId = request.CompanyId,
            EventType = "InvoiceGenerated",
            EventDate = DateTime.UtcNow,
            Amount = request.TotalAmount,
            RelatedEntity = "InvoicePPD",
            RelatedEntityId = created.Id,
            Description = $"Factura {invoicePPD.SerieAndFolio} timbrada con UUID {request.FolioUUID}",
            Notes = $"Monto: {request.TotalAmount:C}, Vencimiento: {dueDate:yyyy-MM-dd}",
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        await _historyRepository.CreateAsync(historyEvent);

        // 8. Mapear entidad a DTO y retornar
        var dto = new InvoicePPDDto
        {
            Id = created.Id,
            InvoiceId = created.InvoiceId,
            CustomerId = created.CustomerId,
            CustomerName = created.CustomerName,
            CustomerRFC = created.CustomerRFC,
            CustomerZipCode = created.CustomerZipCode,
            CustomerTaxRegime = created.CustomerTaxRegime,
            CustomerCfdiUse = created.CustomerCfdiUse,
            FolioUUID = created.FolioUUID,
            Serie = created.Serie,
            Folio = created.Folio,
            SerieAndFolio = created.SerieAndFolio,
            InvoiceDate = created.InvoiceDate,
            DueDate = created.DueDate,
            Currency = created.Currency,
            ExchangeRate = created.ExchangeRate,
            OriginalAmount = created.OriginalAmount,
            Subtotal = created.Subtotal,
            TaxAmount = created.TaxAmount,
            PaidAmount = created.PaidAmount,
            BalanceAmount = created.BalanceAmount,
            NextPartialityNumber = created.NextPartialityNumber,
            TotalPartialities = created.TotalPartialities,
            Status = created.Status,
            DaysOverdue = created.DaysOverdue,
            LastPaymentDate = created.LastPaymentDate,
            Notes = created.Notes,
            CreatedAt = created.CreatedAt
        };

        return dto;
    }
}
