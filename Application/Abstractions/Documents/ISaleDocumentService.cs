namespace Application.Abstractions.Documents
{
    /// <summary>
    /// Servicio para generar documentos PDF de ventas
    /// Tickets, facturas, y reportes de ventas
    /// </summary>
    public interface ISaleDocumentService
    {
        /// <summary>
        /// Genera un ticket de venta en formato PDF
        /// Tamaño carta o media carta
        /// </summary>
        /// <param name="saleId">ID de la venta</param>
        /// <param name="includeCompanyLogo">Incluir logo de la empresa</param>
        /// <returns>Array de bytes del PDF generado</returns>
        Task<byte[]> GenerateSaleTicketPdfAsync(int saleId, bool includeCompanyLogo = true);

        /// <summary>
        /// Genera una factura de venta en formato PDF
        /// Formato completo con CFDI (si está timbrado)
        /// </summary>
        /// <param name="saleId">ID de la venta</param>
        /// <returns>Array de bytes del PDF generado</returns>
        Task<byte[]> GenerateSaleInvoicePdfAsync(int saleId);

        /// <summary>
        /// Genera un reporte de ventas en formato PDF
        /// </summary>
        /// <param name="fromDate">Fecha inicial</param>
        /// <param name="toDate">Fecha final</param>
        /// <param name="warehouseId">Filtro por almacén (opcional)</param>
        /// <returns>Array de bytes del PDF generado</returns>
        Task<byte[]> GenerateSalesReportPdfAsync(DateTime fromDate, DateTime toDate, int? warehouseId = null);
    }
}
