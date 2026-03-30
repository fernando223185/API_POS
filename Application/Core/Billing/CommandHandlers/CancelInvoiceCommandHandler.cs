using Application.Abstractions.AccountsReceivable;
using Application.Abstractions.Billing;
using Application.Core.Billing.Commands;
using Application.DTOs.Billing;
using Domain.Entities;
using MediatR;

namespace Application.Core.Billing.CommandHandlers
{
    /// <summary>
    /// Handler para cancelar una factura timbrada ante el SAT vía Sapiens
    /// </summary>
    public class CancelInvoiceCommandHandler : IRequestHandler<CancelInvoiceCommand, CancelInvoiceResultDto>
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ISapiensService _sapiensService;
        private readonly ICustomerCreditPolicyRepository _creditPolicyRepository;
        private readonly ICustomerCreditHistoryRepository _creditHistoryRepository;

        public CancelInvoiceCommandHandler(
            IInvoiceRepository invoiceRepository,
            ISapiensService sapiensService,
            ICustomerCreditPolicyRepository creditPolicyRepository,
            ICustomerCreditHistoryRepository creditHistoryRepository)
        {
            _invoiceRepository = invoiceRepository;
            _sapiensService = sapiensService;
            _creditPolicyRepository = creditPolicyRepository;
            _creditHistoryRepository = creditHistoryRepository;
        }

