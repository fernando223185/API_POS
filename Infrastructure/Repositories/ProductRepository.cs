using Domain.Entities;
using Infrastructure.Persistence;
using Application.Abstractions.Catalogue;
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

        public async Task<Products> CreateAsync(Products products)
        {
            _dbcontext.Products.Add(products);
            await _dbcontext.SaveChangesAsync();
            return products;
        }
        public async Task<Products> UpdateAsync(Products products)
        {
            var data = await _dbcontext.Products.FirstOrDefaultAsync(p => p.ID == products.ID);

            data.name = products.name;
            data.description = products.description;
            data.code = products.code;
            data.barcode = products.barcode;
            data.category = products.category;
            data.branch = products.branch;
            data.price = products.price;

            await _dbcontext.SaveChangesAsync();

            return products;
        }
        public async Task<bool> isCodeUniqueAsync(string code)
        {
            return !await _dbcontext.Products.AnyAsync(p => p.code == code);

        }
    }
}
