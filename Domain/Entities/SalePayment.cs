using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Pago de venta (permite múltiples formas de pago por venta)
    /// </summary>
    [Table("SalePayments")]
    public class SalePayment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Venta a la que pertenece el pago
        /// </summary>
        [Required]
        public int SaleId { get; set; }

        [ForeignKey("SaleId")]
        public Sale Sale { get; set; } = null!;

        // ========================================
        // INFORMACIÓN BÁSICA DEL PAGO
        // ========================================

        /// <summary>
        /// Forma de pago: Cash, CreditCard, DebitCard, Transfer, Check, etc.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// Monto del pago
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Required]
        public decimal Amount { get; set; }

        /// <summary>
        /// Fecha y hora del pago
        /// </summary>
        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        // ========================================
        // INFORMACIÓN DE TARJETA/TERMINAL BANCARIA
        // ========================================

        /// <summary>
        /// Últimos 4 dígitos de la tarjeta
        /// </summary>
        [MaxLength(4)]
        public string? CardNumber { get; set; }

        /// <summary>
        /// Tipo de tarjeta: Visa, Mastercard, Amex, etc.
        /// </summary>
        [MaxLength(20)]
        public string? CardType { get; set; }

        /// <summary>
        /// Código de autorización de la terminal
        /// </summary>
        [MaxLength(50)]
        public string? AuthorizationCode { get; set; }

        /// <summary>
        /// Referencia de la transacción de la terminal
        /// </summary>
        [MaxLength(100)]
        public string? TransactionReference { get; set; }

        /// <summary>
        /// ID de la terminal bancaria
        /// </summary>
        [MaxLength(50)]
        public string? TerminalId { get; set; }

        /// <summary>
        /// Nombre del banco emisor
        /// </summary>
        [MaxLength(100)]
        public string? BankName { get; set; }

        // ========================================
        // INFORMACIÓN DE TRANSFERENCIAS
        // ========================================

        /// <summary>
        /// Referencia de transferencia bancaria
        /// </summary>
        [MaxLength(100)]
        public string? TransferReference { get; set; }

        // ========================================
        // INFORMACIÓN DE CHEQUES
        // ========================================

        /// <summary>
        /// Número de cheque
        /// </summary>
        [MaxLength(50)]
        public string? CheckNumber { get; set; }

        /// <summary>
        /// Banco del cheque
        /// </summary>
        [MaxLength(100)]
        public string? CheckBank { get; set; }

        // ========================================
        // CONTROL
        // ========================================

        /// <summary>
        /// Estado del pago: Pending, Completed, Cancelled
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Completed";

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
