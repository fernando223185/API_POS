using Application.Abstractions.CashierShifts;
using Application.Abstractions.Sales;
using Application.Core.CashierShifts.Commands;
using Application.DTOs.CashierShift;
using MediatR;

namespace Application.Core.CashierShifts.CommandHandlers
{
    /// <summary>
    /// Handler para cerrar un turno de cajero
    /// Calcula automáticamente el efectivo esperado basado en ventas del período
    /// </summary>
    public class CloseShiftCommandHandler : IRequestHandler<CloseShiftCommand, CashierShiftResponseDto>
    {
        private readonly ICashierShiftRepository _shiftRepository;
        private readonly ISaleRepository _saleRepository;

        public CloseShiftCommandHandler(
            ICashierShiftRepository shiftRepository,
            ISaleRepository saleRepository)
        {
            _shiftRepository = shiftRepository;
            _saleRepository = saleRepository;
        }

        public async Task<CashierShiftResponseDto> Handle(CloseShiftCommand command, CancellationToken cancellationToken)
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
                    $"El turno {shift.Code} no puede cerrarse porque su estado es '{shift.Status}'. " +
                    "Solo se pueden cerrar turnos con estado 'Open'."
                );
            }

            // 3. Obtener todas las ventas del cajero en el período del turno
            var (sales, _) = await _saleRepository.GetPagedAsync(
                page: 1,
                pageSize: 10000, // Obtener todas las ventas
                warehouseId: shift.WarehouseId,
                userId: shift.CashierId,
                fromDate: shift.OpeningDate,
                toDate: DateTime.UtcNow,
                status: null // Todas las ventas (completadas y canceladas)
            );

            // 4. Calcular totales por estado
            var completedSales = sales.Where(s => s.Status == "Completed").ToList();
            var cancelledSales = sales.Where(s => s.Status == "Cancelled").ToList();

            shift.TotalSales = completedSales.Count;
            shift.TotalSalesAmount = completedSales.Sum(s => s.Total);
            shift.CancelledSales = cancelledSales.Count;
            shift.CancelledSalesAmount = cancelledSales.Sum(s => s.Total);

            // 5. Calcular ventas por método de pago (solo ventas completadas)
            shift.CashSales = 0;
            shift.CardSales = 0;
            shift.TransferSales = 0;
            shift.OtherSales = 0;

            foreach (var sale in completedSales)
            {
                // Agrupar por método de pago dominante en la venta
                var cashPayments = sale.Payments?.Where(p => p.PaymentMethod == "Efectivo").Sum(p => p.Amount) ?? 0;
                var cardPayments = sale.Payments?.Where(p => p.PaymentMethod == "Tarjeta").Sum(p => p.Amount) ?? 0;
                var transferPayments = sale.Payments?.Where(p => p.PaymentMethod == "Transferencia").Sum(p => p.Amount) ?? 0;
                var otherPayments = sale.Payments?
                    .Where(p => p.PaymentMethod != "Efectivo" && p.PaymentMethod != "Tarjeta" && p.PaymentMethod != "Transferencia")
                    .Sum(p => p.Amount) ?? 0;

                shift.CashSales += cashPayments;
                shift.CardSales += cardPayments;
                shift.TransferSales += transferPayments;
                shift.OtherSales += otherPayments;
            }

            // 6. Calcular efectivo esperado
            // Fórmula: InitialCash + VentasEnEfectivo + Depósitos - Retiros
            shift.ExpectedCash = shift.InitialCash + shift.CashSales + shift.CashDeposits - shift.CashWithdrawals;

            // 7. Aplicar efectivo final reportado por el cajero
            shift.FinalCash = command.Request.FinalCash;

            // 8. Calcular diferencia (+ = sobrante, - = faltante)
            shift.Difference = shift.FinalCash - shift.ExpectedCash;

            // 9. Guardar notas de cierre
            shift.ClosingNotes = command.Request.ClosingNotes;

            // 10. Actualizar estado y fechas
            shift.Status = "Closed";
            shift.ClosingDate = DateTime.UtcNow;
            shift.ClosedByUserId = command.UserId;

            // 11. Guardar cambios
            await _shiftRepository.UpdateAsync(shift);

            // Log para auditoría
            string differenceMsg = shift.Difference switch
            {
                0 => "✅ SIN DIFERENCIA (cuadrado)",
                > 0 => $"⚠️ SOBRANTE de ${Math.Abs(shift.Difference.Value):N2}",
                < 0 => $"❌ FALTANTE de ${Math.Abs(shift.Difference.Value):N2}"
            };

            Console.WriteLine($"🔒 Turno {shift.Code} cerrado - " +
                            $"Ventas: {shift.TotalSales} (${shift.TotalSalesAmount:N2}), " +
                            $"Esperado: ${shift.ExpectedCash:N2}, Final: ${shift.FinalCash:N2}, {differenceMsg}");

            // 12. Recargar con relaciones actualizadas
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
