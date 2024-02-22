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
        Task<Products> CreateAsync(Products products);
    }
}
