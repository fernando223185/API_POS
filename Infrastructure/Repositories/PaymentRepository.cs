using Application.Abstractions.AccountsReceivable;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly POSDbContext _context;

    public PaymentRepository(POSDbContext context)
    {
        _context = context;
    }

    public async Task<Payment> CreateAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<Payment?> GetByIdAsync(int id)
    {
        return await _context.Payments
            .Include(p => p.PaymentApplications)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Payment?> GetByPaymentNumberAsync(string paymentNumber)
    {
        return await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PaymentNumber == paymentNumber);
    }

    public async Task<List<Payment>> GetByCustomerAsync(int customerId, int? limit = null)
    {
        var query = _context.Payments
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.PaymentDate)
            .AsNoTracking();

        if (limit.HasValue)
            query = query.Take(limit.Value);

        return await query.ToListAsync();
    }

    public async Task<List<Payment>> GetByBatchIdAsync(int batchId)
    {
        return await _context.Payments
            .Where(p => p.BatchId == batchId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Payment> UpdateAsync(Payment payment)
    {
        _context.Entry(payment).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<string> GeneratePaymentNumberAsync(string prefix = "PAG")
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Payments
            .Where(p => p.PaymentNumber.StartsWith($"{prefix}-{year}-"))
            .CountAsync();
        return $"{prefix}-{year}-{(count + 1):D4}";
    }

    public async Task<(List<Payment> items, int totalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        int? customerId = null,
        int? companyId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        bool? hasComplement = null,
        string? searchTerm = null)
    {
        var query = _context.Payments
            .Include(p => p.PaymentApplications)
            .AsNoTracking()
            .AsQueryable();

        // Aplicar filtros
        if (customerId.HasValue)
        {
            query = query.Where(p => p.CustomerId == customerId.Value);
        }

        if (companyId.HasValue)
        {
            query = query.Where(p => p.CompanyId == companyId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(p => p.Status == status);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate <= toDate.Value);
        }

        if (hasComplement.HasValue)
        {
            if (hasComplement.Value)
            {
                query = query.Where(p => p.Uuid != null);
            }
            else
            {
                query = query.Where(p => p.Uuid == null);
            }
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var searchUpper = searchTerm.ToUpper();
            query = query.Where(p =>
                p.PaymentNumber.ToUpper().Contains(searchUpper) ||
                (p.Reference != null && p.Reference.ToUpper().Contains(searchUpper)) ||
                p.CustomerName.ToUpper().Contains(searchUpper)
            );
        }

        // Total de registros
        var totalCount = await query.CountAsync();

        // Paginación
        var items = await query
            .OrderByDescending(p => p.PaymentDate)
            .ThenByDescending(p => p.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
