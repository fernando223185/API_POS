using Application.Abstractions.AccountsReceivable;
using Application.Core.AccountsReceivable.Commands;
using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.CommandHandlers;

/// <summary>
/// Handler para generar complementos de pago de un lote completo
/// </summary>
public class GenerateBatchComplementsCommandHandler : IRequestHandler<GenerateBatchComplementsCommand, GenerateComplementsResultDto>
{
    private readonly IPaymentBatchRepository _batchRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentComplementLogRepository _logRepository;

    public GenerateBatchComplementsCommandHandler(
        IPaymentBatchRepository batchRepository,
        IPaymentRepository paymentRepository,
        IPaymentComplementLogRepository logRepository)
    {
        _batchRepository = batchRepository;
        _paymentRepository = paymentRepository;
        _logRepository = logRepository;
    }

    public async Task<GenerateComplementsResultDto> Handle(GenerateBatchComplementsCommand command, CancellationToken cancellationToken)
    {
        // 1. Obtener lote con pagos
        var batch = await _batchRepository.GetByIdAsync(command.BatchId)
            ?? throw new InvalidOperationException($"Lote {command.BatchId} no encontrado");

        if (batch.Status == "Completed")
            throw new InvalidOperationException("Este lote ya fue procesado completamente");

        // 2. Actualizar estado del lote
        batch.Status = "Processing";
        await _batchRepository.UpdateAsync(batch);

        // 3. Procesar cada pago del lote
        int totalProcessed = 0;
        int totalSuccess = 0;
        int totalErrors = 0;
        var errors = new List<string>();

        foreach (var payment in batch.Payments)
        {
            try
            {
                // TODO: Aquí iría la lógica real de generación de complementos
                // Por ahora solo simulamos el proceso
                
                // Marcar pago como procesado
                payment.Status = "Completed";
                payment.CompletedAt = DateTime.UtcNow;
                await _paymentRepository.UpdateAsync(payment);

                totalSuccess++;
            }
            catch (Exception ex)
            {
                totalErrors++;
                errors.Add($"Pago {payment.PaymentNumber}: {ex.Message}");

                // Registrar error en log
                var log = new Domain.Entities.PaymentComplementLog
                {
                    PaymentId = payment.Id,
                    BatchId = batch.Id,
                    PaymentApplicationId = 0,
                    AttemptNumber = 1,
                    Status = "Error",
                    Action = "GenerateBatch",
                    ErrorMessage = ex.Message,
                    AttemptDate = DateTime.UtcNow
                };
                await _logRepository.CreateAsync(log);
            }

            totalProcessed++;
            
            // Actualizar progreso del lote
            batch.ProcessingProgress = (int)((decimal)totalProcessed / batch.TotalPayments * 100);
            batch.ComplementsGenerated = totalSuccess;
            batch.ComplementsWithError = totalErrors;
            await _batchRepository.UpdateAsync(batch);
        }

        // 4. Actualizar estado final del lote
        batch.Status = totalErrors > 0 ? "PartialError" : "Completed";
        batch.CompletedAt = DateTime.UtcNow;
        batch.ProcessedBy = command.ExecutedBy;
        await _batchRepository.UpdateAsync(batch);

        // 5. Retornar resultado
        return new GenerateComplementsResultDto
        {
            TotalProcessed = totalProcessed,
            SuccessCount = totalSuccess,
            ErrorCount = totalErrors,
            Errors = errors,
            BatchId = batch.Id,
            BatchNumber = batch.BatchNumber
        };
    }
}
