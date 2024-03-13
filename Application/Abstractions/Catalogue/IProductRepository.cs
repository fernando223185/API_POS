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
<<<<<<< HEAD
        Task<Products> UpdateAsync(Products products);
=======
        //Task<Products> UpdateAsync(Products products);
>>>>>>> d399b8080a32d22d35314462b39a24df68edce47
        //Delete a product
        //Task<bool> DeleteAsync(int id);
        //Search a code of product
        //Task<bool> isCodeUniqueAsync(string code);
    }
}