        public async Task<CancelInvoiceResultDto> Handle(CancelInvoiceCommand command, CancellationToken cancellationToken)
        {
            // 1. Cargar factura
            var invoice = await _invoiceRepository.GetByIdAsync(command.InvoiceId);
            if (invoice == null)
                return new CancelInvoiceResultDto { Success = false, Message = $"Factura con ID {command.InvoiceId} no encontrada" };

            // 2. Validaciones
            if (invoice.Status == "Cancelada")
                return new CancelInvoiceResultDto { Success = false, Message = "La factura ya está cancelada" };

            if (invoice.Status == "Borrador")
                return new CancelInvoiceResultDto { Success = false, Message = "No se puede cancelar una factura en estado Borrador. Solo facturas timbradas." };

            if (string.IsNullOrEmpty(invoice.Uuid))
                return new CancelInvoiceResultDto { Success = false, Message = "La factura no tiene UUID, no puede cancelarse ante el SAT" };

            var motivosValidos = new[] { "01", "02", "03", "04" };
            if (!motivosValidos.Contains(command.Motivo))
                return new CancelInvoiceResultDto { Success = false, Message = "Motivo inválido. Use: 01=Con sustitución, 02=Sin sustitución, 03=No se realizó, 04=Global" };

            if (command.Motivo == "01" && string.IsNullOrWhiteSpace(command.FolioSustitucion))
                return new CancelInvoiceResultDto { Success = false, Message = "El Motivo 01 requiere el UUID del CFDI sustituto (FolioSustitucion)" };

            // 3. Para facturas PPD: validar que no haya complementos de pago activos
            if (invoice.MetodoPago == "PPD")
            {
                var complementosActivos = invoice.PaymentApplications
                    .Where(pa => pa.Payment != null
                        && pa.Payment.Uuid != null          // tiene complemento timbrado
                        && pa.Payment.Status != "Cancelled") // y no está ya cancelado
                    .ToList();

                if (complementosActivos.Any())
                {
                    var numeros = string.Join(", ", complementosActivos
                        .Select(pa => pa.Payment!.PaymentNumber)
                        .Distinct());
                    return new CancelInvoiceResultDto
                    {
                        Success = false,
                        Message = $"Esta factura PPD tiene {complementosActivos.Count} complemento(s) de pago activo(s) que deben cancelarse primero: {numeros}. " +
                                  "Cancela los complementos desde Cuentas por Cobrar y luego intenta cancelar la factura."
                    };
                }
            }

            // 4. Llamar al SAT vía Sapiens (sin complementos activos, es seguro cancelar)
            var cancelResponse = await _sapiensService.CancelInvoiceAsync(
                rfcEmisor: invoice.EmisorRfc,
                uuid: invoice.Uuid,
                rfcReceptor: invoice.ReceptorRfc,
                total: invoice.Total,
                motivo: command.Motivo,
                folioSustitucion: command.FolioSustitucion);

            // 4. Determinar el nuevo status según la respuesta del SAT
            // "Cancelable sin aceptación" o statusCancelation = "Cancelado" => Cancelada directamente
            // "Cancelable con aceptación" => En proceso (el receptor tiene 3 días para aceptar/rechazar)
            var requiresAcceptance = cancelResponse.data?.isCancelable == "Cancelable con aceptación";
            var newStatus = requiresAcceptance ? "EnProcesoCancelacion" : "Cancelada";

            // 5. Determinar el nuevo status ya fue calculado arriba
            // 6. Actualizar la factura en BD
            // El dict data.uuid tiene forma { "uuid-cfdi": "201" } — extraer el code del propio UUID
            var satCode = cancelResponse.data?.uuid != null && invoice.Uuid != null
                && cancelResponse.data.uuid.TryGetValue(invoice.Uuid, out var code) ? code : null;

            invoice.Status = newStatus;
            invoice.CancellationMotivo = command.Motivo;
            invoice.CancellationFolioSustitucion = command.FolioSustitucion;
            invoice.CancellationReason = command.Reason;
            invoice.CancellationAcuse = cancelResponse.data?.acuse;
            invoice.CancellationSatCode = satCode;
            invoice.CancelledAt = DateTime.UtcNow;
            invoice.CancelledByUserId = command.UserId;

            await _invoiceRepository.UpdateAsync(invoice);

            // 7. Para facturas PPD: actualizar estado de cuenta del cliente
            if (invoice.MetodoPago == "PPD" && !requiresAcceptance && invoice.CustomerId.HasValue)
            {
                var customerId = invoice.CustomerId.Value;

                // Recalcular balances desde 0 leyendo todas las facturas activas del cliente
                var (pendingAmount, overdueAmount) = await _invoiceRepository.GetCustomerBalanceSummaryAsync(customerId, invoice.CompanyId);
                await _creditPolicyRepository.UpdateBalancesAsync(customerId, pendingAmount, overdueAmount);

                // Registrar el evento en el historial de crédito
                var policy = await _creditPolicyRepository.GetByCustomerIdAsync(customerId);
                await _creditHistoryRepository.CreateAsync(new CustomerCreditHistory
                {
                    CustomerId = customerId,
                    CompanyId = invoice.CompanyId,
                    EventType = "InvoiceCancelled",
                    EventDate = DateTime.UtcNow,
                    Amount = invoice.BalanceAmount,      // saldo que se libera
                    RelatedEntity = "Invoice",
                    RelatedEntityId = invoice.Id,
                    PreviousValue = invoice.Total.ToString("0.00"),
                    NewValue = "0.00",
                    Description = $"Factura {invoice.Serie}-{invoice.Folio} cancelada ante el SAT (Motivo {invoice.CancellationMotivo}). Saldo liberado: ${invoice.BalanceAmount:N2}",
                    Notes = $"UUID: {invoice.Uuid} | Código SAT: {satCode}"
                });
            }

            // 8. Mensaje amigable para el contador
            var message = newStatus == "Cancelada"
                ? "Factura cancelada correctamente ante el SAT"
                : "Solicitud de cancelación enviada. El receptor tiene 3 días para aceptar o rechazar. Si no responde, se cancela automáticamente.";

            return new CancelInvoiceResultDto
            {
                Success = true,
                Message = message,
                Uuid = invoice.Uuid,
                StatusSat = cancelResponse.data?.statusSat,
                StatusCancelation = cancelResponse.data?.statusCancelation,
                IsCancelable = cancelResponse.data?.isCancelable,
                RequiresReceiverAcceptance = requiresAcceptance
            };
        }
    }
}
