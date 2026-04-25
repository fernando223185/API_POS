using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Orden de traspaso de mercancía entre almacenes con soporte de entrada parcial.
    /// Flujo: Draft → Dispatched → PartiallyReceived / Received | Cancelled
    /// </summary>
    [Table("WarehouseTransfers")]
    public class WarehouseTransfer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>Código único — formato WTR-001</summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>Almacén de origen (sale mercancía)</summary>
        [Required]
        public int SourceWarehouseId { get; set; }

        /// <summary>Almacén de destino (recibe mercancía)</summary>
        [Required]
        public int DestinationWarehouseId { get; set; }

        public int? CompanyId { get; set; }

        public DateTime TransferDate { get; set; }

        /// <summary>Draft | Dispatched | PartiallyReceived | Received | Cancelled</summary>
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Draft";

        [MaxLength(1000)]
        public string? Notes { get; set; }

        // ── Despacho (salida desde origen) ────────────────────────────────────
        public DateTime? DispatchedAt { get; set; }
        public int? DispatchedByUserId { get; set; }

        // ── Auditoría ─────────────────────────────────────────────────────────
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }

        // ── Navegación ────────────────────────────────────────────────────────
        public Warehouse SourceWarehouse { get; set; } = null!;
        public Warehouse DestinationWarehouse { get; set; } = null!;
        public User? CreatedBy { get; set; }
        public User? DispatchedBy { get; set; }

        public ICollection<WarehouseTransferDetail> Details { get; set; } = new List<WarehouseTransferDetail>();
        public ICollection<WarehouseTransferReceiving> Receivings { get; set; } = new List<WarehouseTransferReceiving>();
    }

    /// <summary>
    /// Línea de detalle de la orden de traspaso.
    /// </summary>
    [Table("WarehouseTransferDetails")]
    public class WarehouseTransferDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int WarehouseTransferId { get; set; }

        [Required]
        public int ProductId { get; set; }

        /// <summary>Cantidad solicitada al crear la orden.</summary>
        [Required]
        public decimal QuantityRequested { get; set; }

        /// <summary>Cantidad despachada al confirmar la salida.</summary>
        public decimal QuantityDispatched { get; set; }

        /// <summary>Cantidad acumulada recibida en el destino.</summary>
        public decimal QuantityReceived { get; set; }

        public decimal? UnitCost { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // ── Navegación ────────────────────────────────────────────────────────
        public WarehouseTransfer WarehouseTransfer { get; set; } = null!;
        public Product Product { get; set; } = null!;

        public ICollection<WarehouseTransferReceivingDetail> ReceivingDetails { get; set; } = new List<WarehouseTransferReceivingDetail>();
    }

    /// <summary>
    /// Evento de entrada de mercancía en el almacén destino.
    /// Puede ser parcial (solo algunos productos/cantidades) o completa.
    /// </summary>
    [Table("WarehouseTransferReceivings")]
    public class WarehouseTransferReceiving
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>Código único — formato WRV-001</summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public int WarehouseTransferId { get; set; }

        [Required]
        public int DestinationWarehouseId { get; set; }

        public DateTime ReceivingDate { get; set; }

        /// <summary>Partial | Complete</summary>
        [Required]
        [MaxLength(20)]
        public string ReceivingType { get; set; } = "Partial";

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public int? CreatedByUserId { get; set; }

        // ── Navegación ────────────────────────────────────────────────────────
        public WarehouseTransfer WarehouseTransfer { get; set; } = null!;
        public Warehouse DestinationWarehouse { get; set; } = null!;
        public User? CreatedBy { get; set; }

        public ICollection<WarehouseTransferReceivingDetail> Details { get; set; } = new List<WarehouseTransferReceivingDetail>();
    }

    /// <summary>
    /// Detalle de un evento de entrada — qué cantidad de cada producto se recibió.
    /// </summary>
    [Table("WarehouseTransferReceivingDetails")]
    public class WarehouseTransferReceivingDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int WarehouseTransferReceivingId { get; set; }

        [Required]
        public int WarehouseTransferDetailId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public decimal QuantityReceived { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // ── Navegación ────────────────────────────────────────────────────────
        public WarehouseTransferReceiving WarehouseTransferReceiving { get; set; } = null!;
        public WarehouseTransferDetail WarehouseTransferDetail { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
