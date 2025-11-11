using Application.Abstractions.Catalogue;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly POSDbContext _dbcontext;

        public ProductRepository(POSDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Products> CreateAsync(Products product)
        {
            await _dbcontext.Products.AddAsync(product);
            await _dbcontext.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteAsync(int productID)
        {
            var existingProduct = await _dbcontext.Products.FirstOrDefaultAsync(c => c.ID == productID);
            if (existingProduct != null)
            {
                _dbcontext.Products.Remove(existingProduct);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<Products>> GetByPageAsync(ProductPageQuery query)
        {
            var pageSize = query.Size ?? 10;
            var pageNumber = query.Nro ?? 1;

            IQueryable<Product> queryable = _dbcontext.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.search))
            {
                queryable = queryable.Where(p => 
                    p.name.Contains(query.search) ||
                    p.code.Contains(query.search) ||
                    (p.description != null && p.description.Contains(query.search)) ||
                    (p.barcode != null && p.barcode.Contains(query.search))
                );
            }

            var results = await queryable
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Convertir Product a Products usando cast explícito
            return results.Select(p => (Products)p);
        }

        public async Task<Products?> GetByIdAsync(int productID)
        {
            var product = await _dbcontext.Products.FirstOrDefaultAsync(c => c.ID == productID);
            return product != null ? (Products)product : null;
        }

        public async Task<Products?> UpdateAsync(Products product)
        {
            var existingProduct = await _dbcontext.Products.FirstOrDefaultAsync(c => c.ID == product.ID);
            if (existingProduct != null)
            {
                existingProduct.name = product.name;
                existingProduct.description = product.description;
                existingProduct.code = product.code;
                existingProduct.barcode = product.barcode;
                existingProduct.price = product.price;
                existingProduct.Brand = product.Brand;
                existingProduct.Model = product.Model;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.SubcategoryId = product.SubcategoryId;
                existingProduct.SatCode = product.SatCode;
                existingProduct.SatUnit = product.SatUnit;
                existingProduct.BaseCost = product.BaseCost;
                existingProduct.TaxRate = product.TaxRate;
                existingProduct.MinimumStock = product.MinimumStock;
                existingProduct.MaximumStock = product.MaximumStock;
                existingProduct.ReorderPoint = product.ReorderPoint;
                existingProduct.Unit = product.Unit;
                existingProduct.Weight = product.Weight;
                existingProduct.Length = product.Length;
                existingProduct.Width = product.Width;
                existingProduct.Height = product.Height;
                existingProduct.Color = product.Color;
                existingProduct.Size = product.Size;
                existingProduct.IsActive = product.IsActive;
                existingProduct.IsService = product.IsService;
                existingProduct.AllowFractionalQuantities = product.AllowFractionalQuantities;
                existingProduct.TrackSerial = product.TrackSerial;
                existingProduct.TrackExpiry = product.TrackExpiry;
                existingProduct.PrimarySupplierId = product.PrimarySupplierId;
                existingProduct.SupplierCode = product.SupplierCode;
                existingProduct.UpdatedAt = DateTime.UtcNow;
                existingProduct.UpdatedByUserId = product.UpdatedByUserId;

                await _dbcontext.SaveChangesAsync();
                return (Products)existingProduct;
            }
            return null;
        }
    }
}