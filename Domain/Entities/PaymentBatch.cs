using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

/// <summary>
/// Lote de pagos para procesamiento masivo (OPCIONAL)
/// Agrupa múltiples pagos de diferentes clientes para procesar en conjunto
/// </summary>
[Table("PaymentBatches")]
public class PaymentBatch
{
    [Key]
    public int Id { get; set; }

    [MaxLength(20)]
    public string BatchNumber { get; set; } = string.Empty; // LOTE-2026-001

    // Empresa
    public int CompanyId { get; set; }
    public int BranchId { get; set; }

    // Resumen del Lote
    public DateTime PaymentDate { get; set; }
    public int TotalPayments { get; set; } = 0;
    public int TotalInvoices { get; set; } = 0;

    [Precision(18, 2)]
    public decimal TotalAmount { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "MXN";

    // Datos SAT Comunes (opcional, pueden variar por pago)
    [MaxLength(2)]
    public string? DefaultPaymentMethodSAT { get; set; }

    [MaxLength(100)]
    public string? DefaultBankDestination { get; set; }

    [MaxLength(20)]
    public string? DefaultAccountDestination { get; set; }

    // Control de Proceso
    [MaxLength(20)]
    public string Status { get; set; } = "Draft"; // Draft, Confirmed, Processing, Completed, PartialError, Cancelled

    public int ProcessingProgress { get; set; } = 0;
    public int ComplementsGenerated { get; set; } = 0;
    public int ComplementsWithError { get; set; } = 0;

    // Auditoría
    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int CreatedBy { get; set; }
    public int? ProcessedBy { get; set; }
    public int? CancelledBy { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Navegación
    [ForeignKey(nameof(CompanyId))]
    public virtual Company? Company { get; set; }

    [ForeignKey(nameof(BranchId))]
    public virtual Branch? Branch { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
