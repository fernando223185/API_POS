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
    /// Job que corre cada 6 horas y genera alertas para facturas PPD próximas a vencer o vencidas.
    /// Etapas: 7 días, 3 días, 1 día, vencida hoy.
    /// A qué rol se envía cada alerta lo define AlertRuleConfig en BD (configurable por el admin).
    /// </summary>
    public class InvoiceDueAlertJob : BackgroundService
    {
        private static readonly TimeSpan _interval = TimeSpan.FromHours(6);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<InvoiceDueAlertJob> _logger;

        public InvoiceDueAlertJob(IServiceScopeFactory scopeFactory, ILogger<InvoiceDueAlertJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("InvoiceDueAlertJob iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en InvoiceDueAlertJob.");
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

            var today = DateTime.UtcNow.Date;

            // Facturas PPD activas con DueDate definido
            var invoices = await db.Invoices
                .AsNoTracking()
                .Where(i =>
                    i.MetodoPago == "PPD" &&
                    i.DueDate != null &&
                    i.Status != "Cancelada" &&
                    i.Status != "Borrador" &&
                    i.PaymentStatus != "Paid" &&
                    i.PaymentStatus != "Cancelled")
                .Select(i => new
                {
                    i.Id,
                    i.Serie,
                    i.Folio,
                    i.DueDate,
                    i.BalanceAmount,
                    i.CompanyId,
                    i.CreatedByUserId
                })
                .ToListAsync(ct);

            if (!invoices.Any()) return;

            // Cargar reglas por empresa (una sola query por empresa)
            var companyIds = invoices.Select(i => i.CompanyId).Distinct().ToList();
            var rules = new Dictionary<int, AlertRuleConfig?>();

            foreach (var companyId in companyIds)
            {
                await ruleRepo.EnsureDefaultsAsync(companyId);
                rules[companyId] = await ruleRepo.GetAsync("InvoiceDue", companyId);
            }

            foreach (var inv in invoices)
            {
                var rule = rules.GetValueOrDefault(inv.CompanyId);

                // Si la regla existe pero está desactivada, omitir
                if (rule != null && !rule.IsActive) continue;

                var dueDate = inv.DueDate!.Value.Date;
                var daysLeft = (dueDate - today).Days;

                string? stage = daysLeft switch
                {
                    7 => "7DAYS",
                    3 => "3DAYS",
                    1 => "1DAY",
                    0 => "TODAY",
                    < 0 => "OVERDUE",
                    _ => null
                };

                if (stage == null) continue;

                var uniqueKey = $"INVOICEDUE_INV_{inv.Id}_STAGE_{stage}";

                var existing = await alertRepo.GetOpenByUniqueKeyAsync(uniqueKey);
                if (existing != null)
                {
                    await alertRepo.TouchAsync(existing.Id);
                    continue;
                }

                var (title, message, severity) = stage switch
                {
                    "7DAYS" => (
                        $"Factura {inv.Serie}{inv.Folio} vence en 7 días",
                        $"La factura {inv.Serie}{inv.Folio} tiene saldo de {inv.BalanceAmount:C} y vence el {dueDate:dd/MM/yyyy}.",
                        "Info"),
                    "3DAYS" => (
                        $"Factura {inv.Serie}{inv.Folio} vence en 3 días",
                        $"La factura {inv.Serie}{inv.Folio} tiene saldo de {inv.BalanceAmount:C} y vence el {dueDate:dd/MM/yyyy}. Se acerca la fecha límite.",
                        "Warning"),
                    "1DAY" => (
                        $"Factura {inv.Serie}{inv.Folio} vence mañana",
                        $"La factura {inv.Serie}{inv.Folio} tiene saldo de {inv.BalanceAmount:C} y vence mañana {dueDate:dd/MM/yyyy}.",
                        "Warning"),
                    "TODAY" => (
                        $"Factura {inv.Serie}{inv.Folio} vence hoy",
                        $"La factura {inv.Serie}{inv.Folio} tiene saldo de {inv.BalanceAmount:C} y vence hoy. Gestiona el cobro de inmediato.",
                        "Critical"),
                    _ => (
                        $"Factura {inv.Serie}{inv.Folio} vencida",
                        $"La factura {inv.Serie}{inv.Folio} está vencida desde el {dueDate:dd/MM/yyyy} con saldo pendiente de {inv.BalanceAmount:C}.",
                        "Critical")
                };

                await alertRepo.CreateAsync(new Alert
                {
                    Type = "InvoiceDue",
                    Severity = severity,
                    ReferenceId = inv.Id,
                    ReferenceType = "Invoice",
                    CompanyId = inv.CompanyId,
                    // Si la regla tiene TargetRoleId, la alerta va al rol.
                    // Si es null (broadcast), va a todos en la empresa.
                    // Si la regla no existe aún, igual broadcast.
                    TargetRoleId = rule?.TargetRoleId,
                    UserId = rule?.TargetRoleId == null ? inv.CreatedByUserId : null,
                    Title = title,
                    Message = message,
                    UniqueKey = uniqueKey
                });

                _logger.LogInformation("Alerta creada: {UniqueKey}", uniqueKey);
            }
        }
    }
}
