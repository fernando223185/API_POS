using Domain.Entities;

namespace Application.Abstractions.Inventory
{
    public interface IInventoryService
    {
        Task<ProductStock?> GetStockAsync(int productId, int warehouseId);
        Task CreateMovementAsync(InventoryMovement movement);
        Task UpdateStockAsync(ProductStock stock);
        Task CreateStockAsync(ProductStock stock);
    }
}
