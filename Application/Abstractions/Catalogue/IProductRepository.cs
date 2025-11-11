using Domain.Entities;

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
    }

    // Clase para query de paginación
    public class ProductPageQuery
    {
        public int? Size { get; set; } = 10;
        public int? Nro { get; set; } = 1;
        public string? search { get; set; }
    }
}
