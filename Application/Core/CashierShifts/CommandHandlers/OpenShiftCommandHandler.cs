using Application.Abstractions.CashierShifts;
using Application.Abstractions.Config;
using Application.Core.CashierShifts.Commands;
using Application.DTOs.CashierShift;
using Domain.Entities;
using MediatR;

namespace Application.Core.CashierShifts.CommandHandlers
{
    /// <summary>
    /// Handler para abrir un nuevo turno de cajero
    /// </summary>
    public class OpenShiftCommandHandler : IRequestHandler<OpenShiftCommand, CashierShiftResponseDto>
    {
        private readonly ICashierShiftRepository _shiftRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IBranchRepository _branchRepository;

        public OpenShiftCommandHandler(
            ICashierShiftRepository shiftRepository,
            IWarehouseRepository warehouseRepository,
            IBranchRepository branchRepository)
        {
            _shiftRepository = shiftRepository;
            _warehouseRepository = warehouseRepository;
            _branchRepository = branchRepository;
        }

        public async Task<CashierShiftResponseDto> Handle(OpenShiftCommand command, CancellationToken cancellationToken)
        {
            // 1. Validar que la sucursal existe
            var branch = await _branchRepository.GetByIdAsync(command.Request.BranchId);
            if (branch == null)
            {
                throw new KeyNotFoundException($"Sucursal con ID {command.Request.BranchId} no encontrada");
            }

            // 2. Obtener el almacén principal de la sucursal
            var warehouses = await _warehouseRepository.GetByBranchIdAsync(command.Request.BranchId, includeInactive: false);
            var warehouse = warehouses.FirstOrDefault();
            
            if (warehouse == null)
            {
                throw new InvalidOperationException(
                    $"No se encontró ningún almacén activo en la sucursal '{branch.Name}'. " +
                    "Contacta al administrador para configurar un almacén."
                );
            }

            // 3. Validar que no exista un turno activo para este cajero en esta sucursal
            var hasActiveShift = await _shiftRepository.HasActiveShiftAsync(
                command.CashierId,
                branchId: command.Request.BranchId
            );

            if (hasActiveShift)
            {
                throw new InvalidOperationException(
                    $"Ya existe un turno activo para este cajero en la sucursal '{branch.Name}'. " +
                    "Debes cerrar el turno actual antes de abrir uno nuevo."
                );
            }

            // 4. Generar código del turno
            var code = await _shiftRepository.GetNextCodeAsync();

            // 5. Crear el turno
            var shift = new CashierShift
            {
                Code = code,
                CashierId = command.CashierId,
                WarehouseId = warehouse.Id,
                BranchId = warehouse.BranchId,
                CompanyId = warehouse.Branch?.CompanyId,
                OpeningDate = DateTime.UtcNow,
                Status = "Open",
                InitialCash = command.Request.InitialCash,
                ExpectedCash = command.Request.InitialCash,
                OpeningNotes = command.Request.OpeningNotes,
                CreatedAt = DateTime.UtcNow,
                
                // Inicializar contadores en 0
                TotalSales = 0,
                CancelledSales = 0,
                TotalSalesAmount = 0,
                CancelledSalesAmount = 0,
                CashSales = 0,
                CardSales = 0,
                TransferSales = 0,
                OtherSales = 0,
                CashWithdrawals = 0,
                CashDeposits = 0
            };

            var createdShift = await _shiftRepository.CreateAsync(shift);

            Console.WriteLine($"✅ Turno {createdShift.Code} abierto - Cajero: {command.CashierId}, " +
                            $"Sucursal: {branch.Name}, Almacén: {warehouse.Name}, Fondo inicial: ${createdShift.InitialCash:N2}");

            // 6. Recargar con relaciones
            var shiftWithRelations = await _shiftRepository.GetByIdAsync(createdShift.Id);
            if (shiftWithRelations == null)
            {
                throw new InvalidOperationException("Error al obtener el turno creado");
            }

            return MapToResponseDto(shiftWithRelations);
        }

        private static CashierShiftResponseDto MapToResponseDto(CashierShift shift)
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
