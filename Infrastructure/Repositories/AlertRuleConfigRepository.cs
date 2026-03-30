using Application.Abstractions.Alerts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class AlertRuleConfigRepository : IAlertRuleConfigRepository
    {
        // Tipos de alerta que el sistema conoce con su descripción legible.
        // Cuando el developer agrega un tipo nuevo, lo registra aquí.
        private static readonly Dictionary<string, string> KnownTypes = new()
        {
            ["InvoiceDue"] = "Facturas PPD próximas a vencer o vencidas",
            ["StockMin"] = "Productos en stock mínimo",
            ["StockCritical"] = "Productos en stock crítico (≤50% del mínimo)"
        };

        private readonly POSDbContext _context;

        public AlertRuleConfigRepository(POSDbContext context)
        {
            _context = context;
        }

        public async Task<List<AlertRuleConfig>> GetByCompanyAsync(int companyId)
        {
            return await _context.AlertRuleConfigs
                .AsNoTracking()
                .Where(r => r.CompanyId == companyId)
                .Include(r => r.TargetRole)
                .OrderBy(r => r.AlertType)
                .ToListAsync();
        }

        public async Task<AlertRuleConfig?> GetAsync(string alertType, int companyId)
        {
            return await _context.AlertRuleConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.AlertType == alertType && r.CompanyId == companyId);
        }

        public async Task UpsertAsync(int companyId, string alertType, int? targetRoleId, bool isActive, int updatedByUserId)
        {
            var existing = await _context.AlertRuleConfigs
                .FirstOrDefaultAsync(r => r.AlertType == alertType && r.CompanyId == companyId);

            if (existing == null)
            {
                var description = KnownTypes.TryGetValue(alertType, out var d) ? d : alertType;
                _context.AlertRuleConfigs.Add(new AlertRuleConfig
                {
                    AlertType = alertType,
                    Description = description,
                    CompanyId = companyId,
                    TargetRoleId = targetRoleId,
                    IsActive = isActive,
                    UpdatedByUserId = updatedByUserId
                });
            }
            else
            {
                existing.TargetRoleId = targetRoleId;
                existing.IsActive = isActive;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedByUserId = updatedByUserId;
            }

            await _context.SaveChangesAsync();
        }

        public async Task EnsureDefaultsAsync(int companyId)
        {
            var existing = await _context.AlertRuleConfigs
                .AsNoTracking()
                .Where(r => r.CompanyId == companyId)
                .Select(r => r.AlertType)
                .ToListAsync();

            var missing = KnownTypes.Where(k => !existing.Contains(k.Key)).ToList();
            if (!missing.Any()) return;

            foreach (var kv in missing)
            {
                _context.AlertRuleConfigs.Add(new AlertRuleConfig
                {
                    AlertType = kv.Key,
                    Description = kv.Value,
                    CompanyId = companyId,
                    TargetRoleId = null,   // broadcast — el admin lo configura después
                    IsActive = true
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
