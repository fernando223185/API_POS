using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

/// <summary>
/// Aplicación del pago a facturas específicas
/// Aquí se guarda el detalle de cómo se aplicó cada pago a cada factura
/// y se registra el complemento de pago generado
/// </summary>
[Table("PaymentApplications")]
public class PaymentApplication
{
    [Key]
    public int Id { get; set; }

    // Relaciones
    public int PaymentId { get; set; }
    
    /// <summary>
    /// ID de la factura a la que se aplica el pago
    /// </summary>
    public int InvoiceId { get; set; }

    [ForeignKey("InvoiceId")]
    public Invoice? Invoice { get; set; }

    // Datos de la Factura (desnormalizados para performance)
    public int CustomerId { get; set; }

    [MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(36)]
    public string FolioUUID { get; set; } = string.Empty;

    [MaxLength(30)]
    public string SerieAndFolio { get; set; } = string.Empty;

    [Precision(18, 2)]
    public decimal OriginalInvoiceAmount { get; set; }

    // Aplicación del Pago
    [MaxLength(20)]
    public string PaymentType { get; set; } = "FullPayment"; // FullPayment, PartialPayment

    public int PartialityNumber { get; set; }

    [Precision(18, 2)]
    public decimal PreviousBalance { get; set; }

    [Precision(18, 2)]
    public decimal AmountApplied { get; set; }

    [Precision(18, 2)]
    public decimal NewBalance { get; set; }

    // Datos de Moneda del Documento Relacionado
    [MaxLength(3)]
    public string DocumentCurrency { get; set; } = "MXN"; // MonedaDR

    [Precision(18, 6)]
    public decimal DocumentExchangeRate { get; set; } = 1.0M; // EquivalenciaDR

    [MaxLength(2)]
    public string TaxObject { get; set; } = "02"; // ObjetoImpDR (01=No objeto, 02=Sí objeto de impuestos)

    // Impuestos del Documento Relacionado (ImpuestosDR > TrasladosDR)
    [Precision(18, 6)]
    public decimal TaxBase { get; set; } = 0; // BaseDR - Base del impuesto

    [MaxLength(3)]
    public string TaxCode { get; set; } = "002"; // ImpuestoDR (002=IVA, 001=ISR)

    [MaxLength(20)]
    public string TaxFactorType { get; set; } = "Tasa"; // TipoFactorDR

    [Precision(8, 6)]
    public decimal TaxRate { get; set; } = 0.160000M; // TasaOCuotaDR (0.160000 para IVA 16%)

    [Precision(18, 6)]
    public decimal TaxAmount { get; set; } = 0; // ImporteDR - Importe del impuesto

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AppliedAt { get; set; }

    // Navegación
    [ForeignKey(nameof(PaymentId))]
    public virtual Payment? Payment { get; set; }

    public virtual ICollection<PaymentComplementLog> PaymentComplementLogs { get; set; } = new List<PaymentComplementLog>();
}
