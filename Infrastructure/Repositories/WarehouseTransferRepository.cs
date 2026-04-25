using Application.Abstractions.Inventory;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class WarehouseTransferRepository : IWarehouseTransferRepository
    {
        private readonly POSDbContext _context;

        public WarehouseTransferRepository(POSDbContext context)
        {
            _context = context;
        }

        public async Task<WarehouseTransfer?> GetByIdAsync(int id)
        {
            return await _context.WarehouseTransfers
                .Include(t => t.SourceWarehouse)
                .Include(t => t.DestinationWarehouse)
                .Include(t => t.CreatedBy)
                .Include(t => t.DispatchedBy)
                .Include(t => t.Details)
                    .ThenInclude(d => d.Product)
                .Include(t => t.Receivings)
                    .ThenInclude(r => r.Details)
                        .ThenInclude(rd => rd.Product)
                .Include(t => t.Receivings)
                    .ThenInclude(r => r.CreatedBy)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<WarehouseTransfer?> GetByCodeAsync(string code)
        {
            return await _context.WarehouseTransfers
                .Include(t => t.SourceWarehouse)
                .Include(t => t.DestinationWarehouse)
                .Include(t => t.CreatedBy)
                .Include(t => t.DispatchedBy)
                .Include(t => t.Details)
                    .ThenInclude(d => d.Product)
                .Include(t => t.Receivings)
                    .ThenInclude(r => r.Details)
                        .ThenInclude(rd => rd.Product)
                .Include(t => t.Receivings)
                    .ThenInclude(r => r.CreatedBy)
                .FirstOrDefaultAsync(t => t.Code == code);
        }

        public async Task<(List<WarehouseTransfer> transfers, int totalRecords)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            int? sourceWarehouseId = null,
            int? destinationWarehouseId = null,
            string? status = null,
            int? companyId = null)
        {
            var query = _context.WarehouseTransfers
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
                query = query.Where(t => t.SourceWarehouseId == sourceWarehouseId.Value);

            if (destinationWarehouseId.HasValue)
                query = query.Where(t => t.DestinationWarehouseId == destinationWarehouseId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(t => t.Status == status);

            if (companyId.HasValue)
                query = query.Where(t => t.CompanyId == companyId.Value);

            var total = await query.CountAsync();
            var transfers = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (transfers, total);
        }

        public async Task<WarehouseTransfer> CreateAsync(WarehouseTransfer transfer)
        {
            _context.WarehouseTransfers.Add(transfer);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(transfer.Id) ?? transfer;
        }

        public async Task UpdateAsync(WarehouseTransfer transfer)
        {
            _context.WarehouseTransfers.Update(transfer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateWithReceivingAsync(WarehouseTransfer transfer, WarehouseTransferReceiving receiving)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.WarehouseTransfers.Update(transfer);
                _context.WarehouseTransferReceivings.Add(receiving);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.WarehouseTransfers.AnyAsync(t => t.Id == id);
        }

        public async Task<WarehouseTransferReceiving?> GetReceivingByIdAsync(int receivingId)
        {
            return await _context.WarehouseTransferReceivings
                .Include(r => r.WarehouseTransfer)
                .Include(r => r.DestinationWarehouse)
                .Include(r => r.CreatedBy)
                .Include(r => r.Details)
                    .ThenInclude(d => d.Product)
                .Include(r => r.Details)
                    .ThenInclude(d => d.WarehouseTransferDetail)
                .FirstOrDefaultAsync(r => r.Id == receivingId);
        }

        public async Task<List<WarehouseTransferReceiving>> GetReceivingsByTransferIdAsync(int transferId)
        {
            return await _context.WarehouseTransferReceivings
                .AsNoTracking()
                .Include(r => r.DestinationWarehouse)
                .Include(r => r.CreatedBy)
                .Include(r => r.Details)
                    .ThenInclude(d => d.Product)
                .Where(r => r.WarehouseTransferId == transferId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}
