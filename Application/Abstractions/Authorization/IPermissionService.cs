using Domain.Entities;

namespace Application.Abstractions.Authorization
{
    /// <summary>
    /// Servicio de permisos - SISTEMA SIMPLIFICADO
    /// Busca directamente en la base de datos por nombre de módulo y submódulo
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// Verifica si un usuario tiene permiso para un módulo/submódulo
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="moduleName">Nombre exacto del módulo (ej: "Configuration")</param>
        /// <param name="submoduleName">Nombre exacto del submódulo (ej: "Empresas")</param>
        /// <returns>true si tiene permiso, false si no</returns>
        Task<bool> HasPermissionAsync(int userId, string moduleName, string submoduleName);

        /// <summary>
        /// Obtiene todos los permisos de un usuario
        /// Retorna lista en formato "ModuleName/SubmoduleName"
        /// </summary>
        Task<List<string>> GetUserPermissionClaimsAsync(int userId);
    }
}