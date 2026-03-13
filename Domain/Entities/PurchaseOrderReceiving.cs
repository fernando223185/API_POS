using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Recepción de mercancía (documento de entrada)
    /// </summary>
    [Table("PurchaseOrderReceivings")]
    public class PurchaseOrderReceiving
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Código único de recepción (generado automáticamente)
        /// Formato: REC-001, REC-002, etc.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Orden de compra relacionada
        /// </summary>
        [Required]
        public int PurchaseOrderId { get; set; }

        /// <summary>
        /// Fecha de recepción
        /// </summary>
        public DateTime ReceivingDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Almacén donde se recibe
        /// </summary>
        [Required]
        public int WarehouseId { get; set; }

        /// <summary>
        /// Estado: Draft, Received, QualityCheck, Approved, Rejected
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Draft";

        /// <summary>
        /// Número de remisión del proveedor
        /// </summary>
        [MaxLength(100)]
        public string? SupplierInvoiceNumber { get; set; }

        /// <summary>
        /// Nombre del transportista
        /// </summary>
        [MaxLength(200)]
        public string? CarrierName { get; set; }

        /// <summary>
        /// Número de guía de transporte
        /// </summary>
        [MaxLength(100)]
        public string? TrackingNumber { get; set; }

        /// <summary>
        /// Persona que recibió
        /// </summary>
        [MaxLength(200)]
        public string? ReceivedBy { get; set; }

        /// <summary>
        /// Observaciones de la recepción
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// Indica si se aplicó al inventario
        /// </summary>
        public bool IsPostedToInventory { get; set; } = false;

        /// <summary>
        /// Fecha en que se aplicó al inventario
        /// </summary>
        public DateTime? PostedToInventoryDate { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }

        // Relaciones
        [ForeignKey(nameof(PurchaseOrderId))]
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;

        [ForeignKey(nameof(WarehouseId))]
        public virtual Warehouse Warehouse { get; set; } = null!;

        [ForeignKey(nameof(CreatedByUserId))]
        public virtual User? CreatedBy { get; set; }

        [ForeignKey(nameof(UpdatedByUserId))]
        public virtual User? UpdatedBy { get; set; }

        public virtual ICollection<PurchaseOrderReceivingDetail> Details { get; set; } = new List<PurchaseOrderReceivingDetail>();
    }
}
