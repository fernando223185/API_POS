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

        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Sales> Sales { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        // Nuevas entidades de productos y precios
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductSubcategory> ProductSubcategories { get; set; }
        public DbSet<PriceList> PriceLists { get; set; }
        public DbSet<ProductPrice> ProductPrices { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<ProductSupplier> ProductSuppliers { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurar nombre de tabla para User -> Users
            modelBuilder.Entity<User>().ToTable("Users");

            // ✅ CONFIGURACIÓN COMPLETA DE CUSTOMER ERP
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.ID);
                
                // ✅ CORREGIDO: Cambiar precisión de DiscountPercentage de (5,4) a (6,4)
                entity.Property(c => c.DiscountPercentage)
                    .HasPrecision(6, 4)  // Permite hasta 99.9999
                    .HasDefaultValue(0);
                
                entity.Property(c => c.CreditLimit)
                    .HasPrecision(18, 2)
                    .HasDefaultValue(0);
                
                // Configurar valores por defecto
                entity.Property(c => c.SatCfdiUse)
                    .HasDefaultValue("G03");
                
                entity.Property(c => c.PaymentTermsDays)
                    .HasDefaultValue(0);
                
                entity.Property(c => c.IsActive)
                    .HasDefaultValue(true);

                // Configurar relaciones (comentadas temporalmente para evitar errores en migración)
                /*
                entity.HasOne(c => c.PriceList)
                    .WithMany(p => p.Customers)
                    .HasForeignKey(c => c.PriceListId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(c => c.CreatedBy)
                    .WithMany()
                    .HasForeignKey(c => c.CreatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(c => c.UpdatedBy)
                    .WithMany()
                    .HasForeignKey(c => c.UpdatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
                */

                // Configurar índices
                entity.HasIndex(c => c.Code).IsUnique();
                entity.HasIndex(c => c.TaxId);
                entity.HasIndex(c => c.IsActive);
            });

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
            modelBuilder.Entity<Product>()
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

            // Configurar relación Role -> RolePermission -> Permission
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany()
                .HasForeignKey(rp => rp.PermissionId);

            // ✅ CONFIGURACIÓN DE PRICE LIST
            modelBuilder.Entity<PriceList>(entity =>
            {
                entity.Property(p => p.DefaultDiscountPercentage)
                    .HasPrecision(6, 4)  // También corregir aquí
                    .HasDefaultValue(0);
            });

            // ✅ CONFIGURACIÓN DE PRODUCT PRICE
            modelBuilder.Entity<ProductPrice>(entity =>
            {
                entity.Property(pp => pp.Price)
                    .HasPrecision(18, 4);
                    
                entity.Property(pp => pp.DiscountPercentage)
                    .HasPrecision(6, 4)  // También corregir aquí
                    .HasDefaultValue(0);

                entity.HasOne(pp => pp.Product)
                    .WithMany()
                    .HasForeignKey(pp => pp.ProductId);

                entity.HasOne(pp => pp.PriceList)
                    .WithMany(pl => pl.ProductPrices)
                    .HasForeignKey(pp => pp.PriceListId);
            });

            // ✅ NO AGREGAR SEED DATA AQUÍ - Ya existe en la base de datos
            // Los datos de Roles, Users, Modules, Permissions ya están en la BD
            // Solo configuramos las entidades sin duplicar datos
        }
    }
}
