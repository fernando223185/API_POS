using Application.Abstractions.Alerts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs
{
    /// <summary>
    /// Job que corre cada 30 minutos y genera alertas de stock mínimo/crítico.
    /// A qué rol se envía lo define AlertRuleConfig en BD (configurable por el admin).
    /// Si el stock se recupera, resuelve automáticamente las alertas abiertas.
    /// </summary>
    public class StockAlertJob : BackgroundService
    {
        private static readonly TimeSpan _interval = TimeSpan.FromMinutes(30);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<StockAlertJob> _logger;

        public StockAlertJob(IServiceScopeFactory scopeFactory, ILogger<StockAlertJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("StockAlertJob iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en StockAlertJob.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task RunAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<POSDbContext>();
            var alertRepo = scope.ServiceProvider.GetRequiredService<IAlertRepository>();
            var ruleRepo = scope.ServiceProvider.GetRequiredService<IAlertRuleConfigRepository>();

            var stockRecords = await db.ProductStock
                .AsNoTracking()
                .Include(ps => ps.Product)
                .Include(ps => ps.Warehouse)
                    .ThenInclude(w => w.Branch)
                .Where(ps => ps.MinimumStock != null && ps.MinimumStock > 0)
                .Select(ps => new
                {
                    ps.Id,
                    ps.ProductId,
                    ps.Quantity,
                    ps.MinimumStock,
                    ProductName = ps.Product.name,
                    WarehouseName = ps.Warehouse.Name,
                    ps.Warehouse.BranchId,
                    CompanyId = ps.Warehouse.Branch.CompanyId
                })
                .ToListAsync(ct);

            if (!stockRecords.Any()) return;

            // Cargar reglas por empresa
            var companyIds = stockRecords
                .Select(ps => ps.CompanyId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var ruleMin = new Dictionary<int, AlertRuleConfig?>();
            var ruleCritical = new Dictionary<int, AlertRuleConfig?>();

            foreach (var companyId in companyIds)
            {
                await ruleRepo.EnsureDefaultsAsync(companyId);
                ruleMin[companyId] = await ruleRepo.GetAsync("StockMin", companyId);
                ruleCritical[companyId] = await ruleRepo.GetAsync("StockCritical", companyId);
            }

            foreach (var ps in stockRecords)
            {
                var company = ps.CompanyId;
                if (company == null) continue;
                var cid = company.Value;

                var min = ps.MinimumStock!.Value;
                var isCritical = ps.Quantity <= min * 0.5m;
                var isAtMin = ps.Quantity <= min;

                var minKey = $"STOCKMIN_PS_{ps.Id}";
                var criticalKey = $"STOCKCRITICAL_PS_{ps.Id}";

                if (isAtMin)
                {
                    var rule = isCritical
                        ? ruleCritical.GetValueOrDefault(cid)
                        : ruleMin.GetValueOrDefault(cid);

                    // Si la regla está desactivada, omitir
                    if (rule != null && !rule.IsActive)
                    {
                        if (!isCritical) await alertRepo.ResolveByUniqueKeyAsync(criticalKey);
                        continue;
                    }

                    var level = isCritical ? "StockCritical" : "StockMin";
                    var key = isCritical ? criticalKey : minKey;
                    var severity = isCritical ? "Critical" : "Warning";

                    var title = isCritical
                        ? $"Stock crítico: {ps.ProductName}"
                        : $"Stock mínimo alcanzado: {ps.ProductName}";

                    var message = isCritical
                        ? $"El producto '{ps.ProductName}' en almacén '{ps.WarehouseName}' tiene stock crítico ({ps.Quantity} uds). Mínimo configurado: {min}. Requiere reabastecimiento urgente."
                        : $"El producto '{ps.ProductName}' en almacén '{ps.WarehouseName}' llegó al stock mínimo ({ps.Quantity} uds). Mínimo configurado: {min}. Coordinar reabastecimiento.";

                    var existing = await alertRepo.GetOpenByUniqueKeyAsync(key);
                    if (existing != null)
                    {
                        await alertRepo.TouchAsync(existing.Id);
                    }
                    else
                    {
                        await alertRepo.CreateAsync(new Alert
                        {
                            Type = level,
                            Severity = severity,
                            ReferenceId = ps.Id,
                            ReferenceType = "ProductStock",
                            BranchId = ps.BranchId,
                            CompanyId = cid,
                            TargetRoleId = rule?.TargetRoleId,
                            Title = title,
                            Message = message,
                            UniqueKey = key
                        });

                        _logger.LogInformation("Alerta creada: {Key}", key);
                    }

                    if (!isCritical)
                        await alertRepo.ResolveByUniqueKeyAsync(criticalKey);
                }
                else
                {
                    await alertRepo.ResolveByUniqueKeyAsync(minKey);
                    await alertRepo.ResolveByUniqueKeyAsync(criticalKey);
                }
            }
        }
    }
}
