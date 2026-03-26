using Application.Abstractions.AccountsReceivable;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CustomerCreditPolicyRepository : ICustomerCreditPolicyRepository
{
    private readonly POSDbContext _context;

    public CustomerCreditPolicyRepository(POSDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerCreditPolicy?> GetByCustomerIdAsync(int customerId)
    {
        return await _context.CustomerCreditPolicies
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);
    }

    public async Task<CustomerCreditPolicy> CreateAsync(CustomerCreditPolicy policy)
    {
        _context.CustomerCreditPolicies.Add(policy);
        await _context.SaveChangesAsync();
        return policy;
    }

    public async Task<CustomerCreditPolicy> UpdateAsync(CustomerCreditPolicy policy)
    {
        _context.CustomerCreditPolicies.Update(policy);
        await _context.SaveChangesAsync();
        return policy;
    }

    public async Task<bool> UpdateBalancesAsync(int customerId, decimal pendingAmount, decimal overdueAmount)
    {
        var policy = await GetByCustomerIdAsync(customerId);
        if (policy == null) return false;

        policy.TotalPendingAmount = pendingAmount;
        policy.TotalOverdueAmount = overdueAmount;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ValidateCreditAvailabilityAsync(int customerId, decimal amount)
    {
        var policy = await GetByCustomerIdAsync(customerId);
        if (policy == null) return false;

        var availableCredit = policy.CreditLimit - policy.TotalPendingAmount;
        return availableCredit >= amount;
    }

    public async Task<List<CustomerCreditPolicy>> GetBlockedCustomersAsync(int companyId)
    {
        return await _context.CustomerCreditPolicies
            .Include(c => c.Customer)
            .Where(c => c.CompanyId == companyId && c.Status == "Blocked")
            .ToListAsync();
    }

    public async Task<List<CustomerCreditPolicy>> GetCustomersNearLimitAsync(int companyId, decimal percentage = 0.9M)
    {
        return await _context.CustomerCreditPolicies
            .Include(c => c.Customer)
            .Where(c => c.CompanyId == companyId && 
                        c.CreditLimit > 0 &&
                        (c.TotalPendingAmount / c.CreditLimit) >= percentage)
            .ToListAsync();
    }
}
