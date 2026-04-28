namespace Domain.Entities
{
    /// <summary>
    /// Registro de ajustes de inventario para corregir discrepancias entre stock físico y sistema.
    /// Ejemplos: inventario físico, mermas, daños, robos, productos vencidos.
    /// </summary>
    public class StockAdjustment
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Código único del ajuste (ej: ADJ-001, ADJ-002)
        /// </summary>
        public string Code { get; set; } = string.Empty;
        
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;
        
        /// <summary>
        /// Fecha en que se realizó el ajuste
        /// </summary>
        public DateTime AdjustmentDate { get; set; }
        
        /// <summary>
        /// Razón del ajuste: PHYSICAL_COUNT, DAMAGE, LOSS, EXPIRATION, ERROR, SAMPLE, PRODUCTION_WASTE, OTHER
        /// </summary>
        public string Reason { get; set; } = string.Empty;
        
        /// <summary>
        /// Notas adicionales explicando el ajuste
        /// </summary>
        public string? Notes { get; set; }
        
        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Detalles de los productos ajustados
        /// </summary>
        public List<StockAdjustmentDetail> Details { get; set; } = new();
        
        public int? CompanyId { get; set; }
        public Company? Company { get; set; }
    }
}
