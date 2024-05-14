using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions.Sales_Orders
{
    public interface ISalesRepository
    {
        Task<Sales> CreateAsync(Sales Sale);
        Task<Sales> GetByIdAsync(int id);


    }
}
