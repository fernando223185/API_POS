using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Movimientos de inventario (kardex)
    /// Registra todos los movimientos de entrada, salida y ajustes
    /// </summary>
    [Table("InventoryMovements")]
    public class InventoryMovement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// C�digo �nico del movimiento (generado autom�ticamente)
        /// Formato: MOV-001, MOV-002, etc.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de movimiento: IN (Entrada), OUT (Salida), TRANSFER (Traspaso), ADJUSTMENT (Ajuste)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string MovementType { get; set; } = string.Empty;

        /// <summary>
        /// Subtipo: PURCHASE (Compra), SALE (Venta), RETURN (Devoluci�n), etc.
        /// </summary>
        [MaxLength(50)]
        public string? MovementSubType { get; set; }

        /// <summary>
        /// Producto
        /// </summary>
        [Required]
        public int ProductId { get; set; }

        /// <summary>
        /// Almac�n
        /// </summary>
        [Required]
        public int WarehouseId { get; set; }

        /// <summary>
        /// Cantidad (positiva para IN, negativa para OUT)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Stock antes del movimiento
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal StockBefore { get; set; }

        /// <summary>
        /// Stock despu�s del movimiento
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal StockAfter { get; set; }

        /// <summary>
        /// Costo unitario
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? UnitCost { get; set; }

        /// <summary>
        /// Costo total
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalCost { get; set; }

        /// <summary>
        /// Fecha del movimiento
        /// </summary>
        public DateTime MovementDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Documento de referencia (ID de la orden de compra, venta, etc.)
        /// </summary>
        public int? ReferenceDocumentId { get; set; }

        /// <summary>
        /// Tipo de documento de referencia (PurchaseOrder, Sale, etc.)
        /// </summary>
        [MaxLength(50)]
        public string? ReferenceDocumentType { get; set; }

        /// <summary>
        /// C�digo del documento de referencia
        /// </summary>
        [MaxLength(50)]
        public string? ReferenceDocumentCode { get; set; }

        /// <summary>
        /// ID de recepci�n (si aplica)
        /// </summary>
        public int? PurchaseOrderReceivingId { get; set; }

        /// <summary>
        /// ID de venta (si aplica)
        /// </summary>
        public int? SaleId { get; set; }

        /// <summary>
        /// ID de traspaso directo (si aplica)
        /// </summary>
        public int? StockTransferId { get; set; }

        /// <summary>
        /// Lote o serie
        /// </summary>
        [MaxLength(100)]
        public string? LotNumber { get; set; }

        /// <summary>
        /// Ubicaci�n f�sica
        /// </summary>
        [MaxLength(100)]
        public string? StorageLocation { get; set; }

        /// <summary>
        /// Notas
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedByUserId { get; set; }

        // Relaciones
        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey(nameof(WarehouseId))]
        public virtual Warehouse Warehouse { get; set; } = null!;

        [ForeignKey(nameof(PurchaseOrderReceivingId))]
        public virtual PurchaseOrderReceiving? PurchaseOrderReceiving { get; set; }

        /// <summary>
        /// ? NUEVO: Relaci�n con venta
        /// </summary>
        [ForeignKey(nameof(SaleId))]
        public virtual Sale? Sale { get; set; }

        [ForeignKey(nameof(StockTransferId))]
        public virtual StockTransfer? StockTransfer { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public virtual User? CreatedBy { get; set; }
    }
}
