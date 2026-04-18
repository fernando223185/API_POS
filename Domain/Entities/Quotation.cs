using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Cotización. Al ser aceptada se convierte en una Sale mediante escaneo de QR.
    /// No mueve inventario, no genera pago ni CFDI.
    /// </summary>
    [Table("Quotations")]
    public class Quotation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Código único. Formato: COT-000001
        /// Este código se incluye en el QR para convertirla en venta.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de creación de la cotización
        /// </summary>
        [Required]
        public DateTime QuotationDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de vencimiento de la cotización
        /// </summary>
        public DateTime? ValidUntil { get; set; }

        // ========================================
        // CLIENTE
        // ========================================

        public int? CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        [MaxLength(200)]
        public string? CustomerName { get; set; }

        // ========================================
        // UBICACIÓN Y USUARIO
        // ========================================

        [Required]
        public int WarehouseId { get; set; }

        [ForeignKey("WarehouseId")]
        public Warehouse Warehouse { get; set; } = null!;

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public int? BranchId { get; set; }

        [ForeignKey("BranchId")]
        public Branch? Branch { get; set; }

        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }

        public int? PriceListId { get; set; }

        [ForeignKey("PriceListId")]
        public PriceList? PriceList { get; set; }

        // ========================================
        // MONTOS
        // ========================================

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(6,4)")]
        public decimal DiscountPercentage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        // ========================================
        // ESTADO
        // ========================================

        /// <summary>
        /// Estado: Draft, Sent, Accepted, Rejected, Expired, Converted
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Draft";

        public bool RequiresInvoice { get; set; }

        // ========================================
        // CONVERSIÓN A VENTA
        // ========================================

        /// <summary>
        /// Sale generada al escanear el QR y confirmar la cotización
        /// </summary>
        public int? SaleId { get; set; }

        [ForeignKey("SaleId")]
        public Sale? Sale { get; set; }

        /// <summary>
        /// Fecha en que se convirtió en venta
        /// </summary>
        public DateTime? ConvertedAt { get; set; }

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

        public ICollection<QuotationDetail> Details { get; set; } = new List<QuotationDetail>();
    }
}
