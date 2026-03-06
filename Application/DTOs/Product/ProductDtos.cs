using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Product
{
    public class CreateProductRequestDto
    {
        // ? INFORMACIÓN BÁSICA
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

        // ? CLASIFICACIÓN
        public int? CategoryId { get; set; }
        public int? SubcategoryId { get; set; }

        // ? INFORMACIÓN FISCAL
        [StringLength(10)]
        public string SatCode { get; set; } = "01010101";

        [StringLength(10)]
        public string SatUnit { get; set; } = "PZA";

        [StringLength(20)]
        public string SatTaxType { get; set; } = "Tasa";

        [StringLength(20)]
        public string? CustomsCode { get; set; }

        [StringLength(50)]
        public string CountryOfOrigin { get; set; } = "México";

        // ? PRECIOS Y COSTOS
        [Range(0, double.MaxValue)]
        public decimal BaseCost { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; } = 0;

        [Range(0, 1)]
        public decimal TaxRate { get; set; } = 0.16m;

        // ? INVENTARIO
        [Range(0, double.MaxValue)]
        public decimal MinimumStock { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal MaximumStock { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal ReorderPoint { get; set; } = 0;

        [StringLength(20)]
        public string Unit { get; set; } = "PZA";

        // ? CARACTERÍSTICAS FÍSICAS
        public decimal? Weight { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(50)]
        public string? Size { get; set; }

        // ? INFORMACIÓN COMERCIAL AVANZADA
        public int? Warranty { get; set; }

        [StringLength(50)]
        public string? WarrantyType { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(20)]
        public string? Aisle { get; set; }

        [StringLength(20)]
        public string? Shelf { get; set; }

        [StringLength(20)]
        public string? Bin { get; set; }

        // ? CLASIFICACIÓN AVANZADA
        [StringLength(500)]
        public string? Tags { get; set; }

        [StringLength(50)]
        public string? Season { get; set; }

        [StringLength(20)]
        public string? TargetGender { get; set; }

        [StringLength(50)]
        public string? AgeGroup { get; set; }

        // ? INFORMACIÓN DE VENTAS
        public decimal? MaxQuantityPerSale { get; set; }
        public decimal MinQuantityPerSale { get; set; } = 1;

        [StringLength(500)]
        public string? SalesNotes { get; set; }

        public bool IsDiscountAllowed { get; set; } = true;
        public decimal? MaxDiscountPercentage { get; set; }

        // ? E-COMMERCE Y MARKETING
        [StringLength(200)]
        public string? SEOTitle { get; set; }

        [StringLength(500)]
        public string? SEODescription { get; set; }

        [StringLength(300)]
        public string? SEOKeywords { get; set; }

        public bool IsWebVisible { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public DateTime? LaunchDate { get; set; }
        public DateTime? DiscontinuedDate { get; set; }

        // ? INFORMACIÓN TÉCNICA
        [StringLength(2000)]
        public string? TechnicalSpecs { get; set; }

        [StringLength(100)]
        public string? ManufacturerPartNumber { get; set; }

        [StringLength(50)]
        public string? UPC { get; set; }

        [StringLength(50)]
        public string? EAN { get; set; }

        [StringLength(50)]
        public string? ISBN { get; set; }

        // ? LOGÍSTICA Y ENVÍO
        public bool IsFragile { get; set; } = false;
        public bool RequiresSpecialHandling { get; set; } = false;

        [StringLength(50)]
        public string? ShippingClass { get; set; }

        public decimal? PackageLength { get; set; }
        public decimal? PackageWidth { get; set; }
        public decimal? PackageHeight { get; set; }
        public decimal? PackageWeight { get; set; }

        // ? CONTROL DE CALIDAD
        [StringLength(20)]
        public string? QualityGrade { get; set; }

        public DateTime? LastQualityCheck { get; set; }
        public decimal? DefectRate { get; set; }
        public decimal? ReturnRate { get; set; }

        // ? ANÁLISIS Y REPORTES
        [StringLength(5)]
        public string? ABCClassification { get; set; }

        [StringLength(20)]
        public string? VelocityCode { get; set; }

        public decimal? ProfitMarginPercentage { get; set; }
        public DateTime? LastSaleDate { get; set; }
        public decimal TotalSalesQuantity { get; set; } = 0;

        // ? INFORMACIÓN ADICIONAL
        [StringLength(1000)]
        public string? InternalNotes { get; set; }

        [StringLength(500)]
        public string? CustomerNotes { get; set; }

        [StringLength(1000)]
        public string? MaintenanceInstructions { get; set; }

        [StringLength(500)]
        public string? SafetyWarnings { get; set; }

        // ? CONFIGURACIÓN
        public bool IsActive { get; set; } = true;
        public bool IsService { get; set; } = false;
        public bool AllowFractionalQuantities { get; set; } = false;
        public bool TrackSerial { get; set; } = false;
        public bool TrackExpiry { get; set; } = false;

        // ? PROVEEDORES
        public int? PrimarySupplierId { get; set; }

        [StringLength(100)]
        public string? SupplierCode { get; set; }
    }

    public class ProductResponseDto
    {
        public int ID { get; set; } // ? CORREGIDO: ID en lugar de Id
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }

        // Clasificación
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? SubcategoryId { get; set; }
        public string? SubcategoryName { get; set; }

        // Precios y costos
        public decimal BaseCost { get; set; }
        public decimal Price { get; set; }
        public decimal TaxRate { get; set; }
        public decimal PriceWithTax { get; set; } // ? CORREGIDO: Campo normal en lugar de readonly

        // Inventario
        public decimal MinimumStock { get; set; }
        public decimal MaximumStock { get; set; }
        public decimal ReorderPoint { get; set; }
        public string Unit { get; set; } = string.Empty;

        // Información fiscal
        public string SatCode { get; set; } = string.Empty;
        public string SatUnit { get; set; } = string.Empty;
        public string SatTaxType { get; set; } = string.Empty;
        public string? CustomsCode { get; set; }
        public string CountryOfOrigin { get; set; } = string.Empty;

        // Características físicas
        public decimal? Weight { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }

        // Información comercial avanzada
        public int? Warranty { get; set; }
        public string? WarrantyType { get; set; }
        public string? Location { get; set; }
        public string? Aisle { get; set; }
        public string? Shelf { get; set; }
        public string? Bin { get; set; }

        // Clasificación avanzada
        public string? Tags { get; set; }
        public string? Season { get; set; }
        public string? TargetGender { get; set; }
        public string? AgeGroup { get; set; }

        // Información de ventas
        public decimal? MaxQuantityPerSale { get; set; }
        public decimal MinQuantityPerSale { get; set; }
        public string? SalesNotes { get; set; }
        public bool IsDiscountAllowed { get; set; }
        public decimal? MaxDiscountPercentage { get; set; }

        // E-commerce y marketing
        public string? SEOTitle { get; set; }
        public string? SEODescription { get; set; }
        public string? SEOKeywords { get; set; }
        public bool IsWebVisible { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime? LaunchDate { get; set; }
        public DateTime? DiscontinuedDate { get; set; }

        // Información técnica
        public string? TechnicalSpecs { get; set; }
        public string? ManufacturerPartNumber { get; set; }
        public string? UPC { get; set; }
        public string? EAN { get; set; }
        public string? ISBN { get; set; }

        // Logística y envío
        public bool IsFragile { get; set; }
        public bool RequiresSpecialHandling { get; set; }
        public string? ShippingClass { get; set; }
        public decimal? PackageLength { get; set; }
        public decimal? PackageWidth { get; set; }
        public decimal? PackageHeight { get; set; }
        public decimal? PackageWeight { get; set; }

        // Control de calidad
        public string? QualityGrade { get; set; }
        public DateTime? LastQualityCheck { get; set; }
        public decimal? DefectRate { get; set; }
        public decimal? ReturnRate { get; set; }

        // Análisis y reportes
        public string? ABCClassification { get; set; }
        public string? VelocityCode { get; set; }
        public decimal? ProfitMarginPercentage { get; set; }
        public DateTime? LastSaleDate { get; set; }
        public decimal TotalSalesQuantity { get; set; }

        // Información adicional
        public string? InternalNotes { get; set; }
        public string? CustomerNotes { get; set; }
        public string? MaintenanceInstructions { get; set; }
        public string? SafetyWarnings { get; set; }

        // Configuración
        public bool IsActive { get; set; }
        public bool IsService { get; set; }
        public bool AllowFractionalQuantities { get; set; }
        public bool TrackSerial { get; set; }
        public bool TrackExpiry { get; set; }

        // Proveedores
        public int? PrimarySupplierId { get; set; }
        public string? PrimarySupplierName { get; set; }
        public string? SupplierCode { get; set; }

        // Metadatos
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public string? UpdatedByName { get; set; }
    }

    // ? DTO específico para tabla de productos (optimizado para frontend)
    public class ProductTableDto
    {
        public int ID { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? CategoryName { get; set; }
        public decimal Price { get; set; }
        public decimal BaseCost { get; set; }
        public decimal ProfitMargin => Price > 0 ? ((Price - BaseCost) / Price) * 100 : 0;
        public decimal MinimumStock { get; set; }
        public decimal MaximumStock { get; set; }
        public string? Location { get; set; }
        public bool IsActive { get; set; }
        public string Status => IsActive ? "Activo" : "Inactivo";
        public string StatusColor => IsActive ? "green" : "red";
        public DateTime CreatedAt { get; set; }
        public string FormattedCreatedAt => CreatedAt.ToString("dd/MM/yyyy");
        public string? ABCClassification { get; set; }
        public string? VelocityCode { get; set; }
        public DateTime? LastSaleDate { get; set; }
        public decimal TotalSalesQuantity { get; set; }
    }

    // ? DTO DE RESPUESTA PAGINADA
    public class GetProductsPagedResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; } = 0;
        public List<ProductTableDto> Data { get; set; } = new();
        public PaginationMetadata Pagination { get; set; } = new();
        public ProductsStatistics Statistics { get; set; } = new();
    }

    public class PaginationMetadata
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    public class ProductsStatistics
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int InactiveProducts { get; set; }
        public decimal TotalValue { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public List<CategoryStats> TopCategories { get; set; } = new();
    }

    public class CategoryStats
    {
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public decimal TotalValue { get; set; }
    }
}