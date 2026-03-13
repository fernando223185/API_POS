using Application.Abstractions.Purchasing;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PurchaseOrderRepository : IPurchaseOrderRepository
    {
        private readonly POSDbContext _context;

        public PurchaseOrderRepository(POSDbContext context)
        {
            _context = context;
        }

        public async Task<PurchaseOrder?> GetByIdAsync(int id)
        {
            return await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                    .ThenInclude(w => w.Branch)
                .Include(po => po.Details)
                    .ThenInclude(d => d.Product)
                .Include(po => po.CreatedBy)
                .Include(po => po.UpdatedBy)
                .FirstOrDefaultAsync(po => po.Id == id);
        }

        public async Task<PurchaseOrder?> GetByCodeAsync(string code)
        {
            return await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                    .ThenInclude(w => w.Branch)
                .Include(po => po.Details)
                    .ThenInclude(d => d.Product)
                .Include(po => po.CreatedBy)
                .Include(po => po.UpdatedBy)
                .FirstOrDefaultAsync(po => po.Code == code);
        }

        public async Task<List<PurchaseOrder>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                    .ThenInclude(w => w.Branch)
                .Include(po => po.Details)
                    .ThenInclude(d => d.Product)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(po => po.IsActive);
            }

            return await query
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PurchaseOrder>> GetBySupplierAsync(int supplierId, bool includeInactive = false)
        {
            var query = _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                    .ThenInclude(w => w.Branch)
                .Include(po => po.Details)
                    .ThenInclude(d => d.Product)
                .Include(po => po.CreatedBy)
                .Include(po => po.UpdatedBy)
                .Where(po => po.SupplierId == supplierId)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(po => po.IsActive);
            }

            return await query
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();
        }

        public async Task<List<PurchaseOrder>> GetByWarehouseAsync(int warehouseId, bool includeInactive = false)
        {
            var query = _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                    .ThenInclude(w => w.Branch)
                .Include(po => po.Details)
                    .ThenInclude(d => d.Product)
                .Include(po => po.CreatedBy)
                .Include(po => po.UpdatedBy)
                .Where(po => po.WarehouseId == warehouseId)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(po => po.IsActive);
            }

            return await query
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();
        }

        public async Task<List<PurchaseOrder>> GetByStatusAsync(string status)
        {
            return await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                    .ThenInclude(w => w.Branch)
                .Include(po => po.Details)
                    .ThenInclude(d => d.Product)
                .Include(po => po.CreatedBy)
                .Include(po => po.UpdatedBy)
                .Where(po => po.Status == status && po.IsActive)
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();
        }

        public async Task<List<PurchaseOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                    .ThenInclude(w => w.Branch)
                .Include(po => po.Details)
                    .ThenInclude(d => d.Product)
                .Include(po => po.CreatedBy)
                .Include(po => po.UpdatedBy)
                .Where(po => po.OrderDate >= startDate && po.OrderDate <= endDate && po.IsActive)
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();
        }

        public async Task<List<PurchaseOrder>> GetPendingToReceiveAsync()
        {
            return await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                    .ThenInclude(w => w.Branch)
                .Include(po => po.Details)
                    .ThenInclude(d => d.Product)
                .Include(po => po.CreatedBy)
                .Include(po => po.UpdatedBy)
                .Where(po => (po.Status == "Approved" || po.Status == "InTransit" || po.Status == "PartiallyReceived") 
                    && po.IsActive)
                .OrderBy(po => po.ExpectedDeliveryDate)
                .ToListAsync();
        }

        public async Task<List<PurchaseOrder>> GetPendingApprovalAsync()
        {
            return await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                    .ThenInclude(w => w.Branch)
                .Include(po => po.Details)
                    .ThenInclude(d => d.Product)
                .Include(po => po.CreatedBy)
                .Include(po => po.UpdatedBy)
                .Where(po => po.Status == "Pending" && po.IsActive)
                .OrderBy(po => po.OrderDate)
                .ToListAsync();
        }

        public async Task<(List<PurchaseOrder> orders, int totalRecords)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            bool includeInactive = false,
            string? searchTerm = null,
            string? status = null,
            int? supplierId = null,
            int? warehouseId = null)
        {
            var query = _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                    .ThenInclude(w => w.Branch)
                .Include(po => po.Details)
                    .ThenInclude(d => d.Product)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(po => po.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(po =>
                    po.Code.Contains(searchTerm) ||
                    po.Supplier.Name.Contains(searchTerm) ||
                    po.Warehouse.Name.Contains(searchTerm) ||
                    (po.SupplierReference != null && po.SupplierReference.Contains(searchTerm)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(po => po.Status == status);
            }

            if (supplierId.HasValue)
            {
                query = query.Where(po => po.SupplierId == supplierId.Value);
            }

            if (warehouseId.HasValue)
            {
                query = query.Where(po => po.WarehouseId == warehouseId.Value);
            }

            var totalRecords = await query.CountAsync();

            var orders = await query
                .OrderByDescending(po => po.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalRecords);
        }

        public async Task<PurchaseOrder> CreateAsync(PurchaseOrder purchaseOrder)
        {
            _context.PurchaseOrders.Add(purchaseOrder);
            await _context.SaveChangesAsync();

            // Recargar con navegación
            return await GetByIdAsync(purchaseOrder.Id) ?? purchaseOrder;
        }

        public async Task UpdateAsync(PurchaseOrder purchaseOrder)
        {
            purchaseOrder.UpdatedAt = DateTime.UtcNow;
            _context.PurchaseOrders.Update(purchaseOrder);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.PurchaseOrders.AnyAsync(po => po.Id == id);
        }

        public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
        {
            var query = _context.PurchaseOrders.Where(po => po.Code == code);

            if (excludeId.HasValue)
            {
                query = query.Where(po => po.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> SupplierExistsAsync(int supplierId)
        {
            return await _context.Suppliers.AnyAsync(s => s.Id == supplierId && s.IsActive);
        }

        public async Task<bool> WarehouseExistsAsync(int warehouseId)
        {
            return await _context.Warehouses.AnyAsync(w => w.Id == warehouseId && w.IsActive && w.AllowsReceiving);
        }

        public async Task<bool> ProductExistsAsync(int productId)
        {
            return await _context.Products.AnyAsync(p => p.ID == productId && p.IsActive);
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.PurchaseOrders.CountAsync(po => po.IsActive);
        }

        public async Task<int> GetCountByStatusAsync(string status)
        {
            return await _context.PurchaseOrders
                .CountAsync(po => po.Status == status && po.IsActive);
        }

        public async Task<decimal> GetTotalAmountBySupplierAsync(int supplierId, DateTime? startDate = null)
        {
            var query = _context.PurchaseOrders
                .Where(po => po.SupplierId == supplierId && po.IsActive);

            if (startDate.HasValue)
            {
                query = query.Where(po => po.OrderDate >= startDate.Value);
            }

            return await query.SumAsync(po => po.Total);
        }
    }
}
