namespace Application.DTOs.Billing
{
    /// <summary>
    /// Respuesta paginada de ventas pendientes de timbrar
    /// </summary>
    public class PendingInvoiceSalesResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public List<PendingInvoiceSaleDto> Data { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
        public PendingInvoiceSummaryDto? Summary { get; set; }
    }

    /// <summary>
    /// DTO de venta pendiente de timbrar
    /// </summary>
    public class PendingInvoiceSaleDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        
        // Cliente
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerRfc { get; set; }
        public string? CustomerEmail { get; set; }
        
        // Ubicaci�n
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        
        // Montos
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        
        // Facturaci�n
        public bool RequiresInvoice { get; set; }
        public bool IsPaid { get; set; }
        public string Status { get; set; } = string.Empty;
        
        // Metadatos
        public DateTime CreatedAt { get; set; }
        public int DaysPending { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Resumen de ventas pendientes de timbrar
    /// </summary>
    public class PendingInvoiceSummaryDto
    {
        public int TotalSales { get; set; }
        public int SalesRequiresInvoice { get; set; }
        public int SalesNotRequiresInvoice { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public int AverageDaysPending { get; set; }
    }

    /// <summary>
    /// DTO completo de venta para facturaci�n
    /// Incluye toda la informaci�n necesaria para generar CFDI
    /// </summary>
    public class SaleForInvoicingDto
    {
        // Informaci�n de la venta
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        
        // Empresa emisora
        public CompanyForInvoicingDto Company { get; set; } = new();
        
        // Sucursal
        public BranchForInvoicingDto? Branch { get; set; }
        
        // Cliente receptor
        public CustomerForInvoicingDto Customer { get; set; } = new();
        
        // Montos
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        
        // Estado
        public bool IsPaid { get; set; }
        public bool RequiresInvoice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? InvoiceUuid { get; set; }
        
        // Detalles de productos
        public List<SaleDetailForInvoicingDto> Details { get; set; } = new();
        
        // Formas de pago
        public List<PaymentMethodForInvoicingDto> Payments { get; set; } = new();
        
        // Metadatos
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Informaci�n de la empresa emisora
    /// </summary>
    public class CompanyForInvoicingDto
    {
        public int Id { get; set; }
        public string LegalName { get; set; } = string.Empty;
        public string Rfc { get; set; } = string.Empty;
        public string? FiscalRegime { get; set; }
        public string? PostalCode { get; set; }
        public string? Email { get; set; }
        
        // Certificados SAT
        public string? Serie { get; set; }
        public int? NextFolio { get; set; }
    }

    /// <summary>
    /// Informaci�n de la sucursal
    /// </summary>
    public class BranchForInvoicingDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
    }

    /// <summary>
    /// Informaci�n del cliente receptor
    /// </summary>
    public class CustomerForInvoicingDto
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Rfc { get; set; }
        public string? FiscalRegime { get; set; }
        public string? PostalCode { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? CfdiUse { get; set; }
    }

    /// <summary>
    /// Detalle de producto para facturaci�n
    /// </summary>
    public class SaleDetailForInvoicingDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        
        // Cantidad y precios
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        
        // Claves SAT
        public string? SatProductKey { get; set; }
        public string? SatUnitKey { get; set; }
        
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Forma de pago para facturaci�n
    /// </summary>
    public class PaymentMethodForInvoicingDto
    {
        public int Id { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        
        // Informaci�n adicional seg�n m�todo de pago
        public string? CardNumber { get; set; }
        public string? TransactionReference { get; set; }
        public string? BankName { get; set; }
    }

    // ========================================
    // DTOs para Timbrado CFDI
    // ========================================

    /// <summary>
    /// Request para timbrar un CFDI a partir de una venta
    /// </summary>
    public class TimbrarCfdiRequestDto
    {
        /// <summary>
        /// ID de la venta a timbrar
        /// </summary>
        public int SaleId { get; set; }

        /// <summary>
        /// Serie del comprobante (ej: "A", "F", "FCO")
        /// Si no se especifica, se usa la serie por defecto de la empresa
        /// </summary>
        public string? Serie { get; set; }

        /// <summary>
        /// Folio del comprobante
        /// Si no se especifica, se genera autom�ticamente
        /// </summary>
        public string? Folio { get; set; }

        /// <summary>
        /// Forma de pago SAT (01, 02, 03, etc.)
        /// 01 = Efectivo, 02 = Cheque, 03 = Transferencia, 04 = Tarjeta de cr�dito
        /// </summary>
        public string FormaPago { get; set; } = "01";

        /// <summary>
        /// M�todo de pago SAT (PUE o PPD)
        /// PUE = Pago en una sola exhibici�n
        /// PPD = Pago en parcialidades o diferido
        /// </summary>
        public string MetodoPago { get; set; } = "PUE";

        /// <summary>
        /// Uso del CFDI (P01, G03, etc.)
        /// </summary>
        public string? UsoCfdi { get; set; }

        /// <summary>
        /// Condiciones de pago (texto libre)
        /// </summary>
        public string? CondicionesDePago { get; set; }

        /// <summary>
        /// Versi�n de respuesta de Sapiens (v1, v2, v3, v4)
        /// </summary>
        public string Version { get; set; } = "v4";

        /// <summary>
        /// Notas adicionales
        /// </summary>
        public string? Notas { get; set; }
    }

    /// <summary>
    /// Response del timbrado de CFDI
    /// </summary>
    public class TimbrarCfdiResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public TimbradoDataDto? Data { get; set; }
    }

    /// <summary>
    /// Datos del CFDI timbrado
    /// </summary>
    public class TimbradoDataDto
    {
        /// <summary>
        /// ID de la venta
        /// </summary>
        public int SaleId { get; set; }

        /// <summary>
        /// C�digo de la venta
        /// </summary>
        public string SaleCode { get; set; } = string.Empty;

        /// <summary>
        /// UUID (Folio Fiscal) del CFDI timbrado
        /// </summary>
        public string Uuid { get; set; } = string.Empty;

        /// <summary>
        /// Serie del comprobante
        /// </summary>
        public string Serie { get; set; } = string.Empty;

        /// <summary>
        /// Folio del comprobante
        /// </summary>
        public string Folio { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora de timbrado
        /// </summary>
        public string FechaTimbrado { get; set; } = string.Empty;

        /// <summary>
        /// N�mero de certificado del SAT
        /// </summary>
        public string NoCertificadoSat { get; set; } = string.Empty;

        /// <summary>
        /// N�mero de certificado del CFDI
        /// </summary>
        public string NoCertificadoCfdi { get; set; } = string.Empty;

        /// <summary>
        /// Sello digital del SAT
        /// </summary>
        public string SelloSat { get; set; } = string.Empty;

        /// <summary>
        /// Sello digital del CFDI
        /// </summary>
        public string SelloCfdi { get; set; } = string.Empty;

        /// <summary>
        /// Cadena original del SAT
        /// </summary>
        public string CadenaOriginalSat { get; set; } = string.Empty;

        /// <summary>
        /// C�digo QR en formato base64
        /// </summary>
        public string QrCode { get; set; } = string.Empty;

        /// <summary>
        /// XML completo del CFDI timbrado
        /// </summary>
        public string XmlCfdi { get; set; } = string.Empty;

        /// <summary>
        /// Total del comprobante
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// RFC del emisor
        /// </summary>
        public string RfcEmisor { get; set; } = string.Empty;

        /// <summary>
        /// RFC del receptor
        /// </summary>
        public string RfcReceptor { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de timbrado (DateTime)
        /// </summary>
        public DateTime TimbradoAt { get; set; }
    }

    // ========================================
    // DTOs para Facturas (Invoice Entity)
    // ========================================

    /// <summary>
    /// Request para crear una factura (borrador o timbrada).
    /// El frontend envía toda la información ya calculada. El backend solo persiste y vincula.
    /// </summary>
    public class CreateInvoiceRequestDto
    {
        // ── Empresa emisora ──────────────────────────────────────────────────────
        /// <summary>ID de la empresa emisora (requerido)</summary>
        public int CompanyId { get; set; }

        // ── Ventas origen (opcional) ─────────────────────────────────────────────
        /// <summary>
        /// IDs de ventas a vincular al invoice creado.
        /// El frontend decide si agrupa varias ventas en un solo CFDI o genera uno por venta.
        /// Puede ser null/vacío para facturas independientes.
        /// </summary>
        public List<int>? SaleIds { get; set; }

        // ── Datos del comprobante ────────────────────────────────────────────────
        public string? Serie { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string FormaPago { get; set; } = "01";
        public string MetodoPago { get; set; } = "PUE";
        public string? CondicionesDePago { get; set; }
        public string Currency { get; set; } = "MXN";
        public decimal ExchangeRate { get; set; } = 1m;
        public string? Notes { get; set; }

        // ── Timbrado ─────────────────────────────────────────────────────────────
        public bool TimbrarInmediatamente { get; set; } = false;
        public string Version { get; set; } = "v4";

        // ── Emisor (opcional, sobreescribe datos de la empresa) ──────────────────
        /// <summary>
        /// Si se envía, estos datos reemplazan RFC/Nombre/RegimenFiscal/LugarExpedicion
        /// de la empresa. Útil cuando el usuario quiere facturar con datos distintos.
        /// </summary>
        public EmisorInputDto? Emisor { get; set; }

        // ── Receptor (requerido) ─────────────────────────────────────────────────
        public ReceptorInputDto Receptor { get; set; } = null!;

        // ── Montos (calculados por el frontend) ──────────────────────────────────
        public decimal Subtotal { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal Total { get; set; }

        // ── Conceptos (requerido) ────────────────────────────────────────────────
        public List<UpdateInvoiceItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// Input de concepto para factura manual
    /// </summary>
    public class InvoiceDetailInputDto
    {
        public int? ProductId { get; set; }
        public string ClaveProdServ { get; set; } = "01010101";
        public string? NoIdentificacion { get; set; }
        public decimal Cantidad { get; set; }
        public string ClaveUnidad { get; set; } = "H87";
        public string? Unidad { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal ValorUnitario { get; set; }
        public decimal Descuento { get; set; }
        public string ObjetoImp { get; set; } = "02";

        // Impuestos del concepto
        public ImpuestosConceptoDto? Impuestos { get; set; }
    }

    /// <summary>
    /// Impuestos de un concepto
    /// </summary>
    public class ImpuestosConceptoDto
    {
        public List<TrasladoDto>? Traslados { get; set; }
        public List<RetencionDto>? Retenciones { get; set; }
    }

    /// <summary>
    /// Traslado de impuesto (IVA, IEPS)
    /// </summary>
    public class TrasladoDto
    {
        public decimal Base { get; set; }
        public string Impuesto { get; set; } = "002"; // 002=IVA
        public string TipoFactor { get; set; } = "Tasa";
        public decimal TasaOCuota { get; set; } = 0.160000m;
    }

    /// <summary>
    /// Retención de impuesto (ISR, IVA)
    /// </summary>
    public class RetencionDto
    {
        public decimal Base { get; set; }
        public string Impuesto { get; set; } = "001"; // 001=ISR, 002=IVA
        public string TipoFactor { get; set; } = "Tasa";
        public decimal TasaOCuota { get; set; }
    }

    /// <summary>
    /// Datos del emisor para factura (override opcional).
    /// Si no se envía, se usan los datos fiscales de la empresa en BD.
    /// </summary>
    public class EmisorInputDto
    {
        public string Rfc { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? RegimenFiscal { get; set; }
        /// <summary>Código postal del lugar de expedición</summary>
        public string? LugarExpedicion { get; set; }
    }

    /// <summary>
    /// Datos del receptor para factura manual
    /// </summary>
    public class ReceptorInputDto
    {
        public int? CustomerId { get; set; }
        public string Rfc { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? DomicilioFiscal { get; set; }
        public string? RegimenFiscal { get; set; }
        public string UsoCfdi { get; set; } = "G03";
    }

    /// <summary>
    /// Request para timbrar una factura existente (borrador)
    /// </summary>
    public class TimbrarInvoiceRequestDto
    {
        /// <summary>
        /// ID de la factura a timbrar
        /// </summary>
        public int InvoiceId { get; set; }

        /// <summary>
        /// Versión de respuesta de Sapiens (v1, v2, v3, v4)
        /// </summary>
        public string Version { get; set; } = "v4";
    }

    /// <summary>
    /// Request para actualizar una factura borrador
    /// Solo se pueden actualizar facturas en estado Borrador
    /// </summary>
    public class UpdateInvoiceRequestDto
    {
        // Datos del receptor / cliente
        public string? ClientName { get; set; }
        public string? ClientRFC { get; set; }
        public string? ClientUsoCFDI { get; set; }
        public string? ClientRegimenFiscal { get; set; }
        public string? ClientPostalCode { get; set; }

        // Datos del comprobante
        public DateTime? InvoiceDate { get; set; }    // Fecha de emisión (permite seleccionar fecha anterior)
        public string? PaymentForm { get; set; }      // -> FormaPago  (ej: "99", "01")
        public string? PaymentMethod { get; set; }    // -> MetodoPago (ej: "PPD", "PUE")
        public string? CondicionesDePago { get; set; }
        public string? Currency { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string? Notes { get; set; }

        // Montos recalculados
        public decimal? Subtotal { get; set; }
        public decimal? TotalDiscount { get; set; }
        public decimal? TotalTax { get; set; }
        public decimal? Total { get; set; }

        // Ítems actualizados (opcional)
        public List<UpdateInvoiceItemDto>? Items { get; set; }
    }

    public class UpdateInvoiceItemDto
    {
        public int? Id { get; set; }               // ID del detalle existente (0 o null = nuevo ítem)
        public int? ProductId { get; set; }
        public string? ProductCode { get; set; }
        public string? ClaveProdServ { get; set; } // Clave SAT del producto/servicio
        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxRate { get; set; }       // ej: 16 para IVA 16%
        public decimal Amount { get; set; }        // importe sin impuestos
        public decimal TaxAmount { get; set; }     // importe del impuesto
        public decimal Total { get; set; }
    }

    /// <summary>
    /// Response completo de una factura
    /// </summary>
    public class InvoiceResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public InvoiceDto? Data { get; set; }
    }

    /// <summary>
    /// DTO completo de factura
    /// </summary>
    public class InvoiceDto
    {
        public int Id { get; set; }
        
        // Referencia
        public int? SaleId { get; set; }
        public string? SaleCode { get; set; }
        
        // Comprobante
        public string Serie { get; set; } = string.Empty;
        public string Folio { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public string FormaPago { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = string.Empty;
        public string? CondicionesDePago { get; set; }
        public string TipoDeComprobante { get; set; } = string.Empty;
        public string? LugarExpedicion { get; set; }
        
        // Emisor
        public int CompanyId { get; set; }
        public string EmisorRfc { get; set; } = string.Empty;
        public string EmisorNombre { get; set; } = string.Empty;
        public string? EmisorRegimenFiscal { get; set; }
        
        // Receptor
        public int? CustomerId { get; set; }
        public string ReceptorRfc { get; set; } = string.Empty;
        public string ReceptorNombre { get; set; } = string.Empty;
        public string? ReceptorDomicilioFiscal { get; set; }
        public string? ReceptorRegimenFiscal { get; set; }
        public string? ReceptorUsoCfdi { get; set; }
        
        // Montos
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public string Moneda { get; set; } = "MXN";
        public decimal? TipoCambio { get; set; }
        
        // Estado
        public string Status { get; set; } = string.Empty;
        public string? Uuid { get; set; }
        public DateTime? TimbradoAt { get; set; }
        
        // Timbrado (solo si Status = "Timbrada")
        public string? XmlCfdi { get; set; }
        public string? CadenaOriginalSat { get; set; }
        public string? SelloCfdi { get; set; }
        public string? SelloSat { get; set; }
        public string? NoCertificadoCfdi { get; set; }
        public string? NoCertificadoSat { get; set; }
        public string? QrCode { get; set; }
        
        // Cancelación (solo si Status = "Cancelada")
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        public int? CancelledByUserId { get; set; }
        public string? CancelledByUserName { get; set; }
        
        // Detalles
        public List<InvoiceDetailDto> Details { get; set; } = new();
        
        // Audit
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO de detalle de factura (concepto CFDI)
    /// </summary>
    public class InvoiceDetailDto
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public string ClaveProdServ { get; set; } = string.Empty;
        public string? NoIdentificacion { get; set; }
        public decimal Cantidad { get; set; }
        public string ClaveUnidad { get; set; } = string.Empty;
        public string? Unidad { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal ValorUnitario { get; set; }
        public decimal Importe { get; set; }
        public decimal Descuento { get; set; }
        public string ObjetoImp { get; set; } = string.Empty;

        // Impuestos
        public bool TieneTraslados { get; set; }
        public decimal? TrasladoBase { get; set; }
        public string? TrasladoImpuesto { get; set; }
        public string? TrasladoTipoFactor { get; set; }
        public decimal? TrasladoTasaOCuota { get; set; }
        public decimal? TrasladoImporte { get; set; }

        public bool TieneRetenciones { get; set; }
        public decimal? RetencionBase { get; set; }
        public string? RetencionImpuesto { get; set; }
        public string? RetencionTipoFactor { get; set; }
        public decimal? RetencionTasaOCuota { get; set; }
        public decimal? RetencionImporte { get; set; }

        public string? Notes { get; set; }
    }

    /// <summary>
    /// Response paginado de facturas
    /// </summary>
    public class InvoiceListResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public List<InvoiceListItemDto> Data { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    /// <summary>
    /// Item de listado de facturas
    /// </summary>
    public class InvoiceListItemDto
    {
        public int Id { get; set; }
        public string Serie { get; set; } = string.Empty;
        public string Folio { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        
        public string Status { get; set; } = string.Empty;
        public string? Uuid { get; set; }
        
        public string EmisorRfc { get; set; } = string.Empty;
        public string EmisorNombre { get; set; } = string.Empty;
        
        public string ReceptorRfc { get; set; } = string.Empty;
        public string ReceptorNombre { get; set; } = string.Empty;
        
        public decimal Total { get; set; }
        public string Moneda { get; set; } = "MXN";
        
        public DateTime? TimbradoAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        
        public int? SaleId { get; set; }
        public string? SaleCode { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Respuesta paginada de facturas
    /// </summary>
    public class InvoicesPagedResponseDto
    {
        public string Message { get; set; } = "Facturas obtenidas exitosamente";
        public int Error { get; set; }
        public List<InvoiceListItemDto> Data { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    /// <summary>
    /// Response para resumen de facturación
    /// </summary>
    public class BillingSummaryResponseDto
    {
        public string Message { get; set; } = "Billing summary retrieved successfully";
        public int Error { get; set; }
        public string Period { get; set; } = string.Empty;
        public BillingSummaryDataDto Data { get; set; } = new();
    }

    /// <summary>
    /// Datos del resumen de facturación
    /// </summary>
    public class BillingSummaryDataDto
    {
        public int TotalInvoices { get; set; }
        public decimal TotalAmount { get; set; }
        public int StampedInvoices { get; set; }
        public int PendingInvoices { get; set; }
        public int CancelledInvoices { get; set; }
        public decimal AverageInvoiceAmount { get; set; }
    }

    /// <summary>
    /// Request para cancelar una factura
    /// </summary>
    public class CancelInvoiceRequestDto
    {
        /// <summary>Motivo SAT: 01=Con sustitución, 02=Sin sustitución, 03=No se realizó, 04=Global</summary>
        public string Motivo { get; set; } = string.Empty;
        /// <summary>UUID del CFDI sustituto. Solo cuando Motivo = "01"</summary>
        public string? FolioSustitucion { get; set; }
        /// <summary>Razón interna (texto libre para el contador)</summary>
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Resultado de la cancelación de una factura
    /// </summary>
    public class CancelInvoiceResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? StatusSat { get; set; }
        public string? StatusCancelation { get; set; }
        public string? IsCancelable { get; set; }
        public string? Uuid { get; set; }
        /// <summary>El CFDI necesita aceptación del receptor (motivos 01 y 03 con monto > $1,000)</summary>
        public bool RequiresReceiverAcceptance { get; set; }
    }
}
