using Application.Abstractions.Sales;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class SaleRepository : ISaleRepository
    {
        private readonly POSDbContext _context;

        public SaleRepository(POSDbContext context)
        {
            _context = context;
        }

        public async Task<Sale> CreateAsync(Sale sale)
        {
            _context.SalesNew.Add(sale);
            await _context.SaveChangesAsync();
            return sale;
        }

        public async Task<Sale?> GetByIdAsync(int id)
        {
            return await _context.SalesNew
                .Include(s => s.Details)
                    .ThenInclude(d => d.Product)
                .Include(s => s.Payments)
                .Include(s => s.Customer)
                .Include(s => s.Warehouse)
                    .ThenInclude(w => w.Branch)
                .Include(s => s.User)
                .Include(s => s.PriceList)
                .Include(s => s.CreatedBy)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Sale?> GetByCodeAsync(string code)
        {
            return await _context.SalesNew
                .Include(s => s.Details)
                    .ThenInclude(d => d.Product)
                .Include(s => s.Payments)
                .Include(s => s.Customer)
                .Include(s => s.Warehouse)
                .Include(s => s.User)
                .Include(s => s.PriceList)
                .FirstOrDefaultAsync(s => s.Code == code);
        }

        public async Task<Sale> UpdateAsync(Sale sale)
        {
            _context.SalesNew.Update(sale);
            await _context.SaveChangesAsync();
            return sale;
        }

        public async Task SetInvoiceIdAsync(int saleId, int invoiceId)
        {
            var sale = await _context.SalesNew.FindAsync(saleId);
            if (sale != null)
            {
                sale.InvoiceId = invoiceId;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetInvoiceIdBulkAsync(IEnumerable<int> saleIds, int invoiceId)
        {
            var ids = saleIds.ToList();
            if (!ids.Any()) return;

            var sales = await _context.SalesNew
                .Where(s => ids.Contains(s.Id))
                .ToListAsync();

            foreach (var sale in sales)
                sale.InvoiceId = invoiceId;

            await _context.SaveChangesAsync();
        }

        public async Task<(IEnumerable<Sale> Sales, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            int? warehouseId = null,
            int? customerId = null,
            int? userId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? status = null,
            bool? isPaid = null,
            bool? requiresInvoice = null)
        {
            var query = _context.SalesNew
                .Include(s => s.Customer)
                .Include(s => s.Warehouse)
                .Include(s => s.User)
                .Include(s => s.Details)
                .Include(s => s.Payments)
                .AsQueryable();

            // Filtros
            if (warehouseId.HasValue)
                query = query.Where(s => s.WarehouseId == warehouseId.Value);

            if (customerId.HasValue)
                query = query.Where(s => s.CustomerId == customerId.Value);

            if (userId.HasValue)
                query = query.Where(s => s.UserId == userId.Value);

            if (fromDate.HasValue)
                query = query.Where(s => s.SaleDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(s => s.SaleDate <= toDate.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(s => s.Status == status);

            if (isPaid.HasValue)
                query = query.Where(s => s.IsPaid == isPaid.Value);

            if (requiresInvoice.HasValue)
                query = query.Where(s => s.RequiresInvoice == requiresInvoice.Value);

            // Obtener total
            var totalCount = await query.CountAsync();

            // Ordenar y paginar
            var sales = await query
                .OrderByDescending(s => s.SaleDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (sales, totalCount);
        }

        public async Task<(int Total, int Completed, int Cancelled, int Draft, decimal TotalRevenue, decimal TotalCost)> GetStatisticsAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? warehouseId = null)
        {
            var query = _context.SalesNew.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(s => s.SaleDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(s => s.SaleDate <= toDate.Value);

            if (warehouseId.HasValue)
                query = query.Where(s => s.WarehouseId == warehouseId.Value);

            var total = await query.CountAsync();
            var completed = await query.CountAsync(s => s.Status == "Completed");
            var cancelled = await query.CountAsync(s => s.Status == "Cancelled");
            var draft = await query.CountAsync(s => s.Status == "Draft");

            var totalRevenue = await query
                .Where(s => s.Status == "Completed")
                .SumAsync(s => s.Total);

            var totalCost = await query
                .Where(s => s.Status == "Completed")
                .Include(s => s.Details)
                .SelectMany(s => s.Details)
                .SumAsync(d => d.TotalCost ?? 0);

            return (total, completed, cancelled, draft, totalRevenue, totalCost);
        }

        public async Task<bool> ExistsByCodeAsync(string code)
        {
            return await _context.SalesNew.AnyAsync(s => s.Code == code);
        }

        public async Task<(IEnumerable<Sale> Sales, int TotalCount)> GetPendingInvoiceSalesAsync(
            int page,
            int pageSize,
            bool? onlyRequiresInvoice = null,
            int? warehouseId = null,
            int? branchId = null,
            int? companyId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _context.SalesNew
                .Include(s => s.Customer)
                .Include(s => s.Warehouse)
                    .ThenInclude(w => w.Branch)
                        .ThenInclude(b => b.Company)
                .Include(s => s.Branch)
                .Include(s => s.Company)
                .Include(s => s.User)
                .Include(s => s.Details)
                .AsQueryable();

            // Filtro principal: Ventas completadas, pagadas y sin UUID (no timbradas)
            query = query.Where(s => 
                s.Status == "Completed" && 
                s.IsPaid == true && 
                string.IsNullOrEmpty(s.InvoiceUuid));

            // Filtro por RequiresInvoice
            if (onlyRequiresInvoice.HasValue)
            {
                query = query.Where(s => s.RequiresInvoice == onlyRequiresInvoice.Value);
            }

            // Filtros opcionales
            if (warehouseId.HasValue)
                query = query.Where(s => s.WarehouseId == warehouseId.Value);

            if (branchId.HasValue)
                query = query.Where(s => s.BranchId == branchId.Value);

            if (companyId.HasValue)
                query = query.Where(s => s.CompanyId == companyId.Value);

            if (fromDate.HasValue)
                query = query.Where(s => s.SaleDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(s => s.SaleDate <= toDate.Value);

            // Obtener total
            var totalCount = await query.CountAsync();

            // Ordenar y paginar (m�s recientes primero)
            var sales = await query
                .OrderByDescending(s => s.SaleDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (sales, totalCount);
        }

        public async Task<Sale?> GetSaleForInvoicingAsync(int saleId)
        {
            return await _context.SalesNew
                // Relaci�n con empresa (CR�TICO para facturaci�n)
                .Include(s => s.Company)
                
                // Relaci�n con sucursal
                .Include(s => s.Branch)
                
                // Relaci�n con cliente (CR�TICO para facturaci�n)
                .Include(s => s.Customer)
                
                // Relaci�n con almac�n y su sucursal
                .Include(s => s.Warehouse)
                    .ThenInclude(w => w.Branch)
                        .ThenInclude(b => b!.Company)
                
                // Detalles de venta con productos (para claves SAT)
                .Include(s => s.Details)
                    .ThenInclude(d => d.Product)
                
                // Pagos de la venta
                .Include(s => s.Payments)
                
                // Usuario que cre� la venta
                .Include(s => s.User)
                
                // Lista de precios aplicada
                .Include(s => s.PriceList)
                
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == saleId);
        }
    }
}
