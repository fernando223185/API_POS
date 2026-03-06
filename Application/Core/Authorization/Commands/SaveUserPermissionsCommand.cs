using Application.DTOs.UserPermissions;
using MediatR;

namespace Application.Core.Authorization.Commands
{
    public class SaveUserPermissionsCommand : IRequest<UserPermissionsResponseDto>
    {
        public SaveUserPermissionsRequestDto PermissionsData { get; set; }
        public int RequestingUserId { get; set; }

        public SaveUserPermissionsCommand(SaveUserPermissionsRequestDto permissionsData, int requestingUserId)
        {
            PermissionsData = permissionsData;
            RequestingUserId = requestingUserId;
        }
    }
}
