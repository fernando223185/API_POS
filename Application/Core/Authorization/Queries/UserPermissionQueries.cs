using Application.DTOs.UserPermissions;
using MediatR;

namespace Application.Core.Authorization.Queries
{
    /// <summary>
    /// Query para obtener los permisos de un usuario
    /// </summary>
    public class GetUserPermissionsQuery : IRequest<GetUserPermissionsResponseDto>
    {
        public int UserId { get; set; }

        public GetUserPermissionsQuery(int userId)
        {
            UserId = userId;
        }
    }

    /// <summary>
    /// Query para verificar un permiso específico
    /// </summary>
    public class CheckUserPermissionQuery : IRequest<CheckUserPermissionResponseDto>
    {
        public int UserId { get; set; }
        public int ModuleId { get; set; }
        public int? SubmoduleId { get; set; }
        public string Action { get; set; }

        public CheckUserPermissionQuery(int userId, int moduleId, int? submoduleId, string action)
        {
            UserId = userId;
            ModuleId = moduleId;
            SubmoduleId = submoduleId;
            Action = action;
        }
    }
}
