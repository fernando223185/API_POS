using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Detalle del traspaso: un producto con su cantidad a mover.
    /// </summary>
    [Table("StockTransferDetails")]
    public class StockTransferDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int StockTransferId { get; set; }

        [Required]
        public int ProductId { get; set; }

        /// <summary>Cantidad a traspasar</summary>
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }

        /// <summary>Costo unitario en el momento del traspaso (tomado del AverageCost del origen)</summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? UnitCost { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Relaciones
        [ForeignKey(nameof(StockTransferId))]
        public virtual StockTransfer StockTransfer { get; set; } = null!;

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;
    }
}
