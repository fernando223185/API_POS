using MediatR;

namespace Application.Core.AccountsReceivable.Commands;

/// <summary>
/// Command para bloquear o desbloquear el crédito de un cliente
/// </summary>
public class UpdateCreditStatusCommand : IRequest<bool>
{
    public int CustomerId { get; set; }
    public string Status { get; set; } = string.Empty; // Active, Blocked, Suspended
    public string? Reason { get; set; }
    public int ExecutedBy { get; set; }
}
