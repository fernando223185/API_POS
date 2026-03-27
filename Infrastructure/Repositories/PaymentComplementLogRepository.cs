using Application.Abstractions.AccountsReceivable;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PaymentComplementLogRepository : IPaymentComplementLogRepository
{
    private readonly POSDbContext _context;

    public PaymentComplementLogRepository(POSDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentComplementLog> CreateAsync(PaymentComplementLog log)
    {
        await _context.PaymentComplementLogs.AddAsync(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task<List<PaymentComplementLog>> GetByPaymentApplicationIdAsync(int paymentApplicationId)
    {
        return await _context.PaymentComplementLogs
            .Where(l => l.PaymentApplicationId == paymentApplicationId)
            .OrderByDescending(l => l.AttemptDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<PaymentComplementLog>> GetByPaymentIdAsync(int paymentId)
    {
        return await _context.PaymentComplementLogs
            .Where(l => l.PaymentId == paymentId)
            .OrderByDescending(l => l.AttemptDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<PaymentComplementLog>> GetErrorLogsAsync(int? batchId = null, int? limit = null)
    {
        var query = _context.PaymentComplementLogs
            .Where(l => l.Status == "Failed")
            .OrderByDescending(l => l.AttemptDate)
            .AsNoTracking();

        if (batchId.HasValue)
            query = query.Where(l => l.BatchId == batchId.Value);

        if (limit.HasValue)
            query = query.Take(limit.Value);

        return await query.ToListAsync();
    }
}
