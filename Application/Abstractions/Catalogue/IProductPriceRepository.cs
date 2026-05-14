using Domain.Entities;

namespace Application.Abstractions.Catalogue
{
    public interface IProductPriceRepository
    {
        Task<ProductPrice?> GetByIdAsync(int id);
        Task<ProductPrice?> GetByProductAndListAsync(int productId, int priceListId, bool includeInactive = false);
        Task<List<ProductPrice>> ListAsync(int? productId, int? priceListId, bool onlyActive = true);
        Task<List<ProductPrice>> GetByProductAsync(int productId, bool onlyActive = true);
        Task<List<ProductPrice>> GetByPriceListAsync(int priceListId, bool onlyActive = true);

        Task<ProductPrice> CreateAsync(ProductPrice entity);
        Task<ProductPrice> UpdateAsync(ProductPrice entity);
        Task<bool> SetActiveAsync(int id, bool isActive);

        Task<bool> ProductExistsAsync(int productId);
        Task<bool> PriceListExistsAndActiveAsync(int priceListId);

        /// <summary>
        /// Inserta o actualiza varios precios en bloque (upsert por par Product+PriceList).
        /// Retorna (insertados, actualizados).
        /// </summary>
        Task<(int Inserted, int Updated)> BulkUpsertAsync(
            int priceListId,
            IEnumerable<(int ProductId, decimal Price, decimal DiscountPercentage)> items,
            int createdByUserId);
    }
}
