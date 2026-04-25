using Domain.Entities;

namespace Application.Abstractions.Inventory
{
    public interface IWarehouseTransferRepository
    {
        Task<WarehouseTransfer?> GetByIdAsync(int id);
        Task<WarehouseTransfer?> GetByCodeAsync(string code);

        Task<(List<WarehouseTransfer> transfers, int totalRecords)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            int? sourceWarehouseId = null,
            int? destinationWarehouseId = null,
            string? status = null,
            int? companyId = null);

        Task<WarehouseTransfer> CreateAsync(WarehouseTransfer transfer);
        Task UpdateAsync(WarehouseTransfer transfer);

        /// <summary>
        /// Persiste la actualización del traspaso (estado, cantidades recibidas) y crea
        /// el registro de recepción en una misma operación de base de datos.
        /// </summary>
        Task UpdateWithReceivingAsync(WarehouseTransfer transfer, WarehouseTransferReceiving receiving);

        Task<bool> ExistsAsync(int id);

        Task<WarehouseTransferReceiving?> GetReceivingByIdAsync(int receivingId);
        Task<List<WarehouseTransferReceiving>> GetReceivingsByTransferIdAsync(int transferId);
    }
}
