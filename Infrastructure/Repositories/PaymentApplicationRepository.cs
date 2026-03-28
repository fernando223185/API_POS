using Application.Abstractions.AccountsReceivable;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PaymentApplicationRepository : IPaymentApplicationRepository
{
    private readonly POSDbContext _context;

    public PaymentApplicationRepository(POSDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentApplication> CreateAsync(PaymentApplication application)
    {
        await _context.PaymentApplications.AddAsync(application);
        await _context.SaveChangesAsync();
        return application;
    }

    public async Task<PaymentApplication?> GetByIdAsync(int id)
    {
        return await _context.PaymentApplications
            .Include(a => a.Payment)
            .Include(a => a.Invoice)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<PaymentApplication>> GetByPaymentIdAsync(int paymentId)
    {
        return await _context.PaymentApplications
            .Where(a => a.PaymentId == paymentId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<PaymentApplication>> GetByInvoiceIdAsync(int invoiceId)
    {
        return await _context.PaymentApplications
            .Where(a => a.InvoiceId == invoiceId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<PaymentApplication> UpdateAsync(PaymentApplication application)
    {
        _context.Entry(application).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return application;
    }

    /// <summary>
    /// Cuenta las aplicaciones pendientes de un pago
    /// NOTA: El estado del complemento ahora está en Payment, no en PaymentApplication
    /// </summary>
    public async Task<int> CountPendingComplementsAsync(int paymentId)
    {
        // Ya no hay estado de complemento por aplicación, ahora es por Payment
        // Simplemente contamos las aplicaciones del pago
        return await _context.PaymentApplications
            .CountAsync(a => a.PaymentId == paymentId);
    }
}
