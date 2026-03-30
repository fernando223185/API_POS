using Application.Abstractions.Inventory;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class StockTransferRepository : IStockTransferRepository
    {
        private readonly POSDbContext _context;

        public StockTransferRepository(POSDbContext context)
        {
            _context = context;
        }

        public async Task<StockTransfer?> GetByIdAsync(int id)
        {
            return await _context.StockTransfers
                .Include(t => t.SourceWarehouse)
                .Include(t => t.DestinationWarehouse)
                .Include(t => t.CreatedBy)
                .Include(t => t.AppliedBy)
                .Include(t => t.Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<StockTransfer?> GetByCodeAsync(string code)
        {
            return await _context.StockTransfers
                .Include(t => t.SourceWarehouse)
                .Include(t => t.DestinationWarehouse)
                .Include(t => t.CreatedBy)
                .Include(t => t.AppliedBy)
                .Include(t => t.Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(t => t.Code == code);
        }

        public async Task<(List<StockTransfer> transfers, int totalRecords)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            int? sourceWarehouseId = null,
            int? destinationWarehouseId = null,
            string? status = null,
            int? companyId = null)
        {
            var query = _context.StockTransfers
                .AsNoTracking()
                .Include(t => t.SourceWarehouse)
                .Include(t => t.DestinationWarehouse)
                .Include(t => t.CreatedBy)
                .Include(t => t.Details)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(t =>
                    t.Code.Contains(searchTerm) ||
                    (t.Notes != null && t.Notes.Contains(searchTerm)));

            if (sourceWarehouseId.HasValue)
                query = query.Where(t => t.SourceWarehouseId == sourceWarehouseId);

            if (destinationWarehouseId.HasValue)
                query = query.Where(t => t.DestinationWarehouseId == destinationWarehouseId);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(t => t.Status == status);

            if (companyId.HasValue)
                query = query.Where(t => t.CompanyId == companyId);

            var total = await query.CountAsync();

            var transfers = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (transfers, total);
        }

        public async Task<StockTransfer> CreateAsync(StockTransfer transfer)
        {
            _context.StockTransfers.Add(transfer);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(transfer.Id) ?? transfer;
        }

        public async Task UpdateAsync(StockTransfer transfer)
        {
            _context.StockTransfers.Update(transfer);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.StockTransfers.AnyAsync(t => t.Id == id);
        }
    }
}
