using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Empresa - Entidad principal del sistema que agrupa sucursales
    /// Configuración fiscal, comercial y operativa
    /// </summary>
    [Table("Companies")]
    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Código único de la empresa (generado automáticamente)
        /// Formato: COMP-001, COMP-002, etc.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Razón social (nombre legal de la empresa)
        /// </summary>
        [Required]
        [MaxLength(300)]
        public string LegalName { get; set; } = string.Empty;

        /// <summary>
        /// Nombre comercial (nombre de fantasía)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string TradeName { get; set; } = string.Empty;

        // ============================================
        // INFORMACIÓN FISCAL
        // ============================================

        /// <summary>
        /// RFC (Registro Federal de Contribuyentes)
        /// </summary>
        [Required]
        [MaxLength(13)]
        public string TaxId { get; set; } = string.Empty;

        /// <summary>
        /// Régimen fiscal SAT
        /// Ejemplos: 601 (General), 603 (Personas Morales), 605 (Sueldos), etc.
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string SatTaxRegime { get; set; } = "601";

        /// <summary>
        /// Código postal fiscal
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string FiscalZipCode { get; set; } = string.Empty;

        /// <summary>
        /// Dirección fiscal completa
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string FiscalAddress { get; set; } = string.Empty;

        // ============================================
        // INFORMACIÓN DE CONTACTO
        // ============================================

        /// <summary>
        /// Teléfono principal
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Email principal
        /// </summary>
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Sitio web
        /// </summary>
        [MaxLength(200)]
        public string? Website { get; set; }

        // ============================================
        // CONFIGURACIÓN DE CFDI
        // ============================================

        /// <summary>
        /// Serie para facturas (Prefijo)
        /// Ejemplo: "A", "B", "FAC", etc.
        /// </summary>
        [MaxLength(10)]
        public string? InvoiceSeries { get; set; }

        /// <summary>
        /// Folio inicial para facturas
        /// </summary>
        public int InvoiceStartingFolio { get; set; } = 1;

        /// <summary>
        /// Folio actual (se incrementa automáticamente)
        /// </summary>
        public int InvoiceCurrentFolio { get; set; } = 1;

        /// <summary>
        /// Ruta del certificado SAT (.cer)
        /// </summary>
        [MaxLength(500)]
        public string? SatCertificatePath { get; set; }

        /// <summary>
        /// Ruta de la llave privada SAT (.key)
        /// </summary>
        [MaxLength(500)]
        public string? SatKeyPath { get; set; }

        /// <summary>
        /// Contraseña de la llave privada (encriptada)
        /// </summary>
        [MaxLength(500)]
        public string? SatKeyPassword { get; set; }

        // ============================================
        // CONFIGURACIÓN COMERCIAL
        // ============================================

        /// <summary>
        /// Moneda predeterminada
        /// Ejemplos: MXN, USD, EUR
        /// </summary>
        [Required]
        [MaxLength(3)]
        public string DefaultCurrency { get; set; } = "MXN";

        /// <summary>
        /// Logo de la empresa (URL de S3 o ruta local)
        /// </summary>
        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Eslogan o tagline
        /// </summary>
        [MaxLength(200)]
        public string? Slogan { get; set; }

        // ============================================
        // CONTROL Y ESTADO
        // ============================================

        /// <summary>
        /// Indica si la empresa está activa
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Indica si es la empresa principal/matriz
        /// </summary>
        public bool IsMainCompany { get; set; } = false;

        /// <summary>
        /// Notas adicionales
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // ============================================
        // AUDITORÍA
        // ============================================

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        public int? CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }

        // ============================================
        // RELACIONES
        // ============================================

        /// <summary>
        /// Usuario que creó el registro
        /// </summary>
        [ForeignKey(nameof(CreatedByUserId))]
        public virtual User? CreatedBy { get; set; }

        /// <summary>
        /// Usuario que actualizó el registro
        /// </summary>
        [ForeignKey(nameof(UpdatedByUserId))]
        public virtual User? UpdatedBy { get; set; }

        /// <summary>
        /// Sucursales de la empresa
        /// </summary>
        public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
    }
}
