using Domain.Entities;

namespace Application.Abstractions.Sales
{
    /// <summary>
    /// Repositorio para gestión de ventas
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
        /// Obtener venta por código
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
        /// Obtener estadísticas de ventas
        /// </summary>
        Task<(int Total, int Completed, int Cancelled, int Draft, decimal TotalRevenue, decimal TotalCost)> GetStatisticsAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? warehouseId = null
        );

        /// <summary>
        /// Verificar si existe una venta con el código
        /// </summary>
        Task<bool> ExistsByCodeAsync(string code);
    }
}
