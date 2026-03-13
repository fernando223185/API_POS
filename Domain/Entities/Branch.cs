using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Entidad que representa una sucursal o punto de venta
    /// </summary>
    [Table("Branches")]
    public class Branch
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Código único de la sucursal (generado automáticamente)
        /// Formato: SUC-001, SUC-002, etc.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de la sucursal
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción o notas adicionales
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Dirección completa
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Ciudad
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Estado
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string State { get; set; } = string.Empty;

        /// <summary>
        /// Código postal
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string ZipCode { get; set; } = string.Empty;

        /// <summary>
        /// País
        /// </summary>
        [MaxLength(100)]
        public string Country { get; set; } = "México";

        /// <summary>
        /// Teléfono principal
        /// </summary>
        [MaxLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// Email de contacto
        /// </summary>
        [MaxLength(100)]
        public string? Email { get; set; }

        /// <summary>
        /// Nombre del encargado/gerente
        /// </summary>
        [MaxLength(200)]
        public string? ManagerName { get; set; }

        /// <summary>
        /// Indica si es la sucursal principal (matriz)
        /// </summary>
        public bool IsMainBranch { get; set; } = false;

        /// <summary>
        /// Baja lógica - Indica si la sucursal está activa
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Fecha de apertura de la sucursal
        /// </summary>
        public DateTime? OpeningDate { get; set; }

        /// <summary>
        /// Fecha de creación del registro
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Usuario que creó el registro
        /// </summary>
        public int? CreatedByUserId { get; set; }

        /// <summary>
        /// Usuario que actualizó el registro
        /// </summary>
        public int? UpdatedByUserId { get; set; }

        /// <summary>
        /// ID de la empresa a la que pertenece la sucursal
        /// </summary>
        public int? CompanyId { get; set; }

        // Relaciones
        [ForeignKey(nameof(CreatedByUserId))]
        public virtual User? CreatedBy { get; set; }

        [ForeignKey(nameof(UpdatedByUserId))]
        public virtual User? UpdatedBy { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public virtual Company? Company { get; set; }
    }
}
