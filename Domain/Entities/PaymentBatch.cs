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

    [MaxLength(50)]
    public string BatchNumber { get; set; } = string.Empty; // BTCP-COMP001-270326-001

    // Empresa
    public int CompanyId { get; set; }

    // Resumen del Lote
    public DateTime PaymentDate { get; set; }
    public int TotalPayments { get; set; } = 0;
    public int TotalInvoices { get; set; } = 0;

    [Precision(18, 2)]
    public decimal TotalAmount { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "MXN";

    // Datos por defecto del lote (pueden ser sobreescritos en cada pago)
    [MaxLength(2)]
    public string? PaymentFormSAT { get; set; } // 01=Efectivo, 02=Cheque, 03=Transferencia, etc.

    [MaxLength(100)]
    public string? BankDestination { get; set; }

    [MaxLength(20)]
    public string? AccountDestination { get; set; }

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

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
