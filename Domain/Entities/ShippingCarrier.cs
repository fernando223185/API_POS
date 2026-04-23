using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Paqueterías / Transportistas registrados por la empresa
    /// </summary>
    [Table("ShippingCarriers")]
    public class ShippingCarrier
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ── Empresa propietaria ──────────────────────────────────
        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }

        // ── Identificación ───────────────────────────────────────

        /// <summary>Código interno (FEDEX-00001, DHL-MX, ESTAFETA, etc.)</summary>
        [Required]
        [MaxLength(30)]
        public string Code { get; set; } = string.Empty;

        /// <summary>Nombre comercial de la paquetería</summary>
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        /// <summary>Descripción corta del servicio</summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        // ── Contacto ─────────────────────────────────────────────

        [MaxLength(100)]
        public string? ContactName { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(20)]
        public string? PhoneAlt { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(200)]
        public string? Website { get; set; }

        // ── Dirección ────────────────────────────────────────────

        [MaxLength(300)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(50)]
        public string? State { get; set; }

        [MaxLength(10)]
        public string? ZipCode { get; set; }

        [MaxLength(50)]
        public string? Country { get; set; } = "México";

        // ── Cuenta de cliente ────────────────────────────────────

        /// <summary>Número de cuenta que la empresa tiene con la paquetería</summary>
        [MaxLength(100)]
        public string? AccountNumber { get; set; }

        /// <summary>Nombre del ejecutivo de cuenta asignado</summary>
        [MaxLength(150)]
        public string? AccountRepName { get; set; }

        [MaxLength(20)]
        public string? AccountRepPhone { get; set; }

        [MaxLength(100)]
        public string? AccountRepEmail { get; set; }

        // ── Servicio ─────────────────────────────────────────────

        /// <summary>Tipos de servicio separados por coma: express,terrestre,maritimo,etc.</summary>
        [MaxLength(300)]
        public string? ServiceTypes { get; set; }

        /// <summary>Días hábiles estimados de entrega (default)</summary>
        public int? EstimatedDeliveryDays { get; set; }

        /// <summary>Cobertura geográfica: nacional, internacional, ambos</summary>
        [MaxLength(50)]
        public string? Coverage { get; set; } = "nacional";

        // ── Precios ──────────────────────────────────────────────

        /// <summary>Precio base por envío</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? BasePrice { get; set; }

        /// <summary>Precio por kg adicional</summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? PricePerKg { get; set; }

        /// <summary>Precio por kg adicional en envío express</summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? ExpressPricePerKg { get; set; }

        /// <summary>Peso máximo por paquete en kg</summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal? MaxWeightKg { get; set; }

        // ── Rastreo ──────────────────────────────────────────────

        /// <summary>URL base para rastreo de guías. Ej: https://fedex.com/track?id={guia}</summary>
        [MaxLength(500)]
        public string? TrackingUrl { get; set; }

        // ── Integración API ──────────────────────────────────────

        [MaxLength(300)]
        public string? ApiKey { get; set; }

        [MaxLength(300)]
        public string? ApiSecret { get; set; }

        [MaxLength(200)]
        public string? ApiEndpoint { get; set; }

        // ── Imagen / Logo ────────────────────────────────────────

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        // ── Estado ───────────────────────────────────────────────

        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
