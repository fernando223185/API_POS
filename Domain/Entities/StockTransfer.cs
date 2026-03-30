using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Traspaso directo de mercancía entre almacenes.
    /// Afecta inventario y kardex en el momento de aplicarse.
    /// </summary>
    [Table("StockTransfers")]
    public class StockTransfer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>Código único — formato TRF-001</summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>Almacén de origen (sale mercancía)</summary>
        [Required]
        public int SourceWarehouseId { get; set; }

        /// <summary>Almacén de destino (entra mercancía)</summary>
        [Required]
        public int DestinationWarehouseId { get; set; }

        public int? CompanyId { get; set; }

        /// <summary>Fecha del traspaso</summary>
        public DateTime TransferDate { get; set; } = DateTime.UtcNow;

        /// <summary>Draft | Completed | Cancelled</summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Draft";

        [MaxLength(1000)]
        public string? Notes { get; set; }

        /// <summary>True una vez que se aplicó al inventario — inmutable después</summary>
        public bool IsApplied { get; set; } = false;

        public DateTime? AppliedAt { get; set; }
        public int? AppliedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedByUserId { get; set; }

        // Relaciones
        [ForeignKey(nameof(SourceWarehouseId))]
        public virtual Warehouse SourceWarehouse { get; set; } = null!;

        [ForeignKey(nameof(DestinationWarehouseId))]
        public virtual Warehouse DestinationWarehouse { get; set; } = null!;

        [ForeignKey(nameof(CompanyId))]
        public virtual Company? Company { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public virtual User? CreatedBy { get; set; }

        [ForeignKey(nameof(AppliedByUserId))]
        public virtual User? AppliedBy { get; set; }

        public virtual ICollection<StockTransferDetail> Details { get; set; } = new List<StockTransferDetail>();
    }
}
