using Application.DTOs.CashierShift;
using MediatR;

namespace Application.Core.CashierShifts.Commands
{
    /// <summary>
    /// Comando para abrir un turno de cajero
    /// </summary>
    public record OpenShiftCommand(
        OpenShiftRequestDto Request,
        int CashierId
    ) : IRequest<CashierShiftResponseDto>;

    /// <summary>
    /// Comando para cerrar un turno de cajero
    /// </summary>
    public record CloseShiftCommand(
        int ShiftId,
        CloseShiftRequestDto Request,
        int UserId
    ) : IRequest<CashierShiftResponseDto>;

    /// <summary>
    /// Comando para cancelar un turno de cajero
    /// </summary>
    public record CancelShiftCommand(
        int ShiftId,
        string Reason,
        int UserId
    ) : IRequest<CashierShiftResponseDto>;
}
