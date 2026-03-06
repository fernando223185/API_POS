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

        // ✅ INFORMACIÓN BÁSICA
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

        // ✅ CLASIFICACIÓN Y CATEGORIZACIÓN
        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public ProductCategory? Category { get; set; }

        public int? SubcategoryId { get; set; }

        [ForeignKey("SubcategoryId")]
        public ProductSubcategory? Subcategory { get; set; }

        // ✅ INFORMACIÓN FISCAL PARA FACTURACIÓN
        [StringLength(10)]
        public string? SatCode { get; set; } = "01010101"; // Código SAT para CFDI

        [StringLength(10)]
        public string? SatUnit { get; set; } = "PZA"; // Unidad SAT

        [StringLength(20)]
        public string? SatTaxType { get; set; } = "Tasa"; // "Tasa", "Cuota", "Exento"

        [StringLength(20)]
        public string? CustomsCode { get; set; } // Código aduanero para importación

        [StringLength(50)]
        public string? CountryOfOrigin { get; set; } = "México"; // País de origen

        // ✅ PRECIOS BASE
        [Precision(18, 4)]
        public decimal BaseCost { get; set; } = 0; // Costo base

        [Precision(18, 2)]
        public decimal price { get; set; } = 0; // Mantengo price para compatibilidad

        [Precision(5, 4)]
        public decimal TaxRate { get; set; } = 0.16m; // Tasa de impuesto (16% IVA)

        [NotMapped]
        public decimal BasePriceWithTax => price * (1 + TaxRate);

        // ✅ CONTROL DE INVENTARIO
        [Precision(18, 4)]
        public decimal MinimumStock { get; set; } = 0;

        [Precision(18, 4)]
        public decimal MaximumStock { get; set; } = 0;

        [Precision(18, 4)]
        public decimal ReorderPoint { get; set; } = 0;

        [StringLength(20)]
        public string Unit { get; set; } = "PZA"; // Unidad de medida

        // ✅ CARACTERÍSTICAS DEL PRODUCTO
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

        // ✅ NUEVOS CAMPOS COMERCIALES AVANZADOS
        public int? Warranty { get; set; } // Meses de garantía

        [StringLength(50)]
        public string? WarrantyType { get; set; } // "Fabricante", "Tienda", "Extendida"

        [StringLength(100)]
        public string? Location { get; set; } // Ubicación física en almacén

        [StringLength(20)]
        public string? Aisle { get; set; } // Pasillo

        [StringLength(20)]
        public string? Shelf { get; set; } // Estante

        [StringLength(20)]
        public string? Bin { get; set; } // Contenedor

        // ✅ CLASIFICACIÓN AVANZADA
        [StringLength(500)]
        public string? Tags { get; set; } // Tags separados por comas "oferta,nuevo,temporada"

        [StringLength(50)]
        public string? Season { get; set; } // "Primavera", "Verano", "Otoño", "Invierno", "Todo el año"

        [StringLength(20)]
        public string? TargetGender { get; set; } // "Masculino", "Femenino", "Unisex", "Infantil"

        [StringLength(50)]
        public string? AgeGroup { get; set; } // "Adulto", "Infantil", "Juvenil", "Senior"

        // ✅ INFORMACIÓN DE VENTAS
        [Precision(18, 4)]
        public decimal? MaxQuantityPerSale { get; set; } // Cantidad máxima por venta

        [Precision(18, 4)]
        public decimal MinQuantityPerSale { get; set; } = 1; // Cantidad mínima por venta

        [StringLength(500)]
        public string? SalesNotes { get; set; } // Notas especiales de venta

        public bool IsDiscountAllowed { get; set; } = true; // Permitir descuentos

        [Precision(5, 2)]
        public decimal? MaxDiscountPercentage { get; set; } // Descuento máximo permitido

        // ✅ E-COMMERCE Y MARKETING
        [StringLength(200)]
        public string? SEOTitle { get; set; } // Título SEO

        [StringLength(500)]
        public string? SEODescription { get; set; } // Descripción SEO

        [StringLength(300)]
        public string? SEOKeywords { get; set; } // Palabras clave SEO

        public bool IsWebVisible { get; set; } = true; // Visible en web

        public bool IsFeatured { get; set; } = false; // Producto destacado

        public DateTime? LaunchDate { get; set; } // Fecha de lanzamiento

        public DateTime? DiscontinuedDate { get; set; } // Fecha de descontinuación

        // ✅ INFORMACIÓN TÉCNICA
        [StringLength(2000)]
        public string? TechnicalSpecs { get; set; } // Especificaciones técnicas (JSON)

        [StringLength(100)]
        public string? ManufacturerPartNumber { get; set; } // Número de parte del fabricante

        [StringLength(50)]
        public string? UPC { get; set; } // Código UPC

        [StringLength(50)]
        public string? EAN { get; set; } // Código EAN

        [StringLength(50)]
        public string? ISBN { get; set; } // Para libros

        // ✅ LOGÍSTICA Y ENVÍO
        public bool IsFragile { get; set; } = false; // Producto frágil

        public bool RequiresSpecialHandling { get; set; } = false; // Manejo especial

        [StringLength(50)]
        public string? ShippingClass { get; set; } // "Normal", "Pesado", "Frágil", "Peligroso"

        [Precision(10, 4)]
        public decimal? PackageLength { get; set; } // Largo del paquete

        [Precision(10, 4)]
        public decimal? PackageWidth { get; set; } // Ancho del paquete

        [Precision(10, 4)]
        public decimal? PackageHeight { get; set; } // Alto del paquete

        [Precision(10, 4)]
        public decimal? PackageWeight { get; set; } // Peso del paquete

        // ✅ CONTROL DE CALIDAD
        [StringLength(20)]
        public string? QualityGrade { get; set; } // "A", "B", "C", "Premium", "Standard"

        public DateTime? LastQualityCheck { get; set; } // Última revisión de calidad

        [Precision(5, 4)]
        public decimal? DefectRate { get; set; } // Tasa de defectos

        [Precision(5, 4)]
        public decimal? ReturnRate { get; set; } // Tasa de devoluciones

        // ✅ ANÁLISIS Y REPORTES
        [StringLength(5)]
        public string? ABCClassification { get; set; } // Clasificación ABC (A, B, C)

        [StringLength(20)]
        public string? VelocityCode { get; set; } // "Rápido", "Medio", "Lento"

        [Precision(5, 2)]
        public decimal? ProfitMarginPercentage { get; set; } // Margen de ganancia

        public DateTime? LastSaleDate { get; set; } // Última fecha de venta

        [Precision(18, 4)]
        public decimal TotalSalesQuantity { get; set; } = 0; // Total vendido histórico

        // ✅ INFORMACIÓN ADICIONAL
        [StringLength(1000)]
        public string? InternalNotes { get; set; } // Notas internas

        [StringLength(500)]
        public string? CustomerNotes { get; set; } // Notas visibles al cliente

        [StringLength(1000)]
        public string? MaintenanceInstructions { get; set; } // Instrucciones de mantenimiento

        [StringLength(500)]
        public string? SafetyWarnings { get; set; } // Advertencias de seguridad

        // ✅ ESTADO Y CONFIGURACIÓN
        public bool IsActive { get; set; } = true;
        public bool IsService { get; set; } = false; // Producto vs Servicio
        public bool AllowFractionalQuantities { get; set; } = false;
        public bool TrackSerial { get; set; } = false; // Seguimiento de números de serie
        public bool TrackExpiry { get; set; } = false; // Seguimiento de fechas de vencimiento

        // ✅ INFORMACIÓN DE PROVEEDORES
        public int? PrimarySupplierId { get; set; }

        [ForeignKey("PrimarySupplierId")]
        public Supplier? PrimarySupplier { get; set; }

        [StringLength(100)]
        public string? SupplierCode { get; set; } // Código del producto en el proveedor

        // ✅ METADATOS
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public int? CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public User? CreatedBy { get; set; }

        public int? UpdatedByUserId { get; set; }

        [ForeignKey("UpdatedByUserId")]
        public User? UpdatedBy { get; set; }

        // ✅ RELACIONES - Comentadas temporalmente para evitar errores
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
