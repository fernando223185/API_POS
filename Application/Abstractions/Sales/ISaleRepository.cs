using Domain.Entities;

namespace Application.Abstractions.Sales
{
    /// <summary>
    /// Repositorio para gesti�n de ventas
    /// </summary>
    public interface ISaleRepository
    {
        /// <summary>
        /// Crear una nueva venta
        /// </summary>
        Task<Sale> CreateAsync(Sale sale);

        /// <summary>
        /// Obtener venta por ID con todas las relaciones
        /// </summary>
        Task<Sale?> GetByIdAsync(int id);

        /// <summary>
        /// Obtener venta por c�digo
        /// </summary>
        Task<Sale?> GetByCodeAsync(string code);

        /// <summary>
        /// Actualizar venta
        /// </summary>
        Task<Sale> UpdateAsync(Sale sale);

        /// <summary>
        /// Obtener ventas paginadas con filtros
        /// </summary>
        Task<(IEnumerable<Sale> Sales, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            int? warehouseId = null,
            int? customerId = null,
            int? userId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? status = null,
            bool? isPaid = null,
            bool? requiresInvoice = null
        );

        /// <summary>
        /// Obtener ventas pendientes de timbrar (facturar)
        /// Ventas con Status = 'Completed', IsPaid = true, InvoiceUuid = null
        /// </summary>
        Task<(IEnumerable<Sale> Sales, int TotalCount)> GetPendingInvoiceSalesAsync(
            int page,
            int pageSize,
            bool? onlyRequiresInvoice = null,
            int? warehouseId = null,
            int? branchId = null,
            int? companyId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null
        );

        /// <summary>
        /// Obtener venta completa para facturaci�n
        /// Incluye todas las relaciones necesarias: Company, Branch, Customer, Details, Payments, Product (para claves SAT)
        /// </summary>
        Task<Sale?> GetSaleForInvoicingAsync(int saleId);

        /// <summary>
        /// Obtener estad�sticas de ventas
        /// </summary>
        Task<(int Total, int Completed, int Cancelled, int Draft, decimal TotalRevenue, decimal TotalCost)> GetStatisticsAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? warehouseId = null
        );

        /// <summary>
        /// Verificar si existe una venta con el c�digo
        /// </summary>
        Task<bool> ExistsByCodeAsync(string code);

        /// <summary>
        /// Vincular un invoice a una venta (actualiza InvoiceId en la venta)
        /// </summary>
        Task SetInvoiceIdAsync(int saleId, int invoiceId);

        /// <summary>
        /// Vincular un invoice a múltiples ventas de una sola vez
        /// </summary>
        Task SetInvoiceIdBulkAsync(IEnumerable<int> saleIds, int invoiceId);
    }
}
