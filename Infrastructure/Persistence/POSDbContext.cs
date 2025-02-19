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
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Sales> Sales { get; set; }
        public DbSet<User> User { get; set; }
    }
}
