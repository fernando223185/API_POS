using Application.DTOs.UserPermissions;
using Domain.Entities;

namespace Application.Abstractions.Authorization
{
    public interface IUserPermissionRepository
    {
        /// <summary>
        /// Obtener todos los permisos de un usuario con formato unificado
        /// </summary>
        Task<GetUserPermissionsResponseDto> GetUserPermissionsAsync(int userId);

        /// <summary>
        /// Obtener entidades de permisos de usuario (para uso interno)
        /// </summary>
        Task<List<UserModulePermission>> GetUserPermissionsEntitiesAsync(int userId);

        /// <summary>
        /// Guardar permisos de un usuario (reemplaza los existentes)
        /// </summary>
        Task<bool> SaveUserPermissionsAsync(int userId, List<UserModulePermission> permissions);

        /// <summary>
        /// Eliminar todos los permisos de un usuario
        /// </summary>
        Task<bool> DeleteUserPermissionsAsync(int userId);

        /// <summary>
        /// Obtener un permiso específico de usuario para un módulo/submódulo
        /// </summary>
        Task<UserModulePermission?> GetSpecificPermissionAsync(int userId, int moduleId, int? submoduleId);

        /// <summary>
        /// Verificar si un usuario tiene un permiso específico
        /// </summary>
        Task<bool> HasPermissionAsync(int userId, int moduleId, int? submoduleId, string action);

        /// <summary>
        /// Obtener permisos agrupados por módulo
        /// </summary>
        Task<Dictionary<int, List<UserModulePermission>>> GetPermissionsByModuleAsync(int userId);
    }
}
