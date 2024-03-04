using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions.Catalogue
{
    public interface IProductRepository
    {
        //Create a new producto
        Task<Products> CreateAsync(Products products);
        //Update a product
        Task<Products> UpdateAsync(Products products);
        //Delete a product
        //Task<bool> DeleteAsync(int id);
        //Search a code of product
        Task<bool> isCodeUniqueAsync(string code);
    }
}
