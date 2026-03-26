using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

/// <summary>
/// Historial de eventos de crédito del cliente
/// Registra todos los cambios en el crédito: otorgamiento, aumentos, pagos, etc.
/// </summary>
[Table("CustomerCreditHistory")]
public class CustomerCreditHistory
{
    [Key]
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public int CompanyId { get; set; }

    // Evento
    [MaxLength(50)]
    public string EventType { get; set; } = string.Empty;
    // CreditGranted, CreditIncreased, CreditDecreased, CreditReserved, CreditReleased,
    // InvoiceCreated, InvoiceGenerated, PaymentReceived, InvoiceOverdue, StatusChanged,
    // CreditBlocked, CreditUnblocked

    public DateTime EventDate { get; set; } = DateTime.UtcNow;

    [Precision(18, 2)]
    public decimal? Amount { get; set; }

    // Contexto
    [MaxLength(20)]
    public string? RelatedEntity { get; set; } // Invoice, Payment, Policy

    public int? RelatedEntityId { get; set; }

    [MaxLength(100)]
    public string? PreviousValue { get; set; }

    [MaxLength(100)]
    public string? NewValue { get; set; }

    // Detalles
    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // Auditoría
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    [ForeignKey(nameof(CustomerId))]
    public virtual Customer? Customer { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public virtual Company? Company { get; set; }
}
