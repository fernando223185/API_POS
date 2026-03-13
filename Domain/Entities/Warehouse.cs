using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Entidad que representa un almacén dentro de una sucursal
    /// </summary>
    [Table("Warehouses")]
    public class Warehouse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Código único del almacén (generado automáticamente)
        /// Formato: ALM-001, ALM-002, etc.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del almacén
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
        /// ID de la sucursal a la que pertenece
        /// </summary>
        [Required]
        public int BranchId { get; set; }

        /// <summary>
        /// Tipo de almacén: General, Refrigerado, Materias Primas, Producto Terminado, etc.
        /// </summary>
        [MaxLength(50)]
        public string WarehouseType { get; set; } = "General";

        /// <summary>
        /// Ubicación física dentro de la sucursal
        /// </summary>
        [MaxLength(200)]
        public string? PhysicalLocation { get; set; }

        /// <summary>
        /// Capacidad máxima del almacén en m³
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxCapacity { get; set; }

        /// <summary>
        /// Capacidad actual utilizada en m³
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? CurrentCapacity { get; set; }

        /// <summary>
        /// Nombre del encargado del almacén
        /// </summary>
        [MaxLength(200)]
        public string? ManagerName { get; set; }

        /// <summary>
        /// Email del encargado
        /// </summary>
        [MaxLength(100)]
        public string? ManagerEmail { get; set; }

        /// <summary>
        /// Teléfono del encargado
        /// </summary>
        [MaxLength(20)]
        public string? ManagerPhone { get; set; }

        /// <summary>
        /// Indica si es el almacén principal de la sucursal
        /// </summary>
        public bool IsMainWarehouse { get; set; } = false;

        /// <summary>
        /// Indica si permite recibir mercancía
        /// </summary>
        public bool AllowsReceiving { get; set; } = true;

        /// <summary>
        /// Indica si permite despachar mercancía
        /// </summary>
        public bool AllowsShipping { get; set; } = true;

        /// <summary>
        /// Indica si requiere control de temperatura
        /// </summary>
        public bool RequiresTemperatureControl { get; set; } = false;

        /// <summary>
        /// Temperatura mínima permitida (°C)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? MinTemperature { get; set; }

        /// <summary>
        /// Temperatura máxima permitida (°C)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? MaxTemperature { get; set; }

        /// <summary>
        /// Baja lógica - Indica si el almacén está activo
        /// </summary>
        public bool IsActive { get; set; } = true;

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

        // Relaciones
        [ForeignKey(nameof(BranchId))]
        public virtual Branch Branch { get; set; } = null!;

        [ForeignKey(nameof(CreatedByUserId))]
        public virtual User? CreatedBy { get; set; }

        [ForeignKey(nameof(UpdatedByUserId))]
        public virtual User? UpdatedBy { get; set; }
    }
}
