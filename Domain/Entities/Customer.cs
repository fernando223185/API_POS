using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities
{
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        // Información básica
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        // Información fiscal básica
        [Required]
        [StringLength(20)]
        public string TaxId { get; set; } = string.Empty; // RFC

        // Dirección detallada (campos existentes en tabla original)
        [Required]
        [StringLength(10)]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Commentary { get; set; } = string.Empty;

        [Required]
        public int CountryId { get; set; }
        
        [Required]
        public int StateId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string InteriorNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string ExteriorNumber { get; set; } = string.Empty;
        
        [Required]
        public int StatusId { get; set; }

        // ✅ NUEVOS CAMPOS ERP AVANZADOS (después de ejecutar el SQL)
        
        // Información fiscal para CFDI
        [StringLength(200)]
        public string? CompanyName { get; set; } // Razón social

        [StringLength(10)]
        public string? SatTaxRegime { get; set; } // Régimen fiscal SAT (601, 603, etc.)

        [StringLength(10)]
        public string? SatCfdiUse { get; set; } = "G03"; // Uso del CFDI

        // Configuraciones comerciales
        public int? PriceListId { get; set; }

        [ForeignKey("PriceListId")]
        public PriceList? PriceList { get; set; }

        // ✅ CORREGIDO: Cambiar de Precision(5,4) a Precision(6,4) para permitir hasta 99.9999
        [Precision(6, 4)]
        public decimal DiscountPercentage { get; set; } = 0;

        [Precision(18, 2)]
        public decimal CreditLimit { get; set; } = 0;

        public int PaymentTermsDays { get; set; } = 0; // Días de crédito

        // Control y auditoría
        public bool IsActive { get; set; } = true;

        // Fechas (mapear a campos correctos)
        [Required]
        [Column("Created_at")]  // Campo original en la tabla
        public DateTime CreatedAtOriginal { get; set; } = DateTime.UtcNow;

        [Column("CreatedAt")]  // Nuevo campo agregado
        public DateTime? CreatedAt { get; set; }

        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }

        // Relaciones de auditoría
        [Column("CreatedByUserId")]
        public int? CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public User? CreatedBy { get; set; }

        [Column("UpdatedByUserId")]
        public int? UpdatedByUserId { get; set; }

        [ForeignKey("UpdatedByUserId")]
        public User? UpdatedBy { get; set; }
    }
}

