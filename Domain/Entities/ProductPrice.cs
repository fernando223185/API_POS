using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities
{
    // Lista de precios (mayoreo, menudeo, VIP, etc.)
    public class PriceList
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // "Menudeo", "Mayoreo", "VIP"

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Precision(5, 4)]
        public decimal DefaultDiscountPercentage { get; set; } = 0; // Descuento por defecto

        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        // Relaciones
        public ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }

    // Precios específicos por producto y lista
    public class ProductPrice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Required]
        public int PriceListId { get; set; }

        [ForeignKey("PriceListId")]
        public PriceList PriceList { get; set; }

        [Required]
        [Precision(18, 4)]
        public decimal Price { get; set; }

        [Precision(5, 4)]
        public decimal DiscountPercentage { get; set; } = 0;

        [NotMapped]
        public decimal FinalPrice => Price * (1 - DiscountPercentage / 100);

        public DateTime ValidFrom { get; set; } = DateTime.UtcNow;
        public DateTime? ValidTo { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public int CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public User CreatedBy { get; set; }
    }
}