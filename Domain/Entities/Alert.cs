using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Alerta del sistema generada por jobs programados.
    /// Cubre: facturas próximas a vencer y stock mínimo/crítico.
    /// </summary>
    [Table("Alerts")]
    public class Alert
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ── Tipo y severidad ─────────────────────────────────────────────
        /// <summary>InvoiceDue | StockMin | StockCritical</summary>
        [Required]
        [MaxLength(30)]
        public string Type { get; set; } = string.Empty;

        /// <summary>Info | Warning | Critical</summary>
        [Required]
        [MaxLength(10)]
        public string Severity { get; set; } = "Warning";

        // ── Referencia a la entidad que disparó la alerta ────────────────
        /// <summary>Id de la factura o del ProductStock</summary>
        public int? ReferenceId { get; set; }

        /// <summary>Invoice | ProductStock</summary>
        [MaxLength(30)]
        public string? ReferenceType { get; set; }

        // ── Contexto ─────────────────────────────────────────────────────
        public int? CompanyId { get; set; }
        public int? BranchId { get; set; }

        /// <summary>Si la alerta está dirigida a un usuario en concreto</summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Si la alerta está dirigida a todos los usuarios de un rol.
        /// Ejemplo: StockMin → RoleId de "Almacén"; InvoiceDue → RoleId de "Cobranza".
        /// Tiene prioridad sobre UserId cuando ambos son null.
        /// </summary>
        public int? TargetRoleId { get; set; }

        // ── Contenido ────────────────────────────────────────────────────
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        // ── Deduplicación ────────────────────────────────────────────────
        /// <summary>
        /// Clave única para evitar duplicar la misma alerta en múltiples
        /// ejecuciones del job. Ejemplo: INVOICEDUE_INV_42_STAGE_3DAYS
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string UniqueKey { get; set; } = string.Empty;

        // ── Estado y ciclo de vida ────────────────────────────────────────
        /// <summary>Pending | Read | Resolved</summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        /// <summary>Última vez que el job confirmó que la condición sigue activa</summary>
        public DateTime LastDetectedAt { get; set; } = DateTime.UtcNow;
    }
}
