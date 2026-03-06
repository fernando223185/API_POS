using Domain.Entities;

namespace Application.Abstractions.Authorization
{
    /// <summary>
    /// Servicio de permisos - SISTEMA UNIFICADO
    /// Usa RoleModulePermissions y UserModulePermissions
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// Verifica si un usuario tiene permiso para un recurso y acción
        /// </summary>
        Task<bool> HasPermissionAsync(int userId, string resource, string action);

        /// <summary>
        /// Obtiene todas las claims de permisos de un usuario
        /// Retorna lista en formato "Resource:Action"
        /// </summary>
        Task<List<string>> GetUserPermissionClaimsAsync(int userId);
    }
}