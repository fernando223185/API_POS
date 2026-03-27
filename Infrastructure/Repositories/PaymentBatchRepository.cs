using Application.Abstractions.AccountsReceivable;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de Lotes de Pago
/// </summary>
public class PaymentBatchRepository : IPaymentBatchRepository
{
    private readonly POSDbContext _context;

    public PaymentBatchRepository(POSDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentBatch> CreateAsync(PaymentBatch batch)
    {
        await _context.PaymentBatches.AddAsync(batch);
        await _context.SaveChangesAsync();
        return batch;
    }

    public async Task<PaymentBatch?> GetByIdAsync(int id)
    {
        return await _context.PaymentBatches
            .Include(b => b.Company)
            .Include(b => b.Payments)
                .ThenInclude(p => p.PaymentApplications)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<PaymentBatch?> GetByBatchNumberAsync(string batchNumber)
    {
        return await _context.PaymentBatches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BatchNumber == batchNumber);
    }

    public async Task<bool> ExistsByBatchNumberAsync(string batchNumber)
    {
        return await _context.PaymentBatches
            .AnyAsync(b => b.BatchNumber == batchNumber);
    }

    public async Task<PaymentBatch> UpdateAsync(PaymentBatch batch)
    {
        _context.PaymentBatches.Update(batch);
        await _context.SaveChangesAsync();
        return batch;
    }

    public async Task<string> GenerateBatchNumberAsync(string companyCode, DateTime paymentDate, string prefix = "BTCP")
    {
        // Formato: BTCP-COMP001-260326-001
        var dateStr = paymentDate.ToString("ddMMyy"); // Ejemplo: 260326
        var searchPattern = $"{prefix}-{companyCode}-{dateStr}-";
        
        var lastBatch = await _context.PaymentBatches
            .Where(b => b.BatchNumber.StartsWith(searchPattern))
            .OrderByDescending(b => b.BatchNumber)
            .FirstOrDefaultAsync();

        if (lastBatch == null)
            return $"{searchPattern}001";

        var lastNumber = int.Parse(lastBatch.BatchNumber.Split('-').Last());
        return $"{searchPattern}{(lastNumber + 1):D3}";
    }

    public async Task<List<PaymentBatch>> GetRecentBatchesAsync(int companyId, int limit = 10)
    {
        return await _context.PaymentBatches
            .Where(b => b.CompanyId == companyId)
            .OrderByDescending(b => b.CreatedAt)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<(List<PaymentBatch> items, int totalCount)> GetPagedAsync(
        int companyId,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null,
        bool? hasErrors = null,
        int pageNumber = 1,
        int pageSize = 20,
        string orderBy = "CreatedAt",
        bool ascending = false)
    {
        var query = _context.PaymentBatches
            .Include(b => b.Company)
            .Where(b => b.CompanyId == companyId);

        // Filtros
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(b => b.Status == status);

        if (fromDate.HasValue)
            query = query.Where(b => b.PaymentDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(b => b.PaymentDate <= toDate.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(b => 
                b.BatchNumber.Contains(searchTerm) ||
                (b.Description != null && b.Description.Contains(searchTerm)) ||
                (b.Notes != null && b.Notes.Contains(searchTerm)));
        }

        if (hasErrors.HasValue && hasErrors.Value)
            query = query.Where(b => b.ComplementsWithError > 0);

        // Total de registros
        var totalCount = await query.CountAsync();

        // Ordenamiento
        query = orderBy.ToLower() switch
        {
            "batchnumber" => ascending ? query.OrderBy(b => b.BatchNumber) : query.OrderByDescending(b => b.BatchNumber),
            "paymentdate" => ascending ? query.OrderBy(b => b.PaymentDate) : query.OrderByDescending(b => b.PaymentDate),
            "status" => ascending ? query.OrderBy(b => b.Status) : query.OrderByDescending(b => b.Status),
            "totalamount" => ascending ? query.OrderBy(b => b.TotalAmount) : query.OrderByDescending(b => b.TotalAmount),
            "totalpayments" => ascending ? query.OrderBy(b => b.TotalPayments) : query.OrderByDescending(b => b.TotalPayments),
            "completedat" => ascending ? query.OrderBy(b => b.CompletedAt) : query.OrderByDescending(b => b.CompletedAt),
            _ => ascending ? query.OrderBy(b => b.CreatedAt) : query.OrderByDescending(b => b.CreatedAt)
        };

        // Paginación
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return (items, totalCount);
    }
}
