using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Stock de productos por almacén
    /// </summary>
    [Table("ProductStock")]
    public class ProductStock
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        /// <summary>
        /// Cantidad en stock
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; } = 0;

        /// <summary>
        /// Cantidad reservada (pedidos pendientes)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal ReservedQuantity { get; set; } = 0;

        /// <summary>
        /// Cantidad disponible (Quantity - ReservedQuantity)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal AvailableQuantity { get; set; } = 0;

        /// <summary>
        /// Costo promedio
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? AverageCost { get; set; }

        /// <summary>
        /// Stock mínimo
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? MinimumStock { get; set; }

        /// <summary>
        /// Stock máximo
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? MaximumStock { get; set; }

        /// <summary>
        /// Punto de reorden
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? ReorderPoint { get; set; }

        /// <summary>
        /// Última fecha de movimiento
        /// </summary>
        public DateTime? LastMovementDate { get; set; }

        /// <summary>
        /// Última fecha de actualización
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relaciones
        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey(nameof(WarehouseId))]
        public virtual Warehouse Warehouse { get; set; } = null!;
    }
}
