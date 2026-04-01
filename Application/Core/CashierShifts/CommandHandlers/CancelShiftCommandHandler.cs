using Application.Abstractions.CashierShifts;
using Application.Core.CashierShifts.Commands;
using Application.DTOs.CashierShift;
using MediatR;

namespace Application.Core.CashierShifts.CommandHandlers
{
    /// <summary>
    /// Handler para cancelar un turno de cajero
    /// Solo se pueden cancelar turnos con estado 'Open'
    /// </summary>
    public class CancelShiftCommandHandler : IRequestHandler<CancelShiftCommand, CashierShiftResponseDto>
    {
        private readonly ICashierShiftRepository _shiftRepository;

        public CancelShiftCommandHandler(ICashierShiftRepository shiftRepository)
        {
            _shiftRepository = shiftRepository;
        }

        public async Task<CashierShiftResponseDto> Handle(CancelShiftCommand command, CancellationToken cancellationToken)
        {
            // 1. Validar que el turno existe
            var shift = await _shiftRepository.GetByIdAsync(command.ShiftId);
            if (shift == null)
            {
                throw new KeyNotFoundException($"Turno con ID {command.ShiftId} no encontrado");
            }

            // 2. Validar que el turno está abierto
            if (shift.Status != "Open")
            {
                throw new InvalidOperationException(
                    $"El turno {shift.Code} no puede cancelarse porque su estado es '{shift.Status}'. " +
                    "Solo se pueden cancelar turnos con estado 'Open'."
                );
            }

            // 3. Validar que se proporcionó una razón
            if (string.IsNullOrWhiteSpace(command.Reason))
            {
                throw new ArgumentException("Debe proporcionar una razón para cancelar el turno");
            }

            // 4. Actualizar estado
            shift.Status = "Cancelled";
            shift.CancellationReason = command.Reason;
            shift.CancelledAt = DateTime.UtcNow;
            shift.CancelledByUserId = command.UserId;

            // 5. Guardar cambios
            await _shiftRepository.UpdateAsync(shift);

            Console.WriteLine($"❌ Turno {shift.Code} cancelado - Razón: {command.Reason}");

            // 6. Recargar con relaciones actualizadas
            var updatedShift = await _shiftRepository.GetByIdAsync(shift.Id);
            if (updatedShift == null)
            {
                throw new InvalidOperationException("Error al obtener el turno actualizado");
            }

            return MapToResponseDto(updatedShift);
        }

        private static CashierShiftResponseDto MapToResponseDto(Domain.Entities.CashierShift shift)
        {
            return new CashierShiftResponseDto
            {
                Id = shift.Id,
                Code = shift.Code,
                CashierId = shift.CashierId,
                CashierName = shift.Cashier.Name,
                CashierCode = shift.Cashier.Code,
                WarehouseId = shift.WarehouseId,
                WarehouseName = shift.Warehouse.Name,
                BranchId = shift.BranchId,
                BranchName = shift.Branch?.Name,
                CompanyId = shift.CompanyId,
                CompanyName = shift.Company?.LegalName,
                OpeningDate = shift.OpeningDate,
                ClosingDate = shift.ClosingDate,
                Status = shift.Status,
                InitialCash = shift.InitialCash,
                ExpectedCash = shift.ExpectedCash,
                FinalCash = shift.FinalCash,
                Difference = shift.Difference,
                TotalSales = shift.TotalSales,
                CancelledSales = shift.CancelledSales,
                TotalSalesAmount = shift.TotalSalesAmount,
                CancelledSalesAmount = shift.CancelledSalesAmount,
                CashSales = shift.CashSales,
                CardSales = shift.CardSales,
                TransferSales = shift.TransferSales,
                OtherSales = shift.OtherSales,
                CashWithdrawals = shift.CashWithdrawals,
                CashDeposits = shift.CashDeposits,
                OpeningNotes = shift.OpeningNotes,
                ClosingNotes = shift.ClosingNotes,
                CancellationReason = shift.CancellationReason,
                CreatedAt = shift.CreatedAt,
                ClosedByUserId = shift.ClosedByUserId,
                ClosedByName = shift.ClosedBy?.Name,
                CancelledByUserId = shift.CancelledByUserId,
                CancelledByName = shift.CancelledBy?.Name,
                CancelledAt = shift.CancelledAt
            };
        }
    }
}
