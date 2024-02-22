using Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Persistence
{
    public class POSDbContext : DbContext
    {
        public POSDbContext(
            DbContextOptions options
            )
            : base(options)
        {

        }
        public DbSet<Products> Products { get; set; }

    }
}
