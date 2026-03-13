using Domain.Entities;
using Application.DTOs.Product;

namespace Application.Abstractions.Catalogue
{
    public interface IProductRepository
    {
        // Métodos principales del repositorio
        Task<Products> CreateAsync(Products product);
        Task<Products?> UpdateAsync(Products product);
        Task<bool> DeleteAsync(int productID);
        Task<Products?> GetByIdAsync(int productID);
        Task<IEnumerable<Products>> GetByPageAsync(ProductPageQuery query);
        
        // ✅ NUEVOS MÉTODOS PARA PAGINACIÓN AVANZADA
        Task<int> GetTotalCountAsync(ProductPageQuery query);
        Task<(IEnumerable<Products> Products, int TotalCount)> GetPagedWithCountAsync(ProductPageQuery query);
        Task<(int Total, int Active, int Inactive, decimal TotalValue, int LowStock)> GetStatisticsAsync();
        Task<List<CategoryStats>> GetTopCategoriesAsync(int count = 5);
        Task<int> GetOutOfStockCountAsync();

        // ✅ NUEVO: Métodos para inventario por bodega
        Task<List<ProductStock>> GetProductStockByWarehouseAsync(int productId, int? warehouseId = null);
        Task<decimal> GetTotalStockByProductAsync(int productId);
        Task<List<ProductWarehouseStockDto>> GetWarehouseStockOrZeroAsync(int productId, int? warehouseId, List<ProductStock> existingStock);
    }

    // Clase para query de paginación mejorada
    public class ProductPageQuery
    {
        public int? Size { get; set; } = 10;
        public int? Nro { get; set; } = 1;
        public string? search { get; set; }
        public int? CategoryId { get; set; }
        public bool? IsActive { get; set; }
        public string? SortBy { get; set; } = "name";
        public string? SortOrder { get; set; } = "asc";

        // ✅ NUEVO: Opciones de inventario
        public bool IncludeWarehouseStock { get; set; } = false;
        public int? WarehouseId { get; set; }
        public bool? OnlyWithStock { get; set; }
        public bool? OnlyBelowMinimum { get; set; }
    }
}
