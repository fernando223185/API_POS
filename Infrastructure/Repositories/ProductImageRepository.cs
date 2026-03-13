using Application.Abstractions.Catalogue;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ProductImageRepository : IProductImageRepository
    {
        private readonly POSDbContext _context;

        public ProductImageRepository(POSDbContext context)
        {
            _context = context;
        }

        public async Task<ProductImage> CreateAsync(ProductImage productImage)
        {
            _context.ProductImages.Add(productImage);
            await _context.SaveChangesAsync();
            return productImage;
        }

        public async Task<ProductImage?> GetByIdAsync(int id)
        {
            return await _context.ProductImages
                .Include(pi => pi.UploadedBy)
                .FirstOrDefaultAsync(pi => pi.Id == id);
        }

        public async Task<List<ProductImage>> GetByProductIdAsync(int productId)
        {
            return await _context.ProductImages
                .Include(pi => pi.UploadedBy)
                .Where(pi => pi.ProductId == productId && pi.IsActive)
                .OrderBy(pi => pi.DisplayOrder)
                .ThenByDescending(pi => pi.IsPrimary)
                .ToListAsync();
        }

        public async Task<ProductImage?> GetPrimaryImageAsync(int productId)
        {
            return await _context.ProductImages
                .Where(pi => pi.ProductId == productId && pi.IsPrimary && pi.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var image = await _context.ProductImages.FindAsync(id);
            if (image == null) return false;

            // Soft delete
            image.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetAsPrimaryAsync(int productId, int imageId)
        {
            // Quitar primary de todas las imágenes del producto
            var allImages = await _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .ToListAsync();

            foreach (var img in allImages)
            {
                img.IsPrimary = (img.Id == imageId);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetNextDisplayOrderAsync(int productId)
        {
            var maxOrder = await _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .MaxAsync(pi => (int?)pi.DisplayOrder);

            return (maxOrder ?? 0) + 1;
        }
    }
}
