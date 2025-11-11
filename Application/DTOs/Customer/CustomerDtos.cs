using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Customer
{
    public class CreateCustomerRequestDto
    {
        // ? CODE REMOVIDO - Se genera automáticamente en el backend
        // El sistema generará códigos como: JUAN001, WMART001, CLI001, etc.

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        // Información fiscal básica
        [Required]
        [StringLength(20)]
        public string TaxId { get; set; } = string.Empty; // RFC

        // Dirección detallada (campos originales)
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

        // ? NUEVOS CAMPOS ERP AVANZADOS

        // Información fiscal para CFDI
        [StringLength(200)]
        public string? CompanyName { get; set; } // Razón social (si es empresa, se usa para generar código)

        [StringLength(10)]
        public string? SatTaxRegime { get; set; } // Régimen fiscal SAT (601, 603, etc.)

        [StringLength(10)]
        public string? SatCfdiUse { get; set; } = "G03"; // Uso del CFDI por defecto

        // Lista de precios y configuraciones comerciales
        public int? PriceListId { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercentage { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal CreditLimit { get; set; } = 0;

        [Range(0, 365)]
        public int PaymentTermsDays { get; set; } = 0; // Días de crédito

        public bool IsActive { get; set; } = true;
    }

    public class CustomerResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty; // ? SE DEVUELVE el código generado
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Commentary { get; set; } = string.Empty;
        public int CountryId { get; set; }
        public int StateId { get; set; }
        public string InteriorNumber { get; set; } = string.Empty;
        public string ExteriorNumber { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public DateTime CreatedAt { get; set; }

        // ? CAMPOS ERP AVANZADOS
        public string? CompanyName { get; set; }
        public string? SatTaxRegime { get; set; }
        public string? SatCfdiUse { get; set; }
        public int? PriceListId { get; set; }
        public string? PriceListName { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal CreditLimit { get; set; }
        public int PaymentTermsDays { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedByName { get; set; }
    }
}