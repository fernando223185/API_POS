namespace Domain.Entities
{
    /// <summary>
    /// Detalle de productos incluidos en un ajuste de inventario.
    /// Registra la cantidad en sistema vs cantidad física contada.
    /// </summary>
    public class StockAdjustmentDetail
    {
        public int Id { get; set; }
        
        public int StockAdjustmentId { get; set; }
        public StockAdjustment StockAdjustment { get; set; } = null!;
        
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        /// <summary>
        /// Cantidad que indicaba el sistema ANTES del ajuste
        /// </summary>
        public decimal SystemQuantity { get; set; }
        
        /// <summary>
        /// Cantidad física real contada o corregida
        /// </summary>
        public decimal PhysicalQuantity { get; set; }
        
        /// <summary>
        /// Diferencia: PhysicalQuantity - SystemQuantity
        /// Positivo = Entrada (había más de lo que decía el sistema)
        /// Negativo = Salida (había menos de lo que decía el sistema)
        /// </summary>
        public decimal AdjustmentQuantity { get; set; }
        
        /// <summary>
        /// Costo promedio del producto al momento del ajuste
        /// </summary>
        public decimal? UnitCost { get; set; }
        
        /// <summary>
        /// Costo total del ajuste: AdjustmentQuantity * UnitCost
        /// </summary>
        public decimal? TotalCost { get; set; }
        
        /// <summary>
        /// Notas específicas de este producto (ej: "Dañado por agua", "Vencido 15/04/2026")
        /// </summary>
        public string? Notes { get; set; }
    }
}
