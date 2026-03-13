using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Detalle de recepción (productos recibidos físicamente)
    /// </summary>
    [Table("PurchaseOrderReceivingDetails")]
    public class PurchaseOrderReceivingDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int PurchaseOrderReceivingId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int PurchaseOrderDetailId { get; set; }

        /// <summary>
        /// Cantidad recibida en esta recepción
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal QuantityReceived { get; set; }

        /// <summary>
        /// Cantidad aprobada (después de control de calidad)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? QuantityApproved { get; set; }

        /// <summary>
        /// Cantidad rechazada
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? QuantityRejected { get; set; }

        /// <summary>
        /// Motivo de rechazo
        /// </summary>
        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        /// <summary>
        /// Ubicación física en el almacén
        /// </summary>
        [MaxLength(100)]
        public string? StorageLocation { get; set; }

        /// <summary>
        /// Lote o serie
        /// </summary>
        [MaxLength(100)]
        public string? LotNumber { get; set; }

        /// <summary>
        /// Fecha de vencimiento (para productos perecederos)
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Notas del producto
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        // Relaciones
        [ForeignKey(nameof(PurchaseOrderReceivingId))]
        public virtual PurchaseOrderReceiving PurchaseOrderReceiving { get; set; } = null!;

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey(nameof(PurchaseOrderDetailId))]
        public virtual PurchaseOrderDetail PurchaseOrderDetail { get; set; } = null!;
    }
}
