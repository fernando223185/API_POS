using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Detalle de factura (conceptos del CFDI)
    /// </summary>
    [Table("InvoiceDetails")]
    public class InvoiceDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Factura a la que pertenece
        /// </summary>
        [Required]
        public int InvoiceId { get; set; }

        [ForeignKey("InvoiceId")]
        public Invoice Invoice { get; set; } = null!;

        /// <summary>
        /// Producto vendido (opcional, puede ser concepto genérico)
        /// </summary>
        public int? ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        // ========================================
        // INFORMACIÓN DEL CONCEPTO CFDI
        // ========================================

        /// <summary>
        /// Clave de producto o servicio del SAT
        /// </summary>
        [Required]
        [MaxLength(8)]
        public string ClaveProdServ { get; set; } = "01010101";

        /// <summary>
        /// Número de identificación (código del producto)
        /// </summary>
        [MaxLength(100)]
        public string? NoIdentificacion { get; set; }

        /// <summary>
        /// Cantidad
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        [Required]
        public decimal Cantidad { get; set; }

        /// <summary>
        /// Clave de unidad del SAT
        /// </summary>
        [Required]
        [MaxLength(3)]
        public string ClaveUnidad { get; set; } = "H87";

        /// <summary>
        /// Unidad de medida
        /// </summary>
        [MaxLength(100)]
        public string? Unidad { get; set; }

        /// <summary>
        /// Descripción del concepto
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Valor unitario
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        [Required]
        public decimal ValorUnitario { get; set; }

        /// <summary>
        /// Importe (cantidad * valor unitario)
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        [Required]
        public decimal Importe { get; set; }

        /// <summary>
        /// Descuento aplicado
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal Descuento { get; set; }

        /// <summary>
        /// Objeto de impuesto (01=No objeto, 02=Sí objeto)
        /// </summary>
        [Required]
        [MaxLength(2)]
        public string ObjetoImp { get; set; } = "02";

        // ========================================
        // IMPUESTOS
        // ========================================

        /// <summary>
        /// Tiene traslados (IVA, IEPS)
        /// </summary>
        public bool TieneTraslados { get; set; }

        /// <summary>
        /// Base para traslados
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal? TrasladoBase { get; set; }

        /// <summary>
        /// Impuesto trasladado (002=IVA)
        /// </summary>
        [MaxLength(3)]
        public string? TrasladoImpuesto { get; set; }

        /// <summary>
        /// Tipo de factor (Tasa, Cuota, Exento)
        /// </summary>
        [MaxLength(10)]
        public string? TrasladoTipoFactor { get; set; }

        /// <summary>
        /// Tasa o cuota (0.160000 para IVA 16%)
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal? TrasladoTasaOCuota { get; set; }

        /// <summary>
        /// Importe del traslado
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal? TrasladoImporte { get; set; }

        /// <summary>
        /// Tiene retenciones
        /// </summary>
        public bool TieneRetenciones { get; set; }

        /// <summary>
        /// Base para retenciones
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal? RetencionBase { get; set; }

        /// <summary>
        /// Impuesto retenido (001=ISR, 002=IVA)
        /// </summary>
        [MaxLength(3)]
        public string? RetencionImpuesto { get; set; }

        /// <summary>
        /// Tipo de factor
        /// </summary>
        [MaxLength(10)]
        public string? RetencionTipoFactor { get; set; }

        /// <summary>
        /// Tasa o cuota
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal? RetencionTasaOCuota { get; set; }

        /// <summary>
        /// Importe de la retención
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal? RetencionImporte { get; set; }

        // ========================================
        // INFORMACIÓN ADICIONAL
        // ========================================

        /// <summary>
        /// Referencia al detalle de venta original (si aplica)
        /// </summary>
        public int? SaleDetailId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
