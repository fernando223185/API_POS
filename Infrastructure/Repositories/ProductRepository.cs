using Domain.Entities;
using Infrastructure.Persistence;
using Application.Abstractions.Catalogue;

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
    }
}
