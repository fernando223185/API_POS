using Domain.Entities;

namespace Application.Abstractions.Billing
{
    /// <summary>
    /// Repositorio para gestión de facturas (CFDI)
    /// </summary>
    public interface IInvoiceRepository
    {
        /// <summary>
        /// Crear una nueva factura (borrador o timbrada)
        /// </summary>
        Task<Invoice> CreateAsync(Invoice invoice);

        /// <summary>
        /// Obtener factura por ID con detalles y relaciones
        /// </summary>
        Task<Invoice?> GetByIdAsync(int id);

        /// <summary>
        /// Obtener factura por UUID del SAT
        /// </summary>
        Task<Invoice?> GetByUuidAsync(string uuid);

        /// <summary>
        /// Obtener factura por Serie y Folio
        /// </summary>
        Task<Invoice?> GetBySerieAndFolioAsync(string serie, string folio);

        /// <summary>
        /// Obtener facturas asociadas a una venta
        /// </summary>
        Task<IEnumerable<Invoice>> GetBySaleIdAsync(int saleId);

        /// <summary>
        /// Actualizar factura existente
        /// </summary>
        Task<Invoice> UpdateAsync(Invoice invoice);

        /// <summary>
        /// Obtener facturas paginadas con filtros
        /// </summary>
        Task<(IEnumerable<Invoice> Invoices, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            int? companyId = null,
            int? customerId = null,
            string? status = null, // Borrador, Timbrada, Cancelada
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? serie = null,
            string? rfc = null
        );

        /// <summary>
        /// Obtener borradores de facturas (Status = 'Borrador')
        /// </summary>
        Task<IEnumerable<Invoice>> GetDraftsAsync(int? companyId = null);

        /// <summary>
        /// Verificar si existe una factura con el mismo folio
        /// </summary>
        Task<bool> ExistsBySerieAndFolioAsync(string serie, string folio, int? excludeInvoiceId = null);

        /// <summary>
        /// Obtener siguiente folio disponible para una serie
        /// </summary>
        Task<string> GetNextFolioAsync(int companyId, string serie);

        /// <summary>
        /// Cancelar una factura timbrada
        /// </summary>
        Task<Invoice> CancelAsync(int invoiceId, int userId, string cancellationReason);

        /// <summary>
        /// Obtener resumen de facturación para un mes/año específico
        /// </summary>
        Task<(int totalInvoices, decimal totalAmount, int stampedInvoices, int pendingInvoices, int cancelledInvoices)> GetSummaryAsync(int year, int month);

        /// <summary>
        /// Reemplaza todos los detalles (conceptos) de una factura Borrador
        /// </summary>
        Task ReplaceDetailsAsync(int invoiceId, List<InvoiceDetail> newDetails);
    }
}
