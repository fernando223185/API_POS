using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Product
{
    public class CreateProductRequestDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Barcode { get; set; }

        [StringLength(100)]
        public string? Brand { get; set; }

        [StringLength(100)]
        public string? Model { get; set; }

        // Categorización
        public int? CategoryId { get; set; }
        public int? SubcategoryId { get; set; }

        // Información fiscal
        [StringLength(10)]
        public string? SatCode { get; set; } = "01010101";

        [StringLength(10)]
        public string? SatUnit { get; set; } = "PZA";

        // Precios y costos
        [Range(0, double.MaxValue)]
        public decimal BaseCost { get; set; } = 0;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, 1)]
        public decimal TaxRate { get; set; } = 0.16m;

        // Control de inventario
        [Range(0, double.MaxValue)]
        public decimal MinimumStock { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal MaximumStock { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal ReorderPoint { get; set; } = 0;

        [StringLength(20)]
        public string Unit { get; set; } = "PZA";

        // Características físicas
        public decimal? Weight { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(50)]
        public string? Size { get; set; }

        // Configuración
        public bool IsActive { get; set; } = true;
        public bool IsService { get; set; } = false;
        public bool AllowFractionalQuantities { get; set; } = false;
        public bool TrackSerial { get; set; } = false;
        public bool TrackExpiry { get; set; } = false;

        // Información de proveedor
        public int? PrimarySupplierId { get; set; }

        [StringLength(100)]
        public string? SupplierCode { get; set; }
    }

    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? SubcategoryId { get; set; }
        public string? SubcategoryName { get; set; }
        public string? SatCode { get; set; }
        public string? SatUnit { get; set; }
        public decimal BaseCost { get; set; }
        public decimal Price { get; set; }
        public decimal TaxRate { get; set; }
        public decimal PriceWithTax { get; set; }
        public decimal MinimumStock { get; set; }
        public decimal MaximumStock { get; set; }
        public decimal ReorderPoint { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal? Weight { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public bool IsActive { get; set; }
        public bool IsService { get; set; }
        public bool AllowFractionalQuantities { get; set; }
        public bool TrackSerial { get; set; }
        public bool TrackExpiry { get; set; }
        public int? PrimarySupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedByName { get; set; }
    }
}