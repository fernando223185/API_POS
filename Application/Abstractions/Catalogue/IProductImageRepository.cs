using Domain.Entities;

namespace Application.Abstractions.Catalogue
{
    public interface IProductImageRepository
    {
        Task<ProductImage> CreateAsync(ProductImage productImage);
        Task<ProductImage?> GetByIdAsync(int id);
        Task<List<ProductImage>> GetByProductIdAsync(int productId);
        Task<ProductImage?> GetPrimaryImageAsync(int productId);
        Task<bool> DeleteAsync(int id);
        Task<bool> SetAsPrimaryAsync(int productId, int imageId);
        Task<int> GetNextDisplayOrderAsync(int productId);
    }
}
