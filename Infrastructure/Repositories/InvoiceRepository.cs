using Application.Abstractions.Billing;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly POSDbContext _context;

        public InvoiceRepository(POSDbContext context)
        {
            _context = context;
        }

        public async Task<Invoice> CreateAsync(Invoice invoice)
        {
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task<Invoice?> GetByIdAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.Details)
                    .ThenInclude(d => d.Product)
                .Include(i => i.Sale)
                .Include(i => i.Company)
                .Include(i => i.Customer)
                .Include(i => i.CreatedBy)
                .Include(i => i.CancelledBy)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Invoice?> GetByUuidAsync(string uuid)
        {
            return await _context.Invoices
                .Include(i => i.Details)
                .Include(i => i.Sale)
                .Include(i => i.Company)
                .Include(i => i.Customer)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Uuid == uuid);
        }

        public async Task<Invoice?> GetBySerieAndFolioAsync(string serie, string folio)
        {
            return await _context.Invoices
                .Include(i => i.Details)
                .Include(i => i.Sale)
                .Include(i => i.Company)
                .Include(i => i.Customer)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Serie == serie && i.Folio == folio);
        }

        public async Task<IEnumerable<Invoice>> GetBySaleIdAsync(int saleId)
        {
            return await _context.Invoices
                .Include(i => i.Details)
                .Include(i => i.Company)
                .Include(i => i.Customer)
                .Where(i => i.SaleId == saleId)
                .OrderByDescending(i => i.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Invoice> UpdateAsync(Invoice invoice)
        {
            _context.Entry(invoice).State = EntityState.Modified;

            // No modificar las propiedades de navegación (solo escalares de la raíz)
            _context.Entry(invoice).Collection(i => i.Details).IsModified = false;
            foreach (var detail in invoice.Details)
                _context.Entry(detail).State = EntityState.Detached;

            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task ReplaceDetailsAsync(int invoiceId, List<InvoiceDetail> newDetails)
        {
            var existing = await _context.InvoiceDetails
                .Where(d => d.InvoiceId == invoiceId)
                .ToListAsync();

            _context.InvoiceDetails.RemoveRange(existing);

            foreach (var detail in newDetails)
            {
                detail.InvoiceId = invoiceId;
                detail.Id = 0; // asegurar que EF lo trate como nuevo
            }

            await _context.InvoiceDetails.AddRangeAsync(newDetails);
            await _context.SaveChangesAsync();
        }

        public async Task<(IEnumerable<Invoice> Invoices, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            int? companyId = null,
            int? customerId = null,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? serie = null,
            string? rfc = null)
        {
            var query = _context.Invoices
                .Include(i => i.Company)
                .Include(i => i.Customer)
                .Include(i => i.Sale)
                .AsQueryable();

            // Filtros
            if (companyId.HasValue)
                query = query.Where(i => i.CompanyId == companyId.Value);

            if (customerId.HasValue)
                query = query.Where(i => i.CustomerId == customerId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(i => i.Status == status);

            if (fromDate.HasValue)
                query = query.Where(i => i.InvoiceDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(i => i.InvoiceDate <= toDate.Value);

            if (!string.IsNullOrWhiteSpace(serie))
                query = query.Where(i => i.Serie == serie);

            if (!string.IsNullOrWhiteSpace(rfc))
                query = query.Where(i => i.ReceptorRfc == rfc || i.EmisorRfc == rfc);

            // Total count
            var totalCount = await query.CountAsync();

            // Paginación
            var invoices = await query
                .OrderByDescending(i => i.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (invoices, totalCount);
        }

        public async Task<IEnumerable<Invoice>> GetDraftsAsync(int? companyId = null)
        {
            var query = _context.Invoices
                .Include(i => i.Details)
                .Include(i => i.Company)
                .Include(i => i.Customer)
                .Include(i => i.Sale)
                .Where(i => i.Status == "Borrador");

            if (companyId.HasValue)
                query = query.Where(i => i.CompanyId == companyId.Value);

            return await query
                .OrderByDescending(i => i.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> ExistsBySerieAndFolioAsync(string serie, string folio, int? excludeInvoiceId = null)
        {
            var query = _context.Invoices
                .Where(i => i.Serie == serie && i.Folio == folio);

            if (excludeInvoiceId.HasValue)
                query = query.Where(i => i.Id != excludeInvoiceId.Value);

            return await query.AnyAsync();
        }

        public async Task<string> GetNextFolioAsync(int companyId, string serie)
        {
            // Buscar el último folio usado para esta empresa y serie
            var lastInvoice = await _context.Invoices
                .Where(i => i.CompanyId == companyId && i.Serie == serie)
                .OrderByDescending(i => i.Id)
                .FirstOrDefaultAsync();

            if (lastInvoice == null)
            {
                // Primera factura de esta serie
                return "1";
            }

            // Intentar parsear el folio como número y sumar 1
            if (int.TryParse(lastInvoice.Folio, out int folioNumber))
            {
                return (folioNumber + 1).ToString();
            }

            // Si no es numérico, retornar un valor por defecto
            // Alternativamente, buscar en la tabla Company el CurrentFolio
            var company = await _context.Companies.FindAsync(companyId);
            if (company != null && company.InvoiceCurrentFolio > 0)
            {
                return company.InvoiceCurrentFolio.ToString();
            }

            return "1";
        }

        public async Task<Invoice> CancelAsync(int invoiceId, int userId, string cancellationReason)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Details)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null)
            {
                throw new KeyNotFoundException($"Factura con ID {invoiceId} no encontrada");
            }

            if (invoice.Status != "Timbrada")
            {
                throw new InvalidOperationException("Solo se pueden cancelar facturas timbradas");
            }

            invoice.Status = "Cancelada";
            invoice.CancelledAt = DateTime.UtcNow;
            invoice.CancelledByUserId = userId;
            invoice.CancellationReason = cancellationReason;
            invoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return invoice;
        }

        public async Task<(int totalInvoices, decimal totalAmount, int stampedInvoices, int pendingInvoices, int cancelledInvoices)> GetSummaryAsync(int year, int month)
        {
            // Calcular rango de fechas para el mes especificado
            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1);

            var invoices = await _context.Invoices
                .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate < endDate)
                .ToListAsync();

            var totalInvoices = invoices.Count;
            var totalAmount = invoices.Sum(i => i.Total);
            var stampedInvoices = invoices.Count(i => i.Status == "Timbrada");
            var pendingInvoices = invoices.Count(i => i.Status == "Borrador");
            var cancelledInvoices = invoices.Count(i => i.Status == "Cancelada");

            return (totalInvoices, totalAmount, stampedInvoices, pendingInvoices, cancelledInvoices);
        }
    }
}
