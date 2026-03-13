using Application.Abstractions.Purchasing;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PurchaseOrderReceivingRepository : IPurchaseOrderReceivingRepository
    {
        private readonly POSDbContext _context;

        public PurchaseOrderReceivingRepository(POSDbContext context)
        {
            _context = context;
        }

        public async Task<PurchaseOrderReceiving?> GetByIdAsync(int id)
        {
            return await _context.PurchaseOrderReceivings
                .Include(r => r.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
                .Include(r => r.PurchaseOrder)
                    .ThenInclude(po => po.Warehouse)
                .Include(r => r.Warehouse)
                .Include(r => r.Details)
                    .ThenInclude(d => d.Product)
                .Include(r => r.Details)
                    .ThenInclude(d => d.PurchaseOrderDetail)
                .Include(r => r.CreatedBy)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<PurchaseOrderReceiving?> GetByCodeAsync(string code)
        {
            return await _context.PurchaseOrderReceivings
                .Include(r => r.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
                .Include(r => r.PurchaseOrder)
                    .ThenInclude(po => po.Warehouse)
                .Include(r => r.Warehouse)
                .Include(r => r.Details)
                    .ThenInclude(d => d.Product)
                .Include(r => r.CreatedBy)
                .FirstOrDefaultAsync(r => r.Code == code);
        }

        public async Task<List<PurchaseOrderReceiving>> GetAllAsync(bool includePosted = true)
        {
            var query = _context.PurchaseOrderReceivings
                .Include(r => r.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
                .Include(r => r.Warehouse)
                .Include(r => r.Details)
                    .ThenInclude(d => d.Product)
                .Include(r => r.Details)
                    .ThenInclude(d => d.PurchaseOrderDetail)
                .Include(r => r.CreatedBy)
                .AsQueryable();

            if (!includePosted)
            {
                query = query.Where(r => !r.IsPostedToInventory);
            }

            return await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PurchaseOrderReceiving>> GetByPurchaseOrderAsync(int purchaseOrderId)
        {
            return await _context.PurchaseOrderReceivings
                .Include(r => r.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
                .Include(r => r.Warehouse)
                .Include(r => r.Details)
                    .ThenInclude(d => d.Product)
                .Include(r => r.Details)
                    .ThenInclude(d => d.PurchaseOrderDetail)
                .Include(r => r.CreatedBy)
                .Where(r => r.PurchaseOrderId == purchaseOrderId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PurchaseOrderReceiving>> GetByWarehouseAsync(int warehouseId)
        {
            return await _context.PurchaseOrderReceivings
                .Include(r => r.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
                .Include(r => r.Warehouse)
                .Include(r => r.Details)
                    .ThenInclude(d => d.Product)
                .Include(r => r.Details)
                    .ThenInclude(d => d.PurchaseOrderDetail)
                .Include(r => r.CreatedBy)
                .Where(r => r.WarehouseId == warehouseId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PurchaseOrderReceiving>> GetPendingToPostAsync()
        {
            return await _context.PurchaseOrderReceivings
                .Include(r => r.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
                .Include(r => r.Warehouse)
                .Include(r => r.Details)
                    .ThenInclude(d => d.Product)
                .Include(r => r.Details)
                    .ThenInclude(d => d.PurchaseOrderDetail)
                .Include(r => r.CreatedBy)
                .Where(r => !r.IsPostedToInventory && r.Status == "Received")
                .OrderBy(r => r.ReceivingDate)
                .ToListAsync();
        }

        public async Task<(List<PurchaseOrderReceiving> receivings, int totalRecords)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            int? purchaseOrderId = null,
            int? warehouseId = null,
            string? status = null,
            bool? onlyPendingToPost = null)
        {
            var query = _context.PurchaseOrderReceivings
                .Include(r => r.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
                .Include(r => r.Warehouse)
                .Include(r => r.Details)
                    .ThenInclude(d => d.Product)
                .Include(r => r.Details)
                    .ThenInclude(d => d.PurchaseOrderDetail)
                .Include(r => r.CreatedBy)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(r =>
                    r.Code.Contains(searchTerm) ||
                    r.PurchaseOrder.Code.Contains(searchTerm) ||
                    r.PurchaseOrder.Supplier.Name.Contains(searchTerm) ||
                    (r.SupplierInvoiceNumber != null && r.SupplierInvoiceNumber.Contains(searchTerm)));
            }

            if (purchaseOrderId.HasValue)
            {
                query = query.Where(r => r.PurchaseOrderId == purchaseOrderId.Value);
            }

            if (warehouseId.HasValue)
            {
                query = query.Where(r => r.WarehouseId == warehouseId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.Status == status);
            }

            if (onlyPendingToPost == true)
            {
                query = query.Where(r => !r.IsPostedToInventory);
            }

            var totalRecords = await query.CountAsync();

            var receivings = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (receivings, totalRecords);
        }

        public async Task<PurchaseOrderReceiving> CreateAsync(PurchaseOrderReceiving receiving)
        {
            _context.PurchaseOrderReceivings.Add(receiving);
            await _context.SaveChangesAsync();
            
            // Recargar con relaciones
            return await GetByIdAsync(receiving.Id) ?? receiving;
        }

        public async Task UpdateAsync(PurchaseOrderReceiving receiving)
        {
            _context.PurchaseOrderReceivings.Update(receiving);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.PurchaseOrderReceivings.AnyAsync(r => r.Id == id);
        }

        public async Task<bool> PurchaseOrderExistsAsync(int purchaseOrderId)
        {
            return await _context.PurchaseOrders.AnyAsync(po => po.Id == purchaseOrderId && po.IsActive);
        }

        public async Task<bool> CanReceiveAsync(int purchaseOrderId)
        {
            var order = await _context.PurchaseOrders.FindAsync(purchaseOrderId);
            if (order == null) return false;

            var validStatuses = new[] { "Approved", "InTransit", "PartiallyReceived" };
            return validStatuses.Contains(order.Status) && order.IsActive;
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.PurchaseOrderReceivings.CountAsync();
        }

        public async Task<int> GetPendingToPostCountAsync()
        {
            return await _context.PurchaseOrderReceivings
                .CountAsync(r => !r.IsPostedToInventory && r.Status == "Received");
        }
    }
}
