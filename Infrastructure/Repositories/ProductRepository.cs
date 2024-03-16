using Domain.Entities;
using Infrastructure.Persistence;
using Application.Abstractions.Catalogue;
using Microsoft.EntityFrameworkCore;
using Domain.DTOs;
using Application.Core.Product.Queries;

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
            data.price = products.price;

            await _dbcontext.SaveChangesAsync();

            return products;
        }
        public async Task<bool> isCodeUniqueAsync(string code)
        {
            return !await _dbcontext.Products.AnyAsync(p => p.code == code);

        }

        public async Task<PaginatedDto> GetByPageAsync(GetProductByPageQuery data)
        {
            int pageSize = 50;
            PaginatedDto paginate = new PaginatedDto(); 

            IQueryable<Products> query = _dbcontext.Products.AsQueryable();

            if (!string.IsNullOrEmpty(data.search))
            {
                query = query.Where(c => c.code == data.search
                        || c.name.Contains(data.search)
                        || c.barcode.Contains(data.search)
                );
            }

            int pagesCount = (int)Math.Ceiling((decimal)await query.CountAsync() / pageSize);

            var products = await query
                .OrderByDescending(c => c.ID)
                .Skip((data.Page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new
                {
                    s.ID,
                    s.name,
                    s.barcode,
                    s.description,
                    s.price
                })
                .ToListAsync();

            paginate.sizePage = pagesCount;
            paginate.page = data.Page;
            paginate.totalPages = pagesCount;
            paginate.data = products;

            return paginate;
        }

        public async Task<Products> GetByIdAsync(int productID)
        {
            return await _dbcontext.Products.FirstOrDefaultAsync(c => c.ID == productID);
        }
    }
}
