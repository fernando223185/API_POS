using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Persistence
{
    public class POSDbContext : DbContext
    {
        public POSDbContext(DbContextOptions<POSDbContext> options) : base(options)
        {
        }

        public DbSet<Products> Products { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Sales> Sales { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurar nombre de tabla para User -> Users
            modelBuilder.Entity<User>().ToTable("Users");

            // Configurar precisión decimal para Sales
            modelBuilder.Entity<Sales>()
                .Property(s => s.Importe)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Sales>()
                .Property(s => s.Impuestos)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Sales>()
                .Property(s => s.Saldo)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Sales>()
                .Property(s => s.Discount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Sales>()
                .Property(s => s.PrecioTotal)
                .HasPrecision(18, 2);

            // Configurar precisión decimal para Products
            modelBuilder.Entity<Products>()
                .Property(p => p.price)
                .HasPrecision(18, 2);

            // Configurar la relación entre User y Role
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            // Configurar relación Module -> Permission
            modelBuilder.Entity<Permission>()
                .HasOne(p => p.Module)
                .WithMany(m => m.Permissions)
                .HasForeignKey(p => p.ModuleId);

            // Configurar relación RolePermission (Many-to-Many)
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            // Seed data para roles básicos
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Administrador", Description = "Acceso completo al sistema", IsActive = true },
                new Role { Id = 2, Name = "Usuario", Description = "Acceso básico al sistema", IsActive = true },
                new Role { Id = 3, Name = "Vendedor", Description = "Personal de ventas", IsActive = true },
                new Role { Id = 4, Name = "Almacenista", Description = "Gestión de inventario", IsActive = true },
                new Role { Id = 10, Name = "Cajero", Description = "Operación de punto de venta", IsActive = true },
                new Role { Id = 11, Name = "Gerente", Description = "Supervisión y reportes", IsActive = true }
            );

            // Seed data para el usuario administrador
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Code = "ADMIN001",
                    Name = "Administrador",
                    PasswordHash = HashPasswordSHA256("admin123"),
                    Email = "admin@sistema.com",
                    Phone = "1234567890",
                    RoleId = 1,
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }

        private static byte[] HashPasswordSHA256(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }
    }
}
