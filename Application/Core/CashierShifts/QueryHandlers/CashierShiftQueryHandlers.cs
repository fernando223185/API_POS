using Application.Abstractions.CashierShifts;
using Application.Core.CashierShifts.Queries;
using Application.DTOs.CashierShift;
using MediatR;

namespace Application.Core.CashierShifts.QueryHandlers
{
    /// <summary>
    /// Handler para obtener turnos de cajero paginados con filtros
    /// </summary>
    public class GetCashierShiftsQueryHandler : IRequestHandler<GetCashierShiftsQuery, CashierShiftPagedResponseDto>
    {
        private readonly ICashierShiftRepository _shiftRepository;

        public GetCashierShiftsQueryHandler(ICashierShiftRepository shiftRepository)
        {
            _shiftRepository = shiftRepository;
        }

        public async Task<CashierShiftPagedResponseDto> Handle(GetCashierShiftsQuery query, CancellationToken cancellationToken)
        {
            var (shifts, totalCount) = await _shiftRepository.GetPagedAsync(
                query.Page,
                query.PageSize,
                query.CashierId,
                query.WarehouseId,
                query.BranchId,
                query.CompanyId,
                query.Status,
                query.FromDate,
                query.ToDate
            );

            var shiftDtos = shifts.Select(shift => new CashierShiftResponseDto
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
            }).ToList();

            return new CashierShiftPagedResponseDto
            {
                Message = "Turnos obtenidos exitosamente",
                Error = 0,
                Data = shiftDtos,
                TotalRecords = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }
    }

    /// <summary>
    /// Handler para obtener un turno por ID
    /// </summary>
    public class GetShiftByIdQueryHandler : IRequestHandler<GetShiftByIdQuery, CashierShiftResponseDto>
    {
        private readonly ICashierShiftRepository _shiftRepository;

        public GetShiftByIdQueryHandler(ICashierShiftRepository shiftRepository)
        {
            _shiftRepository = shiftRepository;
        }

        public async Task<CashierShiftResponseDto> Handle(GetShiftByIdQuery query, CancellationToken cancellationToken)
        {
            var shift = await _shiftRepository.GetByIdAsync(query.ShiftId);
            if (shift == null)
            {
                throw new KeyNotFoundException($"Turno con ID {query.ShiftId} no encontrado");
            }

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

    /// <summary>
    /// Handler para obtener el turno activo de un cajero
    /// </summary>
    public class GetActiveShiftQueryHandler : IRequestHandler<GetActiveShiftQuery, CashierShiftResponseDto?>
    {
        private readonly ICashierShiftRepository _shiftRepository;

        public GetActiveShiftQueryHandler(ICashierShiftRepository shiftRepository)
        {
            _shiftRepository = shiftRepository;
        }

        public async Task<CashierShiftResponseDto?> Handle(GetActiveShiftQuery query, CancellationToken cancellationToken)
        {
            var shift = await _shiftRepository.GetActiveShiftAsync(query.CashierId, query.BranchId);
            
            if (shift == null)
            {
                return null;
            }

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
