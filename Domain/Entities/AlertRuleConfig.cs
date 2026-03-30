using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Regla que controla qué tipo de alerta va a qué rol dentro de una empresa.
    ///
    /// El developer define los AlertType disponibles (InvoiceDue, StockMin, StockCritical…).
    /// El admin configura desde la UI a qué rol se envía cada tipo y si está activo.
    ///
    /// Unique: (AlertType, CompanyId) — una sola regla por tipo por empresa.
    /// </summary>
    [Table("AlertRuleConfigs")]
    public class AlertRuleConfig
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ── Identificador del tipo de alerta (inmutable, definido por el developer) ─
        [Required]
        [MaxLength(30)]
        public string AlertType { get; set; } = string.Empty;

        /// <summary>Descripción legible que ve el admin en la UI.</summary>
        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        // ── Alcance ──────────────────────────────────────────────────────────────────
        [Required]
        public int CompanyId { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; } = null!;

        // ── Destinatario ──────────────────────────────────────────────────────────────
        /// <summary>
        /// Rol que recibe la alerta. NULL = broadcast a todos los usuarios de la empresa.
        /// El admin lo configura desde la UI; el default al auto-crear es NULL.
        /// </summary>
        public int? TargetRoleId { get; set; }

        [ForeignKey(nameof(TargetRoleId))]
        public Role? TargetRole { get; set; }

        // ── Control ───────────────────────────────────────────────────────────────────
        /// <summary>
        /// Si false, el job omite este tipo de alerta para esta empresa.
        /// </summary>
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedByUserId { get; set; }
    }
}
