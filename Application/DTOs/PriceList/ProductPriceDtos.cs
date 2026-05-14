using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.PriceList
{
    public class ProductPriceDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductCode { get; set; }
        public int PriceListId { get; set; }
        public string? PriceListName { get; set; }
        public string? PriceListCode { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal FinalPrice { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
    }

    public class CreateProductPriceDto
    {
        [Required(ErrorMessage = "ProductId es obligatorio")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "PriceListId es obligatorio")]
        public int PriceListId { get; set; }

        [Required(ErrorMessage = "Price es obligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo")]
        public decimal Price { get; set; }

        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100")]
        public decimal DiscountPercentage { get; set; } = 0;
    }

    public class UpdateProductPriceDto
    {
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo")]
        public decimal Price { get; set; }

        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100")]
        public decimal DiscountPercentage { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class BulkProductPriceItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo")]
        public decimal Price { get; set; }

        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100")]
        public decimal DiscountPercentage { get; set; } = 0;
    }

    public class BulkProductPricesDto
    {
        [Required(ErrorMessage = "PriceListId es obligatorio")]
        public int PriceListId { get; set; }

        [Required(ErrorMessage = "Items es obligatorio")]
        [MinLength(1, ErrorMessage = "Se requiere al menos un item")]
        public List<BulkProductPriceItemDto> Items { get; set; } = new();
    }

    public class BulkProductPricesResultDto
    {
        public int PriceListId { get; set; }
        public int TotalReceived { get; set; }
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public List<int> SkippedProductIds { get; set; } = new();
    }
}
