using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Detalle de la orden de compra (productos solicitados)
    /// </summary>
    [Table("PurchaseOrderDetails")]
    public class PurchaseOrderDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int PurchaseOrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        /// <summary>
        /// Cantidad solicitada
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal QuantityOrdered { get; set; }

        /// <summary>
        /// Cantidad recibida (acumulada)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal QuantityReceived { get; set; } = 0;

        /// <summary>
        /// Cantidad pendiente
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal QuantityPending { get; set; }

        /// <summary>
        /// Precio unitario
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Descuento por línea
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; } = 0;

        /// <summary>
        /// % de IVA
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal TaxPercentage { get; set; } = 16;

        /// <summary>
        /// Subtotal de la línea
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        /// <summary>
        /// Total de la línea
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        /// <summary>
        /// Notas del producto
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        // Relaciones
        [ForeignKey(nameof(PurchaseOrderId))]
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;
    }
}
