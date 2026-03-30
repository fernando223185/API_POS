using Domain.Entities;

namespace Application.Abstractions.Alerts
{
    public interface IAlertRuleConfigRepository
    {
        /// <summary>
        /// Obtiene todas las reglas de una empresa para mostrar en la UI de configuración.
        /// </summary>
        Task<List<AlertRuleConfig>> GetByCompanyAsync(int companyId);

        /// <summary>
        /// Obtiene la regla específica para un tipo y empresa.
        /// </summary>
        Task<AlertRuleConfig?> GetAsync(string alertType, int companyId);

        /// <summary>
        /// Crea o actualiza una regla (upsert).
        /// Solo permite modificar TargetRoleId e IsActive — AlertType y Description son inmutables.
        /// </summary>
        Task UpsertAsync(int companyId, string alertType, int? targetRoleId, bool isActive, int updatedByUserId);

        /// <summary>
        /// Si la empresa no tiene reglas para los tipos conocidos, las crea con valores por defecto.
        /// Llamado automáticamente por los jobs en cada ciclo.
        /// Default: TargetRoleId = null (broadcast empresa), IsActive = true.
        /// </summary>
        Task EnsureDefaultsAsync(int companyId);
    }
}
