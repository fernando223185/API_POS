using Application.Abstractions.AccountsReceivable;
using Application.Abstractions.Billing;
using Application.Core.AccountsReceivable.Commands;
using Application.DTOs.Billing;
using MediatR;

namespace Application.Core.AccountsReceivable.CommandHandlers;

/// <summary>
/// Cancela el complemento de pago (CFDI tipo P) ante el SAT y revierte el balance de las facturas afectadas
/// </summary>
public class CancelPaymentComplementCommandHandler
    : IRequestHandler<CancelPaymentComplementCommand, CancelInvoiceResultDto>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ISapiensService _sapiensService;

    public CancelPaymentComplementCommandHandler(
        IPaymentRepository paymentRepository,
        IInvoiceRepository invoiceRepository,
        ISapiensService sapiensService)
    {
        _paymentRepository = paymentRepository;
        _invoiceRepository = invoiceRepository;
        _sapiensService = sapiensService;
    }

    public async Task<CancelInvoiceResultDto> Handle(
        CancelPaymentComplementCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Cargar el pago con sus aplicaciones
        var payment = await _paymentRepository.GetByIdAsync(command.PaymentId);
        if (payment == null)
            return new CancelInvoiceResultDto { Success = false, Message = $"Pago con ID {command.PaymentId} no encontrado" };

        // 2. Validaciones
        if (payment.Status == "Cancelled")
            return new CancelInvoiceResultDto { Success = false, Message = "El complemento de pago ya está cancelado" };

        if (string.IsNullOrEmpty(payment.Uuid))
            return new CancelInvoiceResultDto { Success = false, Message = "El pago no tiene UUID de complemento timbrado, no puede cancelarse ante el SAT" };

        if (string.IsNullOrEmpty(payment.EmisorRfc) || string.IsNullOrEmpty(payment.ReceptorRfc))
            return new CancelInvoiceResultDto { Success = false, Message = "El pago no tiene RFC de emisor/receptor registrado" };

        var motivosValidos = new[] { "01", "02", "03", "04" };
        if (!motivosValidos.Contains(command.Motivo))
            return new CancelInvoiceResultDto { Success = false, Message = "Motivo inválido. Use: 01=Con sustitución, 02=Sin sustitución, 03=No se realizó, 04=Global" };

        if (command.Motivo == "01" && string.IsNullOrWhiteSpace(command.FolioSustitucion))
            return new CancelInvoiceResultDto { Success = false, Message = "El Motivo 01 requiere el UUID del CFDI sustituto (FolioSustitucion)" };

        // 3. Llamar al SAT vía Sapiens (mismo servicio que facturas)
        var cancelResponse = await _sapiensService.CancelInvoiceAsync(
            rfcEmisor: payment.EmisorRfc,
            uuid: payment.Uuid,
            rfcReceptor: payment.ReceptorRfc,
            total: payment.TotalAmount,
            motivo: command.Motivo,
            folioSustitucion: command.FolioSustitucion);

        // 4. Determinar nuevo status
        var requiresAcceptance = cancelResponse.data?.isCancelable == "Cancelable con aceptación";
        var newStatus = requiresAcceptance ? "CancellationPending" : "Cancelled";

        // 5. Si la cancelación es efectiva (no requiere aceptación pendiente),
        //    revertir el balance de cada factura que fue abonada por este pago
        if (!requiresAcceptance && payment.PaymentApplications.Any())
        {
            foreach (var application in payment.PaymentApplications)
            {
                var invoice = await _invoiceRepository.GetByIdAsync(application.InvoiceId);
                if (invoice == null) continue;

                // Revertir: devolver el monto aplicado al balance
                invoice.PaidAmount = Math.Max(0, invoice.PaidAmount - application.AmountApplied);
                invoice.BalanceAmount = invoice.Total - invoice.PaidAmount;

                // Actualizar estado de pago de la factura
                invoice.PaymentStatus = invoice.BalanceAmount <= 0
                    ? "Paid"
                    : invoice.PaidAmount > 0
                        ? "PartiallyPaid"
                        : "Pending";

                invoice.UpdatedAt = DateTime.UtcNow;
                await _invoiceRepository.UpdateAsync(invoice);
            }
        }

        // 6. Actualizar el pago en BD
        var satCode = cancelResponse.data?.uuid != null && payment.Uuid != null
            && cancelResponse.data.uuid.TryGetValue(payment.Uuid, out var code) ? code : null;

        payment.Status = newStatus;
        payment.CancellationMotivo = command.Motivo;
        payment.CancellationFolioSustitucion = command.FolioSustitucion;
        payment.CancellationReason = command.Reason;
        payment.CancellationAcuse = cancelResponse.data?.acuse;
        payment.CancellationSatCode = satCode;
        payment.CancelledAt = DateTime.UtcNow;
        payment.CancelledBy = command.UserId;

        await _paymentRepository.UpdateAsync(payment);

        // 7. Respuesta
        var message = newStatus == "Cancelled"
            ? $"Complemento de pago cancelado correctamente. Se revirtió el balance de {payment.PaymentApplications.Count} factura(s)."
            : "Solicitud de cancelación enviada. El receptor tiene 3 días para aceptar o rechazar. El balance se revertirá cuando el SAT confirme la cancelación.";

        return new CancelInvoiceResultDto
        {
            Success = true,
            Message = message,
            StatusSat = cancelResponse.data?.statusSat,
            StatusCancelation = cancelResponse.data?.statusCancelation,
            IsCancelable = cancelResponse.data?.isCancelable,
            Uuid = payment.Uuid,
            RequiresReceiverAcceptance = requiresAcceptance
        };
    }
}
