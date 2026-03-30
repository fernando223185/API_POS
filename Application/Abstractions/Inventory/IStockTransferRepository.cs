using Domain.Entities;

namespace Application.Abstractions.Inventory
{
    public interface IStockTransferRepository
    {
        Task<StockTransfer?> GetByIdAsync(int id);
        Task<StockTransfer?> GetByCodeAsync(string code);

        Task<(List<StockTransfer> transfers, int totalRecords)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            int? sourceWarehouseId = null,
            int? destinationWarehouseId = null,
            string? status = null,
            int? companyId = null);

        Task<StockTransfer> CreateAsync(StockTransfer transfer);
        Task UpdateAsync(StockTransfer transfer);
        Task<bool> ExistsAsync(int id);
    }
}
