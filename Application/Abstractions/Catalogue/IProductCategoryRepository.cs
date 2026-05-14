using Domain.Entities;

namespace Application.Abstractions.Catalogue
{
    public interface IProductCategoryRepository
    {
        Task<ProductCategory?> GetByIdAsync(int id, bool includeSubcategories = false);
        Task<List<ProductCategory>> GetAllAsync(bool includeInactive = false);
        Task<ProductCategory> CreateAsync(ProductCategory category);
        Task<ProductCategory> UpdateAsync(ProductCategory category);
        Task<bool> DeleteAsync(int id);

        Task<bool> CodeExistsAsync(string code, int? excludeId = null);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<bool> HasProductsAsync(int categoryId);

        Task<int> CountProductsAsync(int categoryId, bool onlyActive = false);
        Task<int> CountSubcategoriesAsync(int categoryId, bool onlyActive = true);
        Task<List<CategoryStatsRow>> GetStatsAsync();
    }

    public class CategoryStatsRow
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public int ProductsCount { get; set; }
        public int TotalProducts { get; set; }
        public int SubcategoriesCount { get; set; }
        public decimal AvgPrice { get; set; }
        public decimal TotalValue { get; set; }
    }
}
