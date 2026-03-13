namespace Application.Abstractions.Documents
{
    /// <summary>
    /// Servicio para generar documentos del kardex de inventario
    /// </summary>
    public interface IKardexDocumentService
    {
        /// <summary>
        /// Genera un reporte de kardex en formato Excel
        /// </summary>
        /// <param name="productId">ID del producto (opcional)</param>
        /// <param name="warehouseId">ID del almacén (opcional)</param>
        /// <param name="movementType">Tipo de movimiento (opcional)</param>
        /// <param name="fromDate">Fecha inicial</param>
        /// <param name="toDate">Fecha final</param>
        /// <returns>Array de bytes del archivo Excel</returns>
        Task<byte[]> GenerateKardexExcelAsync(
            int? productId,
            int? warehouseId,
            string? movementType,
            DateTime? fromDate,
            DateTime? toDate);

        /// <summary>
        /// Genera un reporte de kardex en formato PDF
        /// </summary>
        Task<byte[]> GenerateKardexPdfAsync(
            int? productId,
            int? warehouseId,
            string? movementType,
            DateTime? fromDate,
            DateTime? toDate);
    }
}
