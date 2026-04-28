namespace Domain.Entities
{
    /// <summary>
    /// Sesión de conteo de inventario (cíclico, general, por categoría, etc.)
    /// Permite verificar físicamente el stock y detectar discrepancias automáticamente.
    /// </summary>
    public class InventoryCount
    {
        public int Id { get; set; }

        /// <summary>
        /// Código único del conteo (CIC-000001, CIC-000002, etc.)
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// ID del almacén donde se realiza el conteo
        /// </summary>
        public int WarehouseId { get; set; }

        /// <summary>
        /// Tipo de conteo: CYCLE (cíclico), FULL (completo), CATEGORY (por categoría), LOCATION (por ubicación)
        /// </summary>
        public string CountType { get; set; } = string.Empty;

        /// <summary>
        /// Estado del conteo: Draft, InProgress, Completed, Cancelled
        /// </summary>
        public string Status { get; set; } = "Draft";

        /// <summary>
        /// Fecha programada para realizar el conteo
        /// </summary>
        public DateTime ScheduledDate { get; set; }

        /// <summary>
        /// Fecha y hora de inicio del conteo
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// Fecha y hora de finalización del conteo
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// ID del usuario asignado para realizar el conteo
        /// </summary>
        public int AssignedToUserId { get; set; }

        /// <summary>
        /// ID del usuario que aprobó el conteo completado
        /// </summary>
        public int? ApprovedByUserId { get; set; }

        /// <summary>
        /// Fecha de aprobación
        /// </summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// ID de categoría (opcional, solo para conteos por categoría)
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// Notas generales del conteo
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Total de productos que deben ser contados
        /// </summary>
        public int TotalProducts { get; set; }

        /// <summary>
        /// Cantidad de productos ya contados
        /// </summary>
        public int CountedProducts { get; set; }

        /// <summary>
        /// Cantidad de productos que tienen diferencias (variance != 0)
        /// </summary>
        public int ProductsWithVariance { get; set; }

        /// <summary>
        /// Costo total de las diferencias encontradas (puede ser positivo o negativo)
        /// </summary>
        public decimal TotalVarianceCost { get; set; }

        /// <summary>
        /// ID del usuario que creó la sesión de conteo
        /// </summary>
        public int CreatedByUserId { get; set; }

        /// <summary>
        /// Fecha de creación
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// ID de la compañía
        /// </summary>
        public int? CompanyId { get; set; }

        // Navegación
        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual User AssignedToUser { get; set; } = null!;
        public virtual User? ApprovedByUser { get; set; }
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual ProductCategory? Category { get; set; }
        public virtual Company? Company { get; set; }
        public virtual ICollection<InventoryCountDetail> Details { get; set; } = new List<InventoryCountDetail>();
    }
}
