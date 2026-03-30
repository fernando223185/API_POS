using Application.Abstractions.Alerts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class AlertRepository : IAlertRepository
    {
        private readonly POSDbContext _context;

        public AlertRepository(POSDbContext context)
        {
            _context = context;
        }

        public async Task<Alert?> GetOpenByUniqueKeyAsync(string uniqueKey)
        {
            return await _context.Alerts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.UniqueKey == uniqueKey && a.Status == "Pending");
        }

        public async Task<Alert> CreateAsync(Alert alert)
        {
            _context.Alerts.Add(alert);
            await _context.SaveChangesAsync();
            return alert;
        }

        public async Task TouchAsync(int id)
        {
            var alert = await _context.Alerts.FindAsync(id);
            if (alert != null)
            {
                alert.LastDetectedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ResolveByUniqueKeyAsync(string uniqueKey)
        {
            var alert = await _context.Alerts
                .FirstOrDefaultAsync(a => a.UniqueKey == uniqueKey && a.Status == "Pending");

            if (alert != null)
            {
                alert.Status = "Resolved";
                alert.ResolvedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Alert>> GetByUserAsync(int userId, int userRoleId, int? companyId, int page, int pageSize, bool isAdmin = false)
        {
            IQueryable<Alert> query;

            if (isAdmin)
            {
                // El admin ve todas las alertas de su empresa
                query = _context.Alerts.AsNoTracking()
                    .Where(a => a.CompanyId == companyId);
            }
            else
            {
                query = _context.Alerts.AsNoTracking()
                    .Where(a =>
                        a.UserId == userId ||                                          // alerta directa al usuario
                        (a.TargetRoleId == userRoleId && a.CompanyId == companyId) ||  // alerta por rol
                        (a.UserId == null && a.TargetRoleId == null && a.CompanyId == companyId)); // broadcast empresa
            }

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId, int userRoleId, int? companyId, bool isAdmin = false)
        {
            if (isAdmin)
            {
                return await _context.Alerts
                    .AsNoTracking()
                    .CountAsync(a => a.Status == "Pending" && a.CompanyId == companyId);
            }

            return await _context.Alerts
                .AsNoTracking()
                .CountAsync(a =>
                    a.Status == "Pending" &&
                    (a.UserId == userId ||
                     (a.TargetRoleId == userRoleId && a.CompanyId == companyId) ||
                     (a.UserId == null && a.TargetRoleId == null && a.CompanyId == companyId)));
        }

        public async Task<bool> MarkAsReadAsync(int id, int userId)
        {
            var alert = await _context.Alerts.FindAsync(id);
            if (alert == null) return false;

            alert.Status = "Read";
            alert.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAsResolvedAsync(int id, int userId)
        {
            var alert = await _context.Alerts.FindAsync(id);
            if (alert == null) return false;

            alert.Status = "Resolved";
            alert.ResolvedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task ResolveManyAsync(string referenceType, int referenceId)
        {
            var alerts = await _context.Alerts
                .Where(a =>
                    a.ReferenceType == referenceType &&
                    a.ReferenceId == referenceId &&
                    (a.Status == "Pending" || a.Status == "Read"))
                .ToListAsync();

            foreach (var alert in alerts)
            {
                alert.Status = "Resolved";
                alert.ResolvedAt = DateTime.UtcNow;
            }

            if (alerts.Any())
                await _context.SaveChangesAsync();
        }
    }
}
