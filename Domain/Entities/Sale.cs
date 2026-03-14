using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Venta con sistema de cobranza multi-forma de pago y control de inventario
    /// </summary>
    [Table("Sales")]
    public class Sale
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Código único de la venta (generado automáticamente)
        /// Formato: VTA-000001, VTA-000002, etc.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora de la venta
        /// </summary>
        [Required]
        public DateTime SaleDate { get; set; } = DateTime.UtcNow;

        // ========================================
        // INFORMACIÓN DEL CLIENTE
        // ========================================

        /// <summary>
        /// ID del cliente (nullable para ventas a público general)
        /// </summary>
        public int? CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        /// <summary>
        /// Nombre del cliente (denormalizado para auditoría)
        /// </summary>
        [MaxLength(200)]
        public string? CustomerName { get; set; }

        // ========================================
        // UBICACIÓN Y USUARIO
        // ========================================

        /// <summary>
        /// Almacén donde se realiza la venta
        /// </summary>
        [Required]
        public int WarehouseId { get; set; }

        [ForeignKey("WarehouseId")]
        public Warehouse Warehouse { get; set; } = null!;

        /// <summary>
        /// Usuario que realiza la venta (vendedor/cajero)
        /// </summary>
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        /// <summary>
        /// Lista de precios aplicada (opcional)
        /// </summary>
        public int? PriceListId { get; set; }

        [ForeignKey("PriceListId")]
        public PriceList? PriceList { get; set; }

        /// <summary>
        /// ?? Sucursal desde la cual se realiza la venta (desnormalizado para performance)
        /// </summary>
        public int? BranchId { get; set; }

        [ForeignKey("BranchId")]
        public Branch? Branch { get; set; }

        /// <summary>
        /// ?? Empresa a la que pertenece la venta (desnormalizado para performance)
        /// </summary>
        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }

        // ========================================
        // MONTOS
        // ========================================

        /// <summary>
        /// Subtotal (antes de descuentos e impuestos)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        /// <summary>
        /// Monto de descuento aplicado
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Porcentaje de descuento global
        /// </summary>
        [Column(TypeName = "decimal(6,4)")]
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Monto de impuestos (IVA)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Total de la venta
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        // ========================================
        // CONTROL DE PAGO
        // ========================================

        /// <summary>
        /// Total pagado (puede ser mayor al total para calcular cambio)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        /// <summary>
        /// Cambio a devolver
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal ChangeAmount { get; set; }

        /// <summary>
        /// Indica si la venta está pagada
        /// </summary>
        public bool IsPaid { get; set; }

        // ========================================
        // ESTADO
        // ========================================

        /// <summary>
        /// Estado de la venta: Draft, Completed, Cancelled, Invoiced
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Draft";

        /// <summary>
        /// Indica si se aplicaron los movimientos de inventario
        /// </summary>
        public bool IsPostedToInventory { get; set; }

        /// <summary>
        /// Fecha en que se aplicó al inventario
        /// </summary>
        public DateTime? PostedToInventoryDate { get; set; }

        // ========================================
        // FACTURACIÓN
        // ========================================

        /// <summary>
        /// Indica si el cliente requiere factura
        /// </summary>
        public bool RequiresInvoice { get; set; }

        /// <summary>
        /// ID de la factura electrónica (si existe)
        /// </summary>
        public int? InvoiceId { get; set; }

        /// <summary>
        /// UUID del CFDI (si existe)
        /// </summary>
        [MaxLength(50)]
        public string? InvoiceUuid { get; set; }

        // ========================================
        // METADATOS
        // ========================================

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public User? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        public int? CancelledByUserId { get; set; }

        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        // ========================================
        // RELACIONES
        // ========================================

        /// <summary>
        /// Detalles de la venta (productos)
        /// </summary>
        public ICollection<SaleDetail> Details { get; set; } = new List<SaleDetail>();

        /// <summary>
        /// Pagos de la venta (múltiples formas de pago)
        /// </summary>
        public ICollection<SalePayment> Payments { get; set; } = new List<SalePayment>();
    }
}
