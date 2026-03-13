namespace Application.Abstractions.Documents
{
    /// <summary>
    /// Servicio para generar tickets de venta en formato térmico (ESC/POS)
    /// Compatible con impresoras térmicas de 58mm y 80mm
    /// </summary>
    public interface IThermalTicketService
    {
        /// <summary>
        /// Genera un ticket de venta en formato texto plano para impresoras térmicas
        /// Retorna el contenido del ticket con comandos ESC/POS básicos
        /// </summary>
        /// <param name="saleId">ID de la venta</param>
        /// <param name="width">Ancho del papel (58 o 80 caracteres)</param>
        /// <returns>Contenido del ticket en formato texto</returns>
        Task<string> GenerateSaleTicketAsync(int saleId, int width = 48);

        /// <summary>
        /// Genera un ticket de venta en formato binario (comandos ESC/POS completos)
        /// Incluye corte de papel, apertura de cajón, etc.
        /// </summary>
        /// <param name="saleId">ID de la venta</param>
        /// <param name="width">Ancho del papel (58 o 80 caracteres)</param>
        /// <returns>Array de bytes con comandos ESC/POS</returns>
        Task<byte[]> GenerateSaleTicketBinaryAsync(int saleId, int width = 48);
    }
}
