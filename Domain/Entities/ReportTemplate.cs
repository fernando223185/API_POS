using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Plantilla de reporte personalizable por secciones.
    /// El campo SectionsJson guarda la configuración de secciones en formato JSON.
    /// </summary>
    [Table("ReportTemplates")]
    public class ReportTemplate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Nombre descriptivo de la plantilla (ej. "Reporte de ventas mensual")
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de reporte: Sales, Delivery, Quotation, Purchase, Inventory, CashierShift
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string ReportType { get; set; } = string.Empty;

        /// <summary>
        /// Indica si esta es la plantilla predeterminada para el tipo de reporte
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Indica si la plantilla está activa
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Configuración de secciones en JSON:
        /// [{ "type": "Header|Table|Summary|Footer", "title": "...", "order": 1,
        ///    "fields": [{ "field": "key", "label": "...", "bold": false, "fontSize": 9, "align": "left", "format": "text|currency|date|number" }],
        ///    "columns": [{ "field": "key", "label": "...", "width": 80, "align": "left", "format": "text|currency|date|number" }] }]
        /// </summary>
        [Required]
        public string SectionsJson { get; set; } = "[]";

        /// <summary>
        /// Descripción opcional de la plantilla
        /// </summary>
        [MaxLength(300)]
        public string? Description { get; set; }

        /// <summary>
        /// Empresa a la que pertenece la plantilla (null = global/sistema)
        /// </summary>
        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }

        /// <summary>
        /// Usuario que creó la plantilla
        /// </summary>
        public int? CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public User? CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
