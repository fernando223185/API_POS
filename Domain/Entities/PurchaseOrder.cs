using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Orden de compra a proveedor
    /// </summary>
    [Table("PurchaseOrders")]
    public class PurchaseOrder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Código único de la orden (generado automáticamente)
        /// Formato: OC-001, OC-002, etc.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Proveedor
        /// </summary>
        [Required]
        public int SupplierId { get; set; }

        /// <summary>
        /// Almacén de destino
        /// </summary>
        [Required]
        public int WarehouseId { get; set; }

        /// <summary>
        /// Fecha de la orden
        /// </summary>
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha esperada de entrega
        /// </summary>
        public DateTime? ExpectedDeliveryDate { get; set; }

        /// <summary>
        /// Estado: Pending, Approved, InTransit, PartiallyReceived, Received, Cancelled
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Subtotal
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        /// <summary>
        /// IVA
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; }

        /// <summary>
        /// Total
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        /// <summary>
        /// Notas/Observaciones
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// Referencia del proveedor
        /// </summary>
        [MaxLength(100)]
        public string? SupplierReference { get; set; }

        /// <summary>
        /// Términos de pago
        /// </summary>
        [MaxLength(200)]
        public string? PaymentTerms { get; set; }

        /// <summary>
        /// Condiciones de entrega
        /// </summary>
        [MaxLength(200)]
        public string? DeliveryTerms { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }

        // Relaciones
        [ForeignKey(nameof(SupplierId))]
        public virtual Supplier Supplier { get; set; } = null!;

        [ForeignKey(nameof(WarehouseId))]
        public virtual Warehouse Warehouse { get; set; } = null!;

        [ForeignKey(nameof(CreatedByUserId))]
        public virtual User? CreatedBy { get; set; }

        [ForeignKey(nameof(UpdatedByUserId))]
        public virtual User? UpdatedBy { get; set; }

        public virtual ICollection<PurchaseOrderDetail> Details { get; set; } = new List<PurchaseOrderDetail>();
        public virtual ICollection<PurchaseOrderReceiving> Receivings { get; set; } = new List<PurchaseOrderReceiving>();
    }
}
