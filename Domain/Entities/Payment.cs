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

    // Datos SAT - Forma de Pago (c_FormaPago)
    [MaxLength(2)]
    public string PaymentFormSAT { get; set; } = string.Empty; // 01=Efectivo, 02=Cheque, 03=Transferencia, 04=Tarjeta crédito, 28=Tarjeta débito, 99=Por definir

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

    // ========================================
    // EMISOR (Snapshot al momento del timbrado)
    // ========================================

    /// <summary>
    /// RFC del emisor (snapshot de Company.RFC)
    /// </summary>
    [MaxLength(13)]
    public string? EmisorRfc { get; set; }

    /// <summary>
    /// Nombre del emisor (snapshot de Company.Name)
    /// </summary>
    [MaxLength(300)]
    public string? EmisorNombre { get; set; }

    /// <summary>
    /// Régimen fiscal del emisor (snapshot de Company.TaxRegime)
    /// </summary>
    [MaxLength(3)]
    public string? EmisorRegimenFiscal { get; set; }

    /// <summary>
    /// Código postal de expedición (snapshot de Company.ZipCode)
    /// </summary>
    [MaxLength(5)]
    public string? LugarExpedicion { get; set; }

    // ========================================
    // RECEPTOR (Snapshot al momento del timbrado)
    // ========================================

    /// <summary>
    /// RFC del receptor (snapshot de Customer.RFC)
    /// </summary>
    [MaxLength(13)]
    public string? ReceptorRfc { get; set; }

    /// <summary>
    /// Nombre del receptor (snapshot de Customer.Name)
    /// </summary>
    [MaxLength(300)]
    public string? ReceptorNombre { get; set; }

    /// <summary>
    /// Código postal del receptor (snapshot de Customer.ZipCode)
    /// </summary>
    [MaxLength(5)]
    public string? ReceptorDomicilioFiscal { get; set; }

    /// <summary>
    /// Régimen fiscal del receptor (snapshot de Customer.TaxRegime)
    /// </summary>
    [MaxLength(3)]
    public string? ReceptorRegimenFiscal { get; set; }

    /// <summary>
    /// Uso CFDI del receptor. Para complementos de pago siempre es "CP01"
    /// </summary>
    [MaxLength(4)]
    public string? ReceptorUsoCfdi { get; set; }

    // Control
    [MaxLength(20)]
    public string Status { get; set; } = "Draft"; // Draft, Applied, Complemented, Cancelled

    public int AppliedToInvoices { get; set; } = 0;
    public int ComplementsGenerated { get; set; } = 0;
    public int ComplementsWithError { get; set; } = 0;

    // ========================================
    // DATOS DE TIMBRADO DEL COMPLEMENTO DE PAGO
    // ========================================

    /// <summary>
    /// Serie del complemento de pago (para el CFDI tipo "P")
    /// </summary>
    [MaxLength(10)]
    public string? ComplementSerie { get; set; }

    /// <summary>
    /// Folio del complemento de pago
    /// </summary>
    [MaxLength(20)]
    public string? ComplementFolio { get; set; }

    /// <summary>
    /// UUID del complemento de pago timbrado
    /// </summary>
    [MaxLength(50)]
    public string? Uuid { get; set; }

    /// <summary>
    /// Fecha y hora de timbrado
    /// </summary>
    public DateTime? TimbradoAt { get; set; }

    /// <summary>
    /// XML completo del complemento de pago timbrado
    /// </summary>
    public string? XmlCfdi { get; set; }

    /// <summary>
    /// Cadena original del SAT
    /// </summary>
    public string? CadenaOriginalSat { get; set; }

    /// <summary>
    /// Sello digital del CFDI
    /// </summary>
    public string? SelloCfdi { get; set; }

    /// <summary>
    /// Sello digital del SAT
    /// </summary>
    public string? SelloSat { get; set; }

    /// <summary>
    /// Número de certificado del CFDI
    /// </summary>
    [MaxLength(20)]
    public string? NoCertificadoCfdi { get; set; }

    /// <summary>
    /// Número de certificado del SAT
    /// </summary>
    [MaxLength(20)]
    public string? NoCertificadoSat { get; set; }

    /// <summary>
    /// Código QR en base64
    /// </summary>
    public string? QrCode { get; set; }

    // Archivos del complemento
    [MaxLength(500)]
    public string? XmlPath { get; set; }

    [MaxLength(500)]
    public string? PdfPath { get; set; }

    public bool EmailSent { get; set; } = false;
    public DateTime? EmailSentAt { get; set; }

    // Generación del complemento
    [MaxLength(1000)]
    public string? ComplementError { get; set; }

    public int RetryCount { get; set; } = 0;
    public DateTime? LastRetryAt { get; set; }

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

    /// <summary>Motivo SAT: 01=Con sustitución, 02=Sin sustitución, 03=No se realizó, 04=Global</summary>
    [MaxLength(2)]
    public string? CancellationMotivo { get; set; }

    /// <summary>UUID del CFDI sustituto. Solo cuando CancellationMotivo = "01"</summary>
    [MaxLength(36)]
    public string? CancellationFolioSustitucion { get; set; }

    /// <summary>XML de acuse devuelto por el SAT</summary>
    public string? CancellationAcuse { get; set; }

    /// <summary>Código SAT de la cancelación: 201=Cancelado, 202=En proceso, 204=Ya cancelado, etc.</summary>
    [MaxLength(10)]
    public string? CancellationSatCode { get; set; }

    // Navegación
    [ForeignKey(nameof(CustomerId))]
    public virtual Customer? Customer { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public virtual Company? Company { get; set; }

    [ForeignKey(nameof(BatchId))]
    public virtual PaymentBatch? Batch { get; set; }

    public virtual ICollection<PaymentApplication> PaymentApplications { get; set; } = new List<PaymentApplication>();
}
