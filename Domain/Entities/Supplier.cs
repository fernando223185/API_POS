using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities
{
    // Proveedores
    public class Supplier
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [StringLength(20)]
        public string? TaxId { get; set; } // RFC

        [StringLength(200)]
        public string? ContactPerson { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? State { get; set; }

        [StringLength(10)]
        public string? ZipCode { get; set; }

        [StringLength(50)]
        public string? Country { get; set; } = "México";

        // Condiciones comerciales
        public int PaymentTermsDays { get; set; } = 30;

        [Precision(18, 2)]
        public decimal CreditLimit { get; set; } = 0;

        [Precision(5, 4)]
        public decimal DefaultDiscountPercentage { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Relaciones
        public ICollection<Product> PrimaryProducts { get; set; } = new List<Product>();
        public ICollection<ProductSupplier> ProductSuppliers { get; set; } = new List<ProductSupplier>();
    }

    // Relación muchos a muchos entre productos y proveedores
    public class ProductSupplier
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Required]
        public int SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public Supplier Supplier { get; set; }

        [StringLength(100)]
        public string? SupplierProductCode { get; set; } // Código del producto en el proveedor

        [Precision(18, 4)]
        public decimal Cost { get; set; } = 0;

        [Precision(18, 4)]
        public decimal MinimumOrderQuantity { get; set; } = 1;

        public int LeadTimeDays { get; set; } = 7; // Días de entrega

        public bool IsPreferred { get; set; } = false;
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}