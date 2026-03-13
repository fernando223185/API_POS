namespace Application.Abstractions.Documents
{
    /// <summary>
    /// Servicio para generar documentos PDF de compras
    /// </summary>
    public interface IPurchaseDocumentService
    {
        /// <summary>
        /// Genera PDF de Orden de Compra
        /// </summary>
        Task<byte[]> GeneratePurchaseOrderPdfAsync(int purchaseOrderId);

        /// <summary>
        /// Genera PDF de Recibo de Mercancía
        /// </summary>
        Task<byte[]> GenerateReceivingPdfAsync(int receivingId);

        /// <summary>
        /// Guarda PDF de Orden de Compra en S3 y retorna URL
        /// </summary>
        Task<string> SavePurchaseOrderPdfToS3Async(int purchaseOrderId);

        /// <summary>
        /// Guarda PDF de Recibo en S3 y retorna URL
        /// </summary>
        Task<string> SaveReceivingPdfToS3Async(int receivingId);
    }
}
