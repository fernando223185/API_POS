using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

/// <summary>
/// Política de crédito por cliente
/// Define límites de crédito, días de crédito y controla el saldo actual
/// </summary>
[Table("CustomerCreditPolicies")]
public class CustomerCreditPolicy
{
    [Key]
    public int Id { get; set; }

    // Cliente (único)
    public int CustomerId { get; set; }
    public int CompanyId { get; set; }

    // Límites de Crédito
    [Precision(18, 2)]
    public decimal CreditLimit { get; set; } = 0;

    public int CreditDays { get; set; } = 30;
    public int OverdueGraceDays { get; set; } = 0;

    // Balance Actual (actualizado en tiempo real)
    [Precision(18, 2)]
    public decimal TotalPendingAmount { get; set; } = 0;

    [Precision(18, 2)]
    public decimal TotalOverdueAmount { get; set; } = 0;

    [Precision(18, 2)]
    public decimal AvailableCredit { get; set; } = 0;

    public DateTime? OldestInvoiceDate { get; set; }
    public DateTime? OldestOverdueDate { get; set; }

    // Indicadores de Pago
    public int AveragePaymentDays { get; set; } = 0;

    [Precision(5, 2)]
    public decimal OnTimePaymentRate { get; set; } = 0;

    public DateTime? LastPaymentDate { get; set; }

    [Precision(18, 2)]
    public decimal LastPaymentAmount { get; set; } = 0;

    // Estado del Crédito
    [MaxLength(20)]
    public string Status { get; set; } = "Active"; // Active, Warning, Blocked, Suspended

    [MaxLength(500)]
    public string? BlockReason { get; set; }

    public bool AutoBlockOnOverdue { get; set; } = true;

    // Control de Aprobación
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? LastReviewDate { get; set; }
    public DateTime? NextReviewDate { get; set; }

    // Metadata
    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    // Navegación
    [ForeignKey(nameof(CustomerId))]
    public virtual Customer? Customer { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public virtual Company? Company { get; set; }
}
