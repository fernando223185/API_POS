using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Product
{
    public class CreateProductCategoryDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "El código es obligatorio")]
        [StringLength(50, ErrorMessage = "El código no puede exceder 50 caracteres")]
        public string Code { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateProductCategoryDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "El código es obligatorio")]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class ProductCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductsCount { get; set; }
        public int SubcategoriesCount { get; set; }
    }

    public class ProductSubcategoryBriefDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class ProductCategoryDetailDto : ProductCategoryDto
    {
        public List<ProductSubcategoryBriefDto> Subcategories { get; set; } = new();
    }

    public class ProductCategoryDropdownDto
    {
        public int Value { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class ProductCategoryStatsDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public int ProductsCount { get; set; }
        public int TotalProducts { get; set; }
        public int SubcategoriesCount { get; set; }
        public decimal AvgPrice { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class ProductCategoryStatsSummaryDto
    {
        public int TotalCategories { get; set; }
        public int TotalProducts { get; set; }
        public decimal TotalValue { get; set; }
        public double AvgProductsPerCategory { get; set; }
    }

    public class ProductCategoryStatsResultDto
    {
        public List<ProductCategoryStatsDto> CategoryStats { get; set; } = new();
        public ProductCategoryStatsSummaryDto Summary { get; set; } = new();
    }
}
