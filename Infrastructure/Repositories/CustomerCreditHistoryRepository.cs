using Application.Abstractions.AccountsReceivable;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CustomerCreditHistoryRepository : ICustomerCreditHistoryRepository
{
    private readonly POSDbContext _context;

    public CustomerCreditHistoryRepository(POSDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerCreditHistory> CreateAsync(CustomerCreditHistory history)
    {
        _context.CustomerCreditHistory.Add(history);
        await _context.SaveChangesAsync();
        return history;
    }

    public async Task<List<CustomerCreditHistory>> GetByCustomerIdAsync(int customerId, int? limit = null)
    {
        var query = _context.CustomerCreditHistory
            .Where(h => h.CustomerId == customerId)
            .OrderByDescending(h => h.EventDate);

        if (limit.HasValue)
        {
            query = (IOrderedQueryable<CustomerCreditHistory>)query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<List<CustomerCreditHistory>> GetByEventTypeAsync(int customerId, string eventType, int? limit = null)
    {
        var query = _context.CustomerCreditHistory
            .Where(h => h.CustomerId == customerId && h.EventType == eventType)
            .OrderByDescending(h => h.EventDate);

        if (limit.HasValue)
        {
            query = (IOrderedQueryable<CustomerCreditHistory>)query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }
}
