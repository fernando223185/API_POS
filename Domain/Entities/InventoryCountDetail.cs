namespace Domain.Entities
{
    /// <summary>
    /// Detalle de productos a contar en una sesión de conteo de inventario.
    /// Registra cantidad del sistema, cantidad física contada y variaciones.
    /// </summary>
    public class InventoryCountDetail
    {
        public int Id { get; set; }

        /// <summary>
        /// ID de la sesión de conteo padre
        /// </summary>
        public int InventoryCountId { get; set; }

        /// <summary>
        /// ID del producto a contar
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Cantidad según el sistema al momento de crear el conteo
        /// </summary>
        public decimal SystemQuantity { get; set; }

        /// <summary>
        /// Cantidad física contada (null si aún no se ha contado)
        /// </summary>
        public decimal? PhysicalQuantity { get; set; }

        /// <summary>
        /// Diferencia entre físico y sistema (PhysicalQuantity - SystemQuantity)
        /// Se calcula automáticamente
        /// </summary>
        public decimal? Variance { get; set; }

        /// <summary>
        /// Porcentaje de variación ((Variance / SystemQuantity) * 100)
        /// </summary>
        public decimal? VariancePercentage { get; set; }

        /// <summary>
        /// Costo de la variación (Variance * UnitCost)
        /// </summary>
        public decimal? VarianceCost { get; set; }

        /// <summary>
        /// Costo unitario del producto al momento del conteo
        /// </summary>
        public decimal? UnitCost { get; set; }

        /// <summary>
        /// Estado del conteo de este producto: Pending, Counted, Recounted
        /// </summary>
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Notas específicas del producto (ej: "Producto dañado", "Ubicación incorrecta")
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// ID del usuario que realizó el conteo
        /// </summary>
        public int? CountedByUserId { get; set; }

        /// <summary>
        /// Fecha y hora en que se contó el producto
        /// </summary>
        public DateTime? CountedAt { get; set; }

        /// <summary>
        /// Indica si se solicitó un reconteo por discrepancias altas
        /// </summary>
        public bool RecountRequested { get; set; }

        /// <summary>
        /// ID del usuario que realizó el reconteo
        /// </summary>
        public int? RecountedByUserId { get; set; }

        /// <summary>
        /// Fecha del reconteo
        /// </summary>
        public DateTime? RecountedAt { get; set; }

        // Navegación
        public virtual InventoryCount InventoryCount { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
        public virtual User? CountedByUser { get; set; }
        public virtual User? RecountedByUser { get; set; }
    }
}
