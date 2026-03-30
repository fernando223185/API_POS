using Domain.Entities;

namespace Application.Abstractions.Alerts
{
    public interface IAlertRepository
    {
        /// <summary>
        /// Busca una alerta abierta (Pending) por su UniqueKey.
        /// Usado por los jobs para evitar duplicados.
        /// </summary>
        Task<Alert?> GetOpenByUniqueKeyAsync(string uniqueKey);

        /// <summary>
        /// Crea una nueva alerta.
        /// </summary>
        Task<Alert> CreateAsync(Alert alert);

        /// <summary>
        /// Actualiza el campo LastDetectedAt de una alerta existente.
        /// </summary>
        Task TouchAsync(int id);

        /// <summary>
        /// Resuelve automáticamente alertas cuya condición ya no aplica.
        /// Por ejemplo, factura pagada o stock repuesto.
        /// </summary>
        Task ResolveByUniqueKeyAsync(string uniqueKey);

        /// <summary>
        /// Lista de alertas paginadas para un usuario/empresa.
        /// Si isAdmin=true devuelve todas las alertas de la empresa sin filtrar por rol.
        /// </summary>
        Task<List<Alert>> GetByUserAsync(int userId, int userRoleId, int? companyId, int page, int pageSize, bool isAdmin = false);

        /// <summary>
        /// Contador de alertas sin leer para la campanita del frontend.
        /// Si isAdmin=true cuenta todas las alertas Pending de la empresa.
        /// </summary>
        Task<int> GetUnreadCountAsync(int userId, int userRoleId, int? companyId, bool isAdmin = false);

        /// <summary>
        /// Marca una alerta como leída.
        /// </summary>
        Task<bool> MarkAsReadAsync(int id, int userId);

        /// <summary>
        /// Marca una alerta como resuelta.
        /// </summary>
        Task<bool> MarkAsResolvedAsync(int id, int userId);

        /// <summary>
        /// Resuelve masivamente todas las alertas abiertas para una referencia.
        /// Se llama, por ejemplo, cuando se paga una factura.
        /// </summary>
        Task ResolveManyAsync(string referenceType, int referenceId);
    }
}
