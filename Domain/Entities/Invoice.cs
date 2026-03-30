using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Factura CFDI generada desde ventas con control de timbrado
    /// </summary>
    [Table("Invoices")]
    public class Invoice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ========================================
        // REFERENCIA A VENTA ORIGEN
        // ========================================

        /// <summary>
        /// ID de la venta de origen (si aplica)
        /// </summary>
        public int? SaleId { get; set; }

        [ForeignKey("SaleId")]
        public Sale? Sale { get; set; }

        // ========================================
        // INFORMACIÓN DEL COMPROBANTE
        // ========================================

        /// <summary>
        /// Serie del comprobante (A, B, F, etc.)
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string Serie { get; set; } = string.Empty;

        /// <summary>
        /// Folio del comprobante
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Folio { get; set; } = string.Empty;

        /// <summary>
        /// Fecha del comprobante
        /// </summary>
        [Required]
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Forma de pago SAT (01, 02, 03, 04, etc.)
        /// </summary>
        [Required]
        [MaxLength(2)]
        public string FormaPago { get; set; } = string.Empty;

        /// <summary>
        /// Método de pago SAT (PUE o PPD)
        /// </summary>
        [Required]
        [MaxLength(3)]
        public string MetodoPago { get; set; } = string.Empty;

        /// <summary>
        /// Condiciones de pago
        /// </summary>
        [MaxLength(500)]
        public string? CondicionesDePago { get; set; }

        /// <summary>
        /// Tipo de comprobante (I=Ingreso, E=Egreso, T=Traslado)
        /// </summary>
        [Required]
        [MaxLength(1)]
        public string TipoDeComprobante { get; set; } = "I";

        /// <summary>
        /// Lugar de expedición (código postal)
        /// </summary>
        [Required]
        [MaxLength(5)]
        public string LugarExpedicion { get; set; } = string.Empty;

        // ========================================
        // EMISOR
        // ========================================

        [Required]
        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; } = null!;

        [MaxLength(13)]
        public string EmisorRfc { get; set; } = string.Empty;

        [MaxLength(300)]
        public string EmisorNombre { get; set; } = string.Empty;

        [MaxLength(3)]
        public string EmisorRegimenFiscal { get; set; } = string.Empty;

        // ========================================
        // RECEPTOR
        // ========================================

        public int? CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        [Required]
        [MaxLength(13)]
        public string ReceptorRfc { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string ReceptorNombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(5)]
        public string ReceptorDomicilioFiscal { get; set; } = string.Empty;

        [MaxLength(3)]
        public string? ReceptorRegimenFiscal { get; set; }

        [Required]
        [MaxLength(4)]
        public string ReceptorUsoCfdi { get; set; } = string.Empty;

        // ========================================
        // MONTOS
        // ========================================

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Required]
        [MaxLength(3)]
        public string Moneda { get; set; } = "MXN";

        [Column(TypeName = "decimal(18,6)")]
        public decimal TipoCambio { get; set; } = 1;

        // ========================================
        // TIMBRADO
        // ========================================

        /// <summary>
        /// Estado: Borrador, Timbrada, Cancelada
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Borrador";

        /// <summary>
        /// UUID del CFDI timbrado
        /// </summary>
        [MaxLength(50)]
        public string? Uuid { get; set; }

        /// <summary>
        /// Fecha y hora de timbrado
        /// </summary>
        public DateTime? TimbradoAt { get; set; }

        /// <summary>
        /// XML completo del CFDI timbrado
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

        // ========================================
        // CANCELACIÓN
        // ========================================

        /// <summary>
        /// Fecha y hora de cancelación
        /// </summary>
        public DateTime? CancelledAt { get; set; }

        /// <summary>
        /// Motivo de cancelación (texto libre interno)
        /// </summary>
        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        public int? CancelledByUserId { get; set; }

        [ForeignKey("CancelledByUserId")]
        public User? CancelledBy { get; set; }

        /// <summary>
        /// Motivo SAT: 01=Con sustitución, 02=Sin sustitución, 03=No se realizó, 04=Global
        /// </summary>
        [MaxLength(2)]
        public string? CancellationMotivo { get; set; }

        /// <summary>
        /// UUID del CFDI que sustituye a este (solo cuando Motivo = "01")
        /// </summary>
        [MaxLength(36)]
        public string? CancellationFolioSustitucion { get; set; }

        /// <summary>
        /// XML del acuse de cancelación devuelto por el SAT/PAC
        /// </summary>
        public string? CancellationAcuse { get; set; }

        /// <summary>
        /// Código SAT de la cancelación: 201=Cancelado, 202=En proceso, 204=Ya cancelado, 205=No cancelable, etc.
        /// </summary>
        [MaxLength(10)]
        public string? CancellationSatCode { get; set; }

        // ========================================
        // DETALLES DE CONCEPTOS
        // ========================================

        public List<InvoiceDetail> Details { get; set; } = new();

        // ========================================
        // METADATOS
        // ========================================

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public User? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // ========================================
        // CAMPOS PARA FACTURAS PPD (Pago en Parcialidades o Diferido)
        // Solo aplican cuando MetodoPago = "PPD"
        // ========================================

        /// <summary>
        /// Fecha de vencimiento de la factura (solo para PPD)
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Monto pagado acumulado (solo para PPD)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; } = 0;

        /// <summary>
        /// Saldo pendiente (solo para PPD). Se calcula como Total - PaidAmount
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? BalanceAmount { get; set; }

        /// <summary>
        /// Número de la siguiente parcialidad a generar (solo para PPD)
        /// </summary>
        public int? NextPartialityNumber { get; set; }

        /// <summary>
        /// Total de parcialidades generadas (complementos de pago emitidos)
        /// </summary>
        public int TotalPartialities { get; set; } = 0;

        /// <summary>
        /// Días de vencimiento (calculado, solo para PPD)
        /// </summary>
        public int? DaysOverdue { get; set; }

        /// <summary>
        /// Fecha del último pago aplicado (solo para PPD)
        /// </summary>
        public DateTime? LastPaymentDate { get; set; }

        /// <summary>
        /// Estado de cobranza para facturas PPD: Pending, PartiallyPaid, Paid, Overdue, Cancelled
        /// NULL para facturas PUE
        /// </summary>
        [MaxLength(20)]
        public string? PaymentStatus { get; set; }

        /// <summary>
        /// Relación con aplicaciones de pago (complementos de pago asociados)
        /// </summary>
        public List<PaymentApplication> PaymentApplications { get; set; } = new();
    }
}
