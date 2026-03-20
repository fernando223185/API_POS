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
        public DbSet<User> User { get; set; }
        public DbSet<Role> Roles { get; set; }
        
        // ✅ SISTEMA UNIFICADO DE PERMISOS (Módulos/Submódulos)
        public DbSet<UserModulePermission> UserModulePermissions { get; set; }
        public DbSet<RoleModulePermission> RoleModulePermissions { get; set; }

        // ✅ DEFINICIONES DE MÓDULOS Y SUBMÓDULOS DEL SISTEMA
        public DbSet<SystemModule> Modules { get; set; }
        public DbSet<SystemSubmodule> Submodules { get; set; }

        // Nuevas entidades de productos y precios
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductSubcategory> ProductSubcategories { get; set; }
        public DbSet<PriceList> PriceLists { get; set; }
        public DbSet<ProductPrice> ProductPrices { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<ProductSupplier> ProductSuppliers { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        
        // ✅ NUEVO: Gestión de sucursales
        public DbSet<Branch> Branches { get; set; }
        
        // ✅ NUEVO: Gestión de almacenes
        public DbSet<Warehouse> Warehouses { get; set; }
        
        // ✅ NUEVO: Sistema de compras y recepción
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
        public DbSet<PurchaseOrderReceiving> PurchaseOrderReceivings { get; set; }
        public DbSet<PurchaseOrderReceivingDetail> PurchaseOrderReceivingDetails { get; set; }
        
        // ✅ NUEVO: Control de inventario
        public DbSet<InventoryMovement> InventoryMovements { get; set; }
        public DbSet<ProductStock> ProductStock { get; set; }

        // ✅ NUEVO: Sistema de ventas con cobranza
        public DbSet<Sale> SalesNew { get; set; }
        public DbSet<SaleDetail> SaleDetails { get; set; }
        public DbSet<SalePayment> SalePayments { get; set; }

        // ✅ NUEVO: Gestión de empresas
        public DbSet<Company> Companies { get; set; }

        // ✅ NUEVO: Sistema de facturación CFDI
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceDetail> InvoiceDetails { get; set; }

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

                // Configurar índices
                entity.HasIndex(c => c.Code).IsUnique();
                entity.HasIndex(c => c.TaxId);
                entity.HasIndex(c => c.IsActive);
            });

            // Configurar precisión decimal para Products
            modelBuilder.Entity<Product>()
                .Property(p => p.price)
                .HasPrecision(18, 2);

            // Configurar la relación entre User y Role
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            // ✅ CONFIGURACIÓN DE ALMACÉN ASIGNADO AL USUARIO
            modelBuilder.Entity<User>()
                .HasOne(u => u.DefaultWarehouse)
                .WithMany()
                .HasForeignKey(u => u.DefaultWarehouseId)
                .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

            modelBuilder.Entity<User>()
                .Property(u => u.CanSellFromMultipleWarehouses)
                .HasDefaultValue(false);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.DefaultWarehouseId);

            // ✅ CONFIGURACIÓN DE PERMISOS DE USUARIO POR MÓDULO/SUBMÓDULO
            modelBuilder.Entity<UserModulePermission>(entity =>
            {
                entity.HasKey(ump => ump.Id);
                
                entity.HasOne(ump => ump.User)
                    .WithMany()
                    .HasForeignKey(ump => ump.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índice único para evitar duplicados (userId + moduleId + submoduleId)
                entity.HasIndex(ump => new { ump.UserId, ump.ModuleId, ump.SubmoduleId })
                    .IsUnique();
                
                entity.HasIndex(ump => ump.UserId);
                entity.HasIndex(ump => ump.ModuleId);
            });

            // ✅ CONFIGURACIÓN DE PERMISOS DE ROL POR MÓDULO/SUBMÓDULO
            modelBuilder.Entity<RoleModulePermission>(entity =>
            {
                entity.HasKey(rmp => rmp.Id);
                
                entity.HasOne(rmp => rmp.Role)
                    .WithMany()
                    .HasForeignKey(rmp => rmp.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índice único para evitar duplicados (roleId + moduleId + submoduleId)
                entity.HasIndex(rmp => new { rmp.RoleId, rmp.ModuleId, rmp.SubmoduleId })
                    .IsUnique();
                
                entity.HasIndex(rmp => rmp.RoleId);
                entity.HasIndex(rmp => rmp.ModuleId);
            });

            // ✅ CONFIGURACIÓN DE MÓDULOS Y SUBMÓDULOS DEL SISTEMA
            modelBuilder.Entity<SystemModule>(entity =>
            {
                entity.HasKey(sm => sm.Id);
                entity.ToTable("Modules"); // Asegurar nombre correcto
                entity.HasIndex(sm => sm.Order);
                entity.HasIndex(sm => sm.IsActive);
            });

            modelBuilder.Entity<SystemSubmodule>(entity =>
            {
                entity.HasKey(ss => ss.Id);
                entity.ToTable("Submodules"); // Asegurar nombre correcto
                
                entity.HasOne(ss => ss.Module)
                    .WithMany(sm => sm.Submodules)
                    .HasForeignKey(ss => ss.ModuleId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(ss => ss.ModuleId);
                entity.HasIndex(ss => ss.Order);
                entity.HasIndex(ss => ss.IsActive);
            });

            // ✅ CONFIGURACIÓN DE PRICE LIST
            modelBuilder.Entity<PriceList>(entity =>
            {
                entity.Property(p => p.DefaultDiscountPercentage)
                    .HasPrecision(6, 4)
                    .HasDefaultValue(0);
            });

            // ✅ CONFIGURACIÓN DE PRODUCT PRICE
            modelBuilder.Entity<ProductPrice>(entity =>
            {
                entity.Property(pp => pp.Price)
                    .HasPrecision(18, 4);
                    
                entity.Property(pp => pp.DiscountPercentage)
                    .HasPrecision(6, 4)
                    .HasDefaultValue(0);

                entity.HasOne(pp => pp.Product)
                    .WithMany()
                    .HasForeignKey(pp => pp.ProductId);

                entity.HasOne(pp => pp.PriceList)
                    .WithMany(pl => pl.ProductPrices)
                    .HasForeignKey(pp => pp.PriceListId);
            });

            // ✅ CONFIGURACIÓN DE ÓRDENAS DE COMPRA
            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.HasOne(po => po.Supplier)
                    .WithMany()
                    .HasForeignKey(po => po.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

                entity.HasOne(po => po.Warehouse)
                    .WithMany()
                    .HasForeignKey(po => po.WarehouseId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

                entity.HasOne(po => po.CreatedBy)
                    .WithMany()
                    .HasForeignKey(po => po.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

                entity.HasOne(po => po.UpdatedBy)
                    .WithMany()
                    .HasForeignKey(po => po.UpdatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE
            });

            // ✅ CONFIGURACIÓN DE DETALLE DE ÓRDENAS DE COMPRA
            modelBuilder.Entity<PurchaseOrderDetail>(entity =>
            {
                entity.HasOne(pod => pod.PurchaseOrder)
                    .WithMany(po => po.Details)
                    .HasForeignKey(pod => pod.PurchaseOrderId)
                    .OnDelete(DeleteBehavior.Cascade); // CASCADE solo con PurchaseOrder

                entity.HasOne(pod => pod.Product)
                    .WithMany()
                    .HasForeignKey(pod => pod.ProductId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE
            });

            // ✅ CONFIGURACIÓN DE RECEPCIONES
            modelBuilder.Entity<PurchaseOrderReceiving>(entity =>
            {
                entity.HasOne(por => por.PurchaseOrder)
                    .WithMany(po => po.Receivings)
                    .HasForeignKey(por => por.PurchaseOrderId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE - importante para auditoría

                entity.HasOne(por => por.Warehouse)
                    .WithMany()
                    .HasForeignKey(por => por.WarehouseId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

                entity.HasOne(por => por.CreatedBy)
                    .WithMany()
                    .HasForeignKey(por => por.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

                entity.HasOne(por => por.UpdatedBy)
                    .WithMany()
                    .HasForeignKey(por => por.UpdatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE
            });

            // ✅ CONFIGURACIÓN DE DETALLE DE RECEPCIONES
            modelBuilder.Entity<PurchaseOrderReceivingDetail>(entity =>
            {
                entity.HasOne(pord => pord.PurchaseOrderReceiving)
                    .WithMany(por => por.Details)
                    .HasForeignKey(pord => pord.PurchaseOrderReceivingId)
                    .OnDelete(DeleteBehavior.Cascade); // CASCADE solo con Receiving

                entity.HasOne(pord => pord.Product)
                    .WithMany()
                    .HasForeignKey(pord => pord.ProductId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

                entity.HasOne(pord => pord.PurchaseOrderDetail)
                    .WithMany()
                    .HasForeignKey(pord => pord.PurchaseOrderDetailId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE
            });

            // ✅ CONFIGURACIÓN DE MOVIMIENTOS DE INVENTARIO
            modelBuilder.Entity<InventoryMovement>(entity =>
            {
                entity.HasOne(im => im.Product)
                    .WithMany()
                    .HasForeignKey(im => im.ProductId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

                entity.HasOne(im => im.Warehouse)
                    .WithMany()
                    .HasForeignKey(im => im.WarehouseId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

                entity.HasOne(im => im.PurchaseOrderReceiving)
                    .WithMany()
                    .HasForeignKey(im => im.PurchaseOrderReceivingId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

                entity.HasOne(im => im.CreatedBy)
                    .WithMany()
                    .HasForeignKey(im => im.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

                entity.HasIndex(im => im.ProductId);
                entity.HasIndex(im => im.WarehouseId);
                entity.HasIndex(im => im.MovementDate);
                entity.HasIndex(im => im.MovementType);
            });

            // ✅ CONFIGURACIÓN DE STOCK DE PRODUCTOS
            modelBuilder.Entity<ProductStock>(entity =>
            {
                entity.HasOne(ps => ps.Product)
                    .WithMany()
                    .HasForeignKey(ps => ps.ProductId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

                entity.HasOne(ps => ps.Warehouse)
                    .WithMany()
                    .HasForeignKey(ps => ps.WarehouseId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

                // Índice único: un producto solo puede tener un registro por almacén
                entity.HasIndex(ps => new { ps.ProductId, ps.WarehouseId })
                    .IsUnique();
            });

            // ✅ CONFIGURACIÓN DE VENTAS
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.ToTable("Sales");

                entity.HasOne(s => s.Customer)
                    .WithMany()
                    .HasForeignKey(s => s.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull); // NO CASCADE

                entity.HasOne(s => s.Warehouse)
                    .WithMany()
                    .HasForeignKey(s => s.WarehouseId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

                // 🆕 NUEVAS RELACIONES PARA BRANCH Y COMPANY
                entity.HasOne(s => s.Branch)
                    .WithMany()
                    .HasForeignKey(s => s.BranchId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

                entity.HasOne(s => s.Company)
                    .WithMany()
                    .HasForeignKey(s => s.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

                entity.HasOne(s => s.User)
                    .WithMany()
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

                entity.HasOne(s => s.PriceList)
                    .WithMany()
                    .HasForeignKey(s => s.PriceListId)
                    .OnDelete(DeleteBehavior.SetNull); // NO CASCADE

                entity.HasOne(s => s.CreatedBy)
                    .WithMany()
                    .HasForeignKey(s => s.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

                entity.HasIndex(s => s.Code).IsUnique();
                entity.HasIndex(s => s.SaleDate);
                entity.HasIndex(s => s.CustomerId);
                entity.HasIndex(s => s.WarehouseId);
                entity.HasIndex(s => s.BranchId); // 🆕 NUEVO ÍNDICE
                entity.HasIndex(s => s.CompanyId); // 🆕 NUEVO ÍNDICE
                entity.HasIndex(s => s.UserId);
                entity.HasIndex(s => s.Status);
                entity.HasIndex(s => s.IsPostedToInventory);
            });

            // ✅ CONFIGURACIÓN DE DETALLES DE VENTA
            modelBuilder.Entity<SaleDetail>(entity =>
            {
                entity.HasKey(sd => sd.Id);

                entity.HasOne(sd => sd.Sale)
                    .WithMany(s => s.Details)
                    .HasForeignKey(sd => sd.SaleId)
                    .OnDelete(DeleteBehavior.Cascade); // CASCADE con Sale

                entity.HasOne(sd => sd.Product)
                    .WithMany()
                    .HasForeignKey(sd => sd.ProductId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

                entity.HasIndex(sd => sd.SaleId);
                entity.HasIndex(sd => sd.ProductId);
            });

            // ✅ CONFIGURACIÓN DE PAGOS DE VENTA
            modelBuilder.Entity<SalePayment>(entity =>
            {
                entity.HasKey(sp => sp.Id);

                entity.HasOne(sp => sp.Sale)
                    .WithMany(s => s.Payments)
                    .HasForeignKey(sp => sp.SaleId)
                    .OnDelete(DeleteBehavior.Cascade); // CASCADE con Sale

                entity.HasIndex(sp => sp.SaleId);
                entity.HasIndex(sp => sp.PaymentMethod);
                entity.HasIndex(sp => sp.PaymentDate);
                entity.HasIndex(sp => sp.Status);
            });

            // ✅ ACTUALIZAR INVENTORYMOVEMENT PARA SOPORTAR VENTAS
            modelBuilder.Entity<InventoryMovement>(entity =>
            {
                // ...existing configuration...

                entity.HasOne(im => im.Sale)
                    .WithMany()
                    .HasForeignKey(im => im.SaleId)
                    .OnDelete(DeleteBehavior.SetNull); // NO CASCADE

                entity.HasIndex(im => im.SaleId);
            });

            // ✅ CONFIGURACIÓN DE FACTURAS (INVOICES)
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.ToTable("Invoices");

                // Relación opcional con Sale
                entity.HasOne(i => i.Sale)
                    .WithMany()
                    .HasForeignKey(i => i.SaleId)
                    .OnDelete(DeleteBehavior.SetNull); // NO CASCADE

                // Relación con Company (Emisor)
                entity.HasOne(i => i.Company)
                    .WithMany()
                    .HasForeignKey(i => i.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

                // Relación opcional con Customer (Receptor)
                entity.HasOne(i => i.Customer)
                    .WithMany()
                    .HasForeignKey(i => i.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull); // NO CASCADE

                // Relación con Usuario que creó
                entity.HasOne(i => i.CreatedBy)
                    .WithMany()
                    .HasForeignKey(i => i.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

                // Relación con Usuario que canceló
                entity.HasOne(i => i.CancelledBy)
                    .WithMany()
                    .HasForeignKey(i => i.CancelledByUserId)
                    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

                // Índices
                entity.HasIndex(i => new { i.Serie, i.Folio }).IsUnique();
                entity.HasIndex(i => i.Uuid).IsUnique();
                entity.HasIndex(i => i.Status);
                entity.HasIndex(i => i.InvoiceDate);
                entity.HasIndex(i => i.CompanyId);
                entity.HasIndex(i => i.CustomerId);
                entity.HasIndex(i => i.SaleId);
                entity.HasIndex(i => i.CreatedAt);
            });

            // ✅ CONFIGURACIÓN DE DETALLES DE FACTURA (INVOICE DETAILS)
            modelBuilder.Entity<InvoiceDetail>(entity =>
            {
                entity.HasKey(id => id.Id);
                entity.ToTable("InvoiceDetails");

                // Relación con Invoice
                entity.HasOne(id => id.Invoice)
                    .WithMany(i => i.Details)
                    .HasForeignKey(id => id.InvoiceId)
                    .OnDelete(DeleteBehavior.Cascade); // CASCADE con Invoice

                // Relación opcional con Product
                entity.HasOne(id => id.Product)
                    .WithMany()
                    .HasForeignKey(id => id.ProductId)
                    .OnDelete(DeleteBehavior.SetNull); // NO CASCADE

                // Índices
                entity.HasIndex(id => id.InvoiceId);
                entity.HasIndex(id => id.ProductId);
            });
        }
    }
}
