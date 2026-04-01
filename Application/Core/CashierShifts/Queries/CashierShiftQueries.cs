using Application.DTOs.CashierShift;
using MediatR;

namespace Application.Core.CashierShifts.Queries
{
    /// <summary>
    /// Query para obtener turnos paginados con filtros
    /// </summary>
    public record GetCashierShiftsQuery(
        int Page,
        int PageSize,
        int? CashierId = null,
        int? WarehouseId = null,
        int? BranchId = null,
        int? CompanyId = null,
        string? Status = null,
        DateTime? FromDate = null,
        DateTime? ToDate = null
    ) : IRequest<CashierShiftPagedResponseDto>;

    /// <summary>
    /// Query para obtener un turno por ID
    /// </summary>
    public record GetShiftByIdQuery(
        int ShiftId
    ) : IRequest<CashierShiftResponseDto>;

    /// <summary>
    /// Query para obtener el turno activo de un cajero
    /// </summary>
    public record GetActiveShiftQuery(
        int CashierId,
        int? BranchId = null
    ) : IRequest<CashierShiftResponseDto?>;

    /// <summary>
    /// Query para obtener el reporte detallado de un turno
    /// </summary>
    public record GetShiftReportQuery(
        int ShiftId
    ) : IRequest<ShiftReportDto>;

    /// <summary>
    /// Query para generar el PDF del corte de caja
    /// </summary>
    public record GenerateShiftPdfQuery(
        int ShiftId
    ) : IRequest<byte[]>;
}
