using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Turno de cajero - Control de apertura y cierre de caja
    /// </summary>
    [Table("CashierShifts")]
    public class CashierShift
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Código único del turno: TURNO-000001
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Usuario cajero responsable del turno
        /// </summary>
        [Required]
        public int CashierId { get; set; }

        [ForeignKey("CashierId")]
        public User Cashier { get; set; } = null!;

        /// <summary>
        /// Almacén/Caja donde se realiza el turno
        /// </summary>
        [Required]
        public int WarehouseId { get; set; }

        [ForeignKey("WarehouseId")]
        public Warehouse Warehouse { get; set; } = null!;

        /// <summary>
        /// Sucursal (desnormalizado para reportes)
        /// </summary>
        public int? BranchId { get; set; }

        [ForeignKey("BranchId")]
        public Branch? Branch { get; set; }

        /// <summary>
        /// Empresa (desnormalizado para reportes)
        /// </summary>
        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }

        // ========================================
        // FECHAS Y CONTROL DE TURNO
        // ========================================

        /// <summary>
        /// Fecha y hora de apertura del turno
        /// </summary>
        [Required]
        public DateTime OpeningDate { get; set; }

        /// <summary>
        /// Fecha y hora de cierre del turno
        /// </summary>
        public DateTime? ClosingDate { get; set; }

        /// <summary>
        /// Estado: Open, Closed, Cancelled
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Open";

        // ========================================
        // MONTOS DE EFECTIVO
        // ========================================

        /// <summary>
        /// Fondo inicial en efectivo al abrir el turno
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal InitialCash { get; set; }

        /// <summary>
        /// Efectivo esperado al cierre (inicial + ventas efectivo - retiros + entradas)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal ExpectedCash { get; set; }

        /// <summary>
        /// Efectivo real contado al cierre
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? FinalCash { get; set; }

        /// <summary>
        /// Diferencia (FinalCash - ExpectedCash)
        /// Positivo = Sobrante, Negativo = Faltante
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Difference { get; set; }

        // ========================================
        // RESUMEN DE VENTAS DEL TURNO
        // ========================================

        /// <summary>
        /// Total de ventas realizadas en el turno
        /// </summary>
        public int TotalSales { get; set; }

        /// <summary>
        /// Total de ventas canceladas en el turno
        /// </summary>
        public int CancelledSales { get; set; }

        /// <summary>
        /// Monto total de ventas
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSalesAmount { get; set; }

        /// <summary>
        /// Monto total de ventas canceladas
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal CancelledSalesAmount { get; set; }

        // ========================================
        // DESGLOSE POR FORMA DE PAGO
        // ========================================

        /// <summary>
        /// Ventas en efectivo
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal CashSales { get; set; }

        /// <summary>
        /// Ventas con tarjeta
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal CardSales { get; set; }

        /// <summary>
        /// Ventas con transferencia
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TransferSales { get; set; }

        /// <summary>
        /// Otros métodos de pago
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherSales { get; set; }

        // ========================================
        // MOVIMIENTOS DE EFECTIVO
        // ========================================

        /// <summary>
        /// Retiros de efectivo durante el turno
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal CashWithdrawals { get; set; }

        /// <summary>
        /// Depósitos/entradas de efectivo durante el turno
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal CashDeposits { get; set; }

        // ========================================
        // NOTAS Y OBSERVACIONES
        // ========================================

        /// <summary>
        /// Notas de apertura
        /// </summary>
        [MaxLength(1000)]
        public string? OpeningNotes { get; set; }

        /// <summary>
        /// Notas de cierre
        /// </summary>
        [MaxLength(1000)]
        public string? ClosingNotes { get; set; }

        /// <summary>
        /// Razón de cancelación del turno
        /// </summary>
        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        // ========================================
        // AUDITORÍA
        // ========================================

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? ClosedByUserId { get; set; }

        [ForeignKey("ClosedByUserId")]
        public User? ClosedBy { get; set; }

        public int? CancelledByUserId { get; set; }

        [ForeignKey("CancelledByUserId")]
        public User? CancelledBy { get; set; }

        public DateTime? CancelledAt { get; set; }
    }
}
