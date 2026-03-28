using Application.Abstractions.AccountsReceivable;
using Application.Abstractions.Billing;
using Application.Core.AccountsReceivable.Commands;
using Application.DTOs.AccountsReceivable;
using Domain.Entities;
using MediatR;

namespace Application.Core.AccountsReceivable.CommandHandlers;

/// <summary>
/// Handler para crear o actualizar la política de crédito de un cliente
/// </summary>
public class UpsertCustomerCreditPolicyCommandHandler : IRequestHandler<UpsertCustomerCreditPolicyCommand, CustomerCreditPolicyDto>
{
    private readonly ICustomerCreditPolicyRepository _policyRepository;
    private readonly ICustomerCreditHistoryRepository _historyRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly Application.Abstractions.Catalogue.IUserRepository _userRepository;

    public UpsertCustomerCreditPolicyCommandHandler(
        ICustomerCreditPolicyRepository policyRepository,
        ICustomerCreditHistoryRepository historyRepository,
        IInvoiceRepository invoiceRepository,
        Application.Abstractions.Catalogue.IUserRepository userRepository)
    {
        _policyRepository = policyRepository;
        _historyRepository = historyRepository;
        _invoiceRepository = invoiceRepository;
        _userRepository = userRepository;
    }

    public async Task<CustomerCreditPolicyDto> Handle(UpsertCustomerCreditPolicyCommand command, CancellationToken cancellationToken)
    {
        // 0. Obtener CompanyId del usuario si no se proporcionó
        int companyId = command.CompanyId;
        if (companyId == 0 && command.UserId > 0)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId);
            if (user?.CompanyId != null)
            {
                companyId = user.CompanyId.Value;
            }
            else
            {
                throw new InvalidOperationException("El usuario no tiene una compañía asignada. Por favor, proporcione el CompanyId manualmente.");
            }
        }

        if (companyId == 0)
        {
            throw new InvalidOperationException("Se requiere un CompanyId válido para crear la política de crédito.");
        }

        // 1. Buscar política existente
        var existingPolicy = await _policyRepository.GetByCustomerIdAsync(command.CustomerId);

        CustomerCreditPolicy policy;
        bool isNew = existingPolicy == null;

        if (isNew)
        {
            // Crear nueva política
            policy = new CustomerCreditPolicy
            {
                CustomerId = command.CustomerId,
                CompanyId = companyId,
                CreditLimit = command.CreditLimit,
                CreditDays = command.CreditDays,
                OverdueGraceDays = command.OverdueGraceDays,
                Status = "Active",
                AutoBlockOnOverdue = command.AutoBlockOnOverdue,
                Notes = command.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            policy = await _policyRepository.CreateAsync(policy);

            // Registrar en historial
            await _historyRepository.CreateAsync(new CustomerCreditHistory
            {
                CustomerId = command.CustomerId,
                CompanyId = companyId,
                EventType = "PolicyCreated",
                EventDate = DateTime.UtcNow,
                Description = $"Política de crédito creada. Límite: ${command.CreditLimit:N2}, Plazo: {command.CreditDays} días",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = command.ExecutedBy
            });
        }
        else
        {
            // Actualizar política existente
            var changes = new List<string>();

            if (existingPolicy.CreditLimit != command.CreditLimit)
                changes.Add($"Límite: ${existingPolicy.CreditLimit:N2} → ${command.CreditLimit:N2}");

            if (existingPolicy.CreditDays != command.CreditDays)
                changes.Add($"Plazo: {existingPolicy.CreditDays} → {command.CreditDays} días");

            if (existingPolicy.OverdueGraceDays != command.OverdueGraceDays)
                changes.Add($"Días de gracia: {existingPolicy.OverdueGraceDays} → {command.OverdueGraceDays}");

            existingPolicy.CreditLimit = command.CreditLimit;
            existingPolicy.CreditDays = command.CreditDays;
            existingPolicy.OverdueGraceDays = command.OverdueGraceDays;
            existingPolicy.AutoBlockOnOverdue = command.AutoBlockOnOverdue;
            existingPolicy.Notes = command.Notes;
            existingPolicy.UpdatedAt = DateTime.UtcNow;

            policy = await _policyRepository.UpdateAsync(existingPolicy);

            // Registrar cambios en historial
            if (changes.Any())
            {
                await _historyRepository.CreateAsync(new CustomerCreditHistory
                {
                    CustomerId = command.CustomerId,
                    CompanyId = companyId,
                    EventType = "PolicyUpdated",
                    EventDate = DateTime.UtcNow,
                    Description = $"Política actualizada: {string.Join(", ", changes)}",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = command.ExecutedBy
                });
            }
        }

        // 2. Calcular métricas actuales
        var (pendingAmount, overdueAmount) = await _invoiceRepository.GetCustomerBalanceSummaryAsync(command.CustomerId, command.CompanyId);
        var availableCredit = policy.CreditLimit - pendingAmount;

        // 3. Retornar DTO
        return new CustomerCreditPolicyDto
        {
            Id = policy.Id,
            CustomerId = policy.CustomerId,
            CustomerName = "", // Se llena desde el controller si es necesario
            CompanyId = policy.CompanyId,
            CreditLimit = policy.CreditLimit,
            CreditDays = policy.CreditDays,
            OverdueGraceDays = policy.OverdueGraceDays,
            TotalPendingAmount = pendingAmount,
            TotalOverdueAmount = overdueAmount,
            AvailableCredit = availableCredit,
            Status = policy.Status,
            BlockReason = policy.BlockReason,
            AutoBlockOnOverdue = policy.AutoBlockOnOverdue,
            Notes = policy.Notes,
            CreatedAt = policy.CreatedAt,
            UpdatedAt = policy.UpdatedAt
        };
    }
}
