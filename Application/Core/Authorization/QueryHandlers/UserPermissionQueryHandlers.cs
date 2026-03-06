using Application.Abstractions.Authorization;
using Application.Core.Authorization.Queries;
using Application.DTOs.UserPermissions;
using MediatR;

namespace Application.Core.Authorization.QueryHandlers
{
    /// <summary>
    /// Handler para obtener permisos de usuario
    /// </summary>
    public class GetUserPermissionsQueryHandler : IRequestHandler<GetUserPermissionsQuery, GetUserPermissionsResponseDto>
    {
        private readonly IUserPermissionRepository _userPermissionRepository;

        public GetUserPermissionsQueryHandler(IUserPermissionRepository userPermissionRepository)
        {
            _userPermissionRepository = userPermissionRepository;
        }

        public async Task<GetUserPermissionsResponseDto> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
        {
            // Obtener permisos del usuario usando el repositorio
            var result = await _userPermissionRepository.GetUserPermissionsAsync(request.UserId);

            if (result == null)
            {
                throw new Exception($"No se pudieron obtener los permisos del usuario {request.UserId}");
            }

            return result;
        }
    }

    /// <summary>
    /// Handler para verificar un permiso específico
    /// </summary>
    public class CheckUserPermissionQueryHandler : IRequestHandler<CheckUserPermissionQuery, CheckUserPermissionResponseDto>
    {
        private readonly IUserPermissionRepository _userPermissionRepository;

        public CheckUserPermissionQueryHandler(IUserPermissionRepository userPermissionRepository)
        {
            _userPermissionRepository = userPermissionRepository;
        }

        public async Task<CheckUserPermissionResponseDto> Handle(CheckUserPermissionQuery request, CancellationToken cancellationToken)
        {
            bool hasPermission = await _userPermissionRepository.HasPermissionAsync(
                request.UserId,
                request.ModuleId,
                request.SubmoduleId,
                request.Action
            );

            return new CheckUserPermissionResponseDto
            {
                Message = hasPermission ? "Usuario tiene el permiso" : "Usuario no tiene el permiso",
                Error = 0,
                UserId = request.UserId,
                ModuleId = request.ModuleId,
                SubmoduleId = request.SubmoduleId,
                Action = request.Action,
                HasPermission = hasPermission
            };
        }
    }
}
