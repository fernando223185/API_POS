using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

/// <summary>
/// Facturas con Método de Pago PPD (Pago en Parcialidades o Diferido)
/// Registra todas las facturas a crédito pendientes de pago
/// </summary>
[Table("InvoicesPPD")]
public class InvoicePPD
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// FK a la venta/factura original del sistema (puede ser Sale o Invoice según implementación)
    /// </summary>
    public int InvoiceId { get; set; }

    // Información del Cliente
    public int CustomerId { get; set; }

    [MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(13)]
    public string CustomerRFC { get; set; } = string.Empty;

    // Información de la Empresa
    public int CompanyId { get; set; }

    // Datos del CFDI
    [MaxLength(36)]
    public string FolioUUID { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? Serie { get; set; }

    [MaxLength(20)]
    public string? Folio { get; set; }

    [MaxLength(30)]
    public string SerieAndFolio { get; set; } = string.Empty;

    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }

    // Montos
    [MaxLength(3)]
    public string Currency { get; set; } = "MXN";

    [Precision(18, 6)]
    public decimal ExchangeRate { get; set; } = 1.0M;

    [Precision(18, 2)]
    public decimal OriginalAmount { get; set; }

    [Precision(18, 2)]
    public decimal PaidAmount { get; set; } = 0;

    [Precision(18, 2)]
    public decimal BalanceAmount { get; set; }

    // Control de Parcialidades
    public int NextPartialityNumber { get; set; } = 1;
    public int TotalPartialities { get; set; } = 0;

    // Estado y Control
    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, PartiallyPaid, Paid, Overdue, Cancelled

    public int DaysOverdue { get; set; } = 0;
    public DateTime? LastPaymentDate { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedBy { get; set; }
    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navegación
    [ForeignKey(nameof(CustomerId))]
    public virtual Customer? Customer { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public virtual Company? Company { get; set; }

    public virtual ICollection<PaymentApplication> PaymentApplications { get; set; } = new List<PaymentApplication>();
}
