using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

/// <summary>
/// Log de procesamiento de complementos de pago
/// Registra todos los intentos de generación, timbrado y errores
/// </summary>
[Table("PaymentComplementLogs")]
public class PaymentComplementLog
{
    [Key]
    public int Id { get; set; }

    // Relaciones
    public int? PaymentApplicationId { get; set; }
    public int PaymentId { get; set; }
    public int? BatchId { get; set; }

    // Detalle del Intento
    public int AttemptNumber { get; set; }
    public DateTime AttemptDate { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string Action { get; set; } = string.Empty; // Generate, Stamp, CreatePDF, SendEmail, Cancel

    [MaxLength(20)]
    public string Status { get; set; } = string.Empty; // Started, Success, Failed

    // Resultado
    public int ExecutionTimeMs { get; set; }

    [MaxLength(20)]
    public string? ErrorCode { get; set; }

    [MaxLength(2000)]
    public string? ErrorMessage { get; set; }

    [Column(TypeName = "ntext")]
    public string? ErrorStackTrace { get; set; }

    // Respuesta del PAC
    [Column(TypeName = "ntext")]
    public string? PACResponse { get; set; }

    [MaxLength(100)]
    public string? PACTransactionId { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedBy { get; set; }

    // Navegación
    [ForeignKey(nameof(PaymentApplicationId))]
    public virtual PaymentApplication? PaymentApplication { get; set; }

    [ForeignKey(nameof(PaymentId))]
    public virtual Payment? Payment { get; set; }

    [ForeignKey(nameof(BatchId))]
    public virtual PaymentBatch? PaymentBatch { get; set; }
}
