using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

/// <summary>
/// Registro de pagos realizados por clientes
/// Puede ser individual o parte de un batch
/// </summary>
[Table("Payments")]
public class Payment
{
    [Key]
    public int Id { get; set; }

    [MaxLength(20)]
    public string PaymentNumber { get; set; } = string.Empty; // PAG-2026-001

    // Cliente
    public int CustomerId { get; set; }

    [MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    // Empresa
    public int CompanyId { get; set; }
    public int BranchId { get; set; }

    // Batch (OPCIONAL)
    public int? BatchId { get; set; }
    public bool IsBatchPayment { get; set; } = false;

    // Datos del Pago
    public DateTime PaymentDate { get; set; }

    [Precision(18, 2)]
    public decimal TotalAmount { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "MXN";

    [Precision(18, 6)]
    public decimal ExchangeRate { get; set; } = 1.0M;

    // Datos SAT
    [MaxLength(2)]
    public string PaymentMethodSAT { get; set; } = string.Empty; // 03=Transferencia, 02=Cheque, etc.

    [MaxLength(2)]
    public string? PaymentFormSAT { get; set; }

    // Datos Bancarios (opcionales)
    [MaxLength(100)]
    public string? BankOrigin { get; set; }

    [MaxLength(20)]
    public string? BankAccountOrigin { get; set; }

    [MaxLength(100)]
    public string? BankDestination { get; set; }

    [MaxLength(20)]
    public string? BankAccountDestination { get; set; }

    [MaxLength(50)]
    public string? Reference { get; set; }

    // Control
    [MaxLength(20)]
    public string Status { get; set; } = "Draft"; // Draft, Applied, Complemented, Cancelled

    public int AppliedToInvoices { get; set; } = 0;
    public int ComplementsGenerated { get; set; } = 0;
    public int ComplementsWithError { get; set; } = 0;

    // Auditoría
    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AppliedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int CreatedBy { get; set; }
    public int? CancelledBy { get; set; }
    public DateTime? CancelledAt { get; set; }

    [MaxLength(500)]
    public string? CancellationReason { get; set; }

    // Navegación
    [ForeignKey(nameof(CustomerId))]
    public virtual Customer? Customer { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public virtual Company? Company { get; set; }

    [ForeignKey(nameof(BranchId))]
    public virtual Branch? Branch { get; set; }

    [ForeignKey(nameof(BatchId))]
    public virtual PaymentBatch? Batch { get; set; }

    public virtual ICollection<PaymentApplication> PaymentApplications { get; set; } = new List<PaymentApplication>();
}
