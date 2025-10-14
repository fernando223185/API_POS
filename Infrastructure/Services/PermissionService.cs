using Application.Abstractions.Authorization;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly POSDbContext _context;

        public PermissionService(POSDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasPermissionAsync(int userId, string resource, string action)
        {
            var user = await _context.User
                .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == userId && u.Active);

            if (user == null || !user.Role.IsActive)
                return false;

            return user.Role.RolePermissions
                .Any(rp => rp.Permission.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase) &&
                          rp.Permission.Name.Equals(action, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<List<Permission>> GetUserPermissionsAsync(int userId)
        {
            var user = await _context.User
                .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .ThenInclude(p => p.Module)
                .FirstOrDefaultAsync(u => u.Id == userId && u.Active);

            if (user == null || !user.Role.IsActive)
                return new List<Permission>();

            return user.Role.RolePermissions
                .Select(rp => rp.Permission)
                .ToList();
        }

        public async Task<List<string>> GetUserPermissionClaimsAsync(int userId)
        {
            var permissions = await GetUserPermissionsAsync(userId);
            
            return permissions
                .Select(p => $"{p.Resource}:{p.Name}")
                .ToList();
        }
    }
}