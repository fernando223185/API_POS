using Application.Abstractions.Sales_Orders;
using Domain.Entities;
using Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class SalesRepository : ISalesRepository
    {
        private readonly POSDbContext _dbcontext;

        public SalesRepository(POSDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Sales> CreateAsync(Sales sale)
        {
            _dbcontext.Sales.Add(sale);
            await _dbcontext.SaveChangesAsync();
            return sale;
        }

    }
}
