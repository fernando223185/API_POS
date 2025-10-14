using Domain.Entities;

namespace Application.Abstractions.Authorization
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(int userId, string resource, string action);
        Task<List<Permission>> GetUserPermissionsAsync(int userId);
        Task<List<string>> GetUserPermissionClaimsAsync(int userId);
    }
}