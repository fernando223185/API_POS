using Application.Abstractions.AccountsReceivable;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de Facturas PPD
/// </summary>
public class InvoicePPDRepository : IInvoicePPDRepository
{
    private readonly POSDbContext _context;

    public InvoicePPDRepository(POSDbContext context)
    {
        _context = context;
    }

    public async Task<InvoicePPD> CreateAsync(InvoicePPD invoice)
    {
        await _context.InvoicesPPD.AddAsync(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<InvoicePPD?> GetByIdAsync(int id)
    {
        return await _context.InvoicesPPD
            .Include(i => i.Customer)
            .Include(i => i.PaymentApplications)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<InvoicePPD?> GetByUUIDAsync(string folioUUID)
    {
        return await _context.InvoicesPPD
            .FirstOrDefaultAsync(i => i.FolioUUID == folioUUID);
    }

    public async Task<InvoicePPD?> GetByInvoiceIdAsync(int invoiceId)
    {
        return await _context.InvoicesPPD
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
    }

    public async Task<List<InvoicePPD>> GetPendingByCustomerAsync(int customerId)
    {
        return await _context.InvoicesPPD
            .Where(i => i.CustomerId == customerId 
                     && i.IsActive 
                     && i.BalanceAmount > 0)
            .OrderBy(i => i.DueDate)
            .ToListAsync();
    }

    public async Task<List<InvoicePPD>> GetOverdueAsync(int companyId, int? minDaysOverdue = null)
    {
        var query = _context.InvoicesPPD
            .Where(i => i.CompanyId == companyId 
                     && i.IsActive 
                     && i.BalanceAmount > 0
                     && i.DueDate < DateTime.UtcNow);

        if (minDaysOverdue.HasValue)
        {
            var targetDate = DateTime.UtcNow.AddDays(-minDaysOverdue.Value);
            query = query.Where(i => i.DueDate <= targetDate);
        }

        return await query
            .Include(i => i.Customer)
            .OrderBy(i => i.DueDate)
            .ToListAsync();
    }

    public async Task<(List<InvoicePPD> items, int totalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        int? customerId = null,
        int? companyId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? minDaysOverdue = null,
        decimal? minAmount = null,
        string? searchTerm = null)
    {
        var query = _context.InvoicesPPD
            .Where(i => i.IsActive)
            .AsQueryable();

        // Aplicar filtros
        if (customerId.HasValue)
            query = query.Where(i => i.CustomerId == customerId.Value);

        if (companyId.HasValue)
            query = query.Where(i => i.CompanyId == companyId.Value);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(i => i.Status == status);

        if (fromDate.HasValue)
            query = query.Where(i => i.InvoiceDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(i => i.InvoiceDate <= toDate.Value);

        if (minDaysOverdue.HasValue)
        {
            var targetDate = DateTime.UtcNow.AddDays(-minDaysOverdue.Value);
            query = query.Where(i => i.DueDate <= targetDate);
        }

        if (minAmount.HasValue)
            query = query.Where(i => i.BalanceAmount >= minAmount.Value);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(i => 
                i.FolioUUID.ToLower().Contains(searchTerm) ||
                i.SerieAndFolio.ToLower().Contains(searchTerm) ||
                i.CustomerName.ToLower().Contains(searchTerm));
        }

        // Obtener total count
        var totalCount = await query.CountAsync();

        // Aplicar paginación
        var items = await query
            .Include(i => i.Customer)
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<InvoicePPD> UpdateAsync(InvoicePPD invoice)
    {
        invoice.UpdatedAt = DateTime.UtcNow;
        _context.InvoicesPPD.Update(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        var invoice = await _context.InvoicesPPD.FindAsync(id);
        if (invoice == null)
            return false;

        invoice.Status = status;
        invoice.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> GetTotalPendingByCustomerAsync(int customerId)
    {
        return await _context.InvoicesPPD
            .Where(i => i.CustomerId == customerId && i.IsActive && i.BalanceAmount > 0)
            .SumAsync(i => i.BalanceAmount);
    }

    public async Task<decimal> GetTotalOverdueByCustomerAsync(int customerId)
    {
        return await _context.InvoicesPPD
            .Where(i => i.CustomerId == customerId 
                     && i.IsActive 
                     && i.BalanceAmount > 0
                     && i.DueDate < DateTime.UtcNow)
            .SumAsync(i => i.BalanceAmount);
    }

    public async Task<Dictionary<string, decimal>> GetAgingReportAsync(int companyId)
    {
        var query = _context.InvoicesPPD
            .Where(i => i.CompanyId == companyId 
                     && i.IsActive 
                     && i.BalanceAmount > 0);

        var now = DateTime.UtcNow;
        var invoices = await query.ToListAsync();

        var aging = new Dictionary<string, decimal>
        {
            ["Current"] = invoices
                .Where(i => (now - i.DueDate).Days <= 30)
                .Sum(i => i.BalanceAmount),

            ["Days31To60"] = invoices
                .Where(i => (now - i.DueDate).Days > 30 && (now - i.DueDate).Days <= 60)
                .Sum(i => i.BalanceAmount),

            ["Days61To90"] = invoices
                .Where(i => (now - i.DueDate).Days > 60 && (now - i.DueDate).Days <= 90)
                .Sum(i => i.BalanceAmount),

            ["Over90Days"] = invoices
                .Where(i => (now - i.DueDate).Days > 90)
                .Sum(i => i.BalanceAmount)
        };

        return aging;
    }

    public async Task<(decimal pendingAmount, decimal overdueAmount)> GetCustomerBalanceSummaryAsync(int customerId, int companyId)
    {
        var today = DateTime.Today;
        
        var invoices = await _context.InvoicesPPD
            .Where(i => i.CustomerId == customerId && i.CompanyId == companyId && i.Status == "Pending")
            .AsNoTracking()
            .ToListAsync();

        var pendingAmount = invoices.Sum(i => i.BalanceAmount);
        var overdueAmount = invoices.Where(i => i.DueDate < today).Sum(i => i.BalanceAmount);

        return (pendingAmount, overdueAmount);
    }
}
