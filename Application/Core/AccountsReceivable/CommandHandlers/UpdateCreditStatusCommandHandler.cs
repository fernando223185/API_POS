using Application.Abstractions.AccountsReceivable;
using Application.Core.AccountsReceivable.Commands;
using Domain.Entities;
using MediatR;

namespace Application.Core.AccountsReceivable.CommandHandlers;

/// <summary>
/// Handler para bloquear o desbloquear el crédito de un cliente
/// </summary>
public class UpdateCreditStatusCommandHandler : IRequestHandler<UpdateCreditStatusCommand, bool>
{
    private readonly ICustomerCreditPolicyRepository _policyRepository;
    private readonly ICustomerCreditHistoryRepository _historyRepository;

    public UpdateCreditStatusCommandHandler(
        ICustomerCreditPolicyRepository policyRepository,
        ICustomerCreditHistoryRepository historyRepository)
    {
        _policyRepository = policyRepository;
        _historyRepository = historyRepository;
    }

    public async Task<bool> Handle(UpdateCreditStatusCommand command, CancellationToken cancellationToken)
    {
        // 1. Buscar política del cliente
        var policy = await _policyRepository.GetByCustomerIdAsync(command.CustomerId);
        
        if (policy == null)
            throw new InvalidOperationException($"No se encontró política de crédito para el cliente {command.CustomerId}");

        // 2. Actualizar estado
        var previousStatus = policy.Status;

        policy.Status = command.Status;
        policy.BlockReason = command.Status != "Active" ? command.Reason : null;
        policy.UpdatedAt = DateTime.UtcNow;

        await _policyRepository.UpdateAsync(policy);

        // 3. Registrar en historial
        var statusDescription = command.Status switch
        {
            "Active" => "Crédito activado",
            "Blocked" => "Crédito bloqueado",
            "Suspended" => "Crédito suspendido",
            _ => $"Estado cambiado a {command.Status}"
        };

        await _historyRepository.CreateAsync(new CustomerCreditHistory
        {
            CustomerId = command.CustomerId,
            CompanyId = policy.CompanyId,
            EventType = "StatusChange",
            EventDate = DateTime.UtcNow,
            Description = $"{statusDescription}. {(command.Reason != null ? $"Razón: {command.Reason}" : "")}",
            PreviousValue = previousStatus,
            NewValue = command.Status,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = command.ExecutedBy
        });

        return true;
    }
}
