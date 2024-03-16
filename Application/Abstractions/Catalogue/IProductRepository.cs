using Application.Core.Product.Queries;
using Domain.DTOs;
using Domain.Entities;


namespace Application.Abstractions.Catalogue
{
    public interface IProductRepository
    {
        //Create a new producto
        Task<Products> CreateAsync(Products products);
        //Update a product
        Task<Products> UpdateAsync(Products products);
        //Task<Products> UpdateAsync(Products products);
        //Delete a product
        //Task<bool> DeleteAsync(int id);
        //Search a code of product
        //Task<bool> isCodeUniqueAsync(string code);
        Task<PaginatedDto> GetByPageAsync(GetProductByPageQuery data);
        Task<Products> GetByIdAsync(int customerID);

    }
}
