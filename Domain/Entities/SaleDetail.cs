using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Detalle de venta (productos vendidos)
    /// </summary>
    [Table("SaleDetails")]
    public class SaleDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Venta a la que pertenece
        /// </summary>
        [Required]
        public int SaleId { get; set; }

        [ForeignKey("SaleId")]
        public Sale Sale { get; set; } = null!;

        /// <summary>
        /// Producto vendido
        /// </summary>
        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        // ========================================
        // INFORMACIÓN DENORMALIZADA (AUDITORÍA)
        // ========================================

        /// <summary>
        /// Código del producto (para auditoría)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string ProductCode { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del producto (para auditoría)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        // ========================================
        // CANTIDADES Y PRECIOS
        // ========================================

        /// <summary>
        /// Cantidad vendida
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        [Required]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Precio unitario
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        [Required]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Porcentaje de descuento aplicado al producto
        /// </summary>
        [Column(TypeName = "decimal(6,4)")]
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Monto de descuento aplicado
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Porcentaje de impuesto (IVA)
        /// </summary>
        [Column(TypeName = "decimal(6,4)")]
        public decimal TaxPercentage { get; set; } = 0.16m;

        /// <summary>
        /// Monto de impuestos
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Required]
        public decimal TaxAmount { get; set; }

        // ========================================
        // MONTOS TOTALES
        // ========================================

        /// <summary>
        /// Subtotal (cantidad * precio - descuento)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Required]
        public decimal SubTotal { get; set; }

        /// <summary>
        /// Total (subtotal + impuestos)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Required]
        public decimal Total { get; set; }

        // ========================================
        // COSTOS (PARA CÁLCULO DE UTILIDAD)
        // ========================================

        /// <summary>
        /// Costo unitario del producto
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? UnitCost { get; set; }

        /// <summary>
        /// Costo total (cantidad * costo unitario)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalCost { get; set; }

        // ========================================
        // INFORMACIÓN ADICIONAL
        // ========================================

        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Número de serie (si el producto lo requiere)
        /// </summary>
        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        /// <summary>
        /// Número de lote (si el producto lo requiere)
        /// </summary>
        [MaxLength(100)]
        public string? LotNumber { get; set; }
    }
}
