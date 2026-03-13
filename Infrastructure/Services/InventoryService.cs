using Application.Abstractions.Inventory;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly POSDbContext _context;

        public InventoryService(POSDbContext context)
        {
            _context = context;
        }

        public async Task<ProductStock?> GetStockAsync(int productId, int warehouseId)
        {
            return await _context.ProductStock
                .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId);
        }

        public async Task CreateMovementAsync(InventoryMovement movement)
        {
            _context.InventoryMovements.Add(movement);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStockAsync(ProductStock stock)
        {
            _context.ProductStock.Update(stock);
            await _context.SaveChangesAsync();
        }

        public async Task CreateStockAsync(ProductStock stock)
        {
            _context.ProductStock.Add(stock);
            await _context.SaveChangesAsync();
        }
    }
}
