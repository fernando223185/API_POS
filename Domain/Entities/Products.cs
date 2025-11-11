using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace Domain.Entities
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; } // Mantengo ID para compatibilidad

        // Información básica
        [Required]
        [StringLength(200)]
        public string name { get; set; } = string.Empty; // Mantengo lowercase para compatibilidad

        [StringLength(1000)]
        public string? description { get; set; } // Mantengo lowercase para compatibilidad

        [Required]
        [StringLength(50)]
        public string code { get; set; } = string.Empty; // Mantengo lowercase para compatibilidad

        [StringLength(50)]
        public string? barcode { get; set; } // Mantengo lowercase para compatibilidad

        [StringLength(100)]
        public string? Brand { get; set; }

        [StringLength(100)]
        public string? Model { get; set; }

        // Clasificación y categorización
        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public ProductCategory? Category { get; set; }

        public int? SubcategoryId { get; set; }

        [ForeignKey("SubcategoryId")]
        public ProductSubcategory? Subcategory { get; set; }

        // Información fiscal para facturación
        [StringLength(10)]
        public string? SatCode { get; set; } = "01010101"; // Código SAT para CFDI

        [StringLength(10)]
        public string? SatUnit { get; set; } = "PZA"; // Unidad SAT

        // Precios base
        [Precision(18, 4)]
        public decimal BaseCost { get; set; } = 0; // Costo base

        [Precision(18, 2)]
        public decimal price { get; set; } = 0; // Mantengo price para compatibilidad

        [Precision(5, 4)]
        public decimal TaxRate { get; set; } = 0.16m; // Tasa de impuesto (16% IVA)

        [NotMapped]
        public decimal BasePriceWithTax => price * (1 + TaxRate);

        // Control de inventario
        [Precision(18, 4)]
        public decimal MinimumStock { get; set; } = 0;

        [Precision(18, 4)]
        public decimal MaximumStock { get; set; } = 0;

        [Precision(18, 4)]
        public decimal ReorderPoint { get; set; } = 0;

        [StringLength(20)]
        public string Unit { get; set; } = "PZA"; // Unidad de medida

        // Características del producto
        [Precision(10, 4)]
        public decimal? Weight { get; set; }

        [Precision(10, 4)]
        public decimal? Length { get; set; }

        [Precision(10, 4)]
        public decimal? Width { get; set; }

        [Precision(10, 4)]
        public decimal? Height { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(50)]
        public string? Size { get; set; }

        // Estado y configuración
        public bool IsActive { get; set; } = true;
        public bool IsService { get; set; } = false; // Producto vs Servicio
        public bool AllowFractionalQuantities { get; set; } = false;
        public bool TrackSerial { get; set; } = false; // Seguimiento de números de serie
        public bool TrackExpiry { get; set; } = false; // Seguimiento de fechas de vencimiento

        // Información de proveedores
        public int? PrimarySupplierId { get; set; }

        [ForeignKey("PrimarySupplierId")]
        public Supplier? PrimarySupplier { get; set; }

        [StringLength(100)]
        public string? SupplierCode { get; set; } // Código del producto en el proveedor

        // Metadatos
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public int? CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public User? CreatedBy { get; set; }

        public int? UpdatedByUserId { get; set; }

        [ForeignKey("UpdatedByUserId")]
        public User? UpdatedBy { get; set; }

        // Relaciones - Comentadas temporalmente para evitar errores
        // public ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();
        // public ICollection<ProductStock> ProductStocks { get; set; } = new List<ProductStock>();
        // public ICollection<ProductSupplier> ProductSuppliers { get; set; } = new List<ProductSupplier>();
        // public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        // public ICollection<TransferItem> TransferItems { get; set; } = new List<TransferItem>();
        // public ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
    }

    // Alias para compatibilidad con código existente
    public class Products : Product
    {
    }
}
