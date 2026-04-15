using Application.Abstractions.Dashboard;
using Application.DTOs.Dashboard;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly POSDbContext _context;

        public DashboardRepository(POSDbContext context)
        {
            _context = context;
        }

        // ══════════════════════════════════════════════════════════════════
        // USER INFO
        // ══════════════════════════════════════════════════════════════════

        public async Task<(string UserName, string RoleName, int RoleId)> GetUserInfoAsync(int userId, CancellationToken ct)
        {
            var user = await _context.User
                .Include(u => u.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && u.Active, ct);

            if (user == null) return (string.Empty, string.Empty, 0);

            return (user.Name, user.Role?.Name ?? "Sin rol", user.RoleId);
        }

        // ══════════════════════════════════════════════════════════════════
        // MÓDULOS ACCESIBLES
        // ══════════════════════════════════════════════════════════════════

        public async Task<HashSet<string>> GetAccessibleModuleNamesAsync(int userId, int roleId, CancellationToken ct)
        {
            // Admin bypass: si el rol es Administrador devolver todos los módulos activos
            var roleName = await _context.Roles
                .Where(r => r.Id == roleId)
                .Select(r => r.Name)
                .FirstOrDefaultAsync(ct);

            if (string.Equals(roleName, "Administrador", StringComparison.OrdinalIgnoreCase))
            {
                var allModules = await _context.Modules
                    .Where(m => m.IsActive)
                    .AsNoTracking()
                    .Select(m => m.Name)
                    .ToListAsync(ct);

                return new HashSet<string>(allModules, StringComparer.OrdinalIgnoreCase);
            }

            var roleModules = await _context.RoleModulePermissions
                .Where(rp => rp.RoleId == roleId && rp.HasAccess && rp.SubmoduleId == null)
                .AsNoTracking()
                .Select(rp => rp.Name)
                .Distinct()
                .ToListAsync(ct);

            var userModules = await _context.UserModulePermissions
                .Where(up => up.UserId == userId && up.HasAccess && up.SubmoduleId == null)
                .AsNoTracking()
                .Select(up => up.Name)
                .Distinct()
                .ToListAsync(ct);

            var result = new HashSet<string>(roleModules, StringComparer.OrdinalIgnoreCase);
            foreach (var m in userModules) result.Add(m);
            return result;
        }

        // ══════════════════════════════════════════════════════════════════
        // VENTAS
        // ══════════════════════════════════════════════════════════════════

        public async Task<SalesDashboardSectionDto> GetSalesSectionAsync(int? companyId, DateTime today, DateTime firstOfMonth, CancellationToken ct)
        {
            var query = _context.SalesNew.AsNoTracking();
            if (companyId.HasValue)
                query = query.Where(s => s.CompanyId == companyId.Value);

            var todaySales = await query
                .Where(s => s.SaleDate >= today && s.SaleDate < today.AddDays(1) && s.Status != "Cancelled")
                .Select(s => new { s.Total })
                .ToListAsync(ct);

            var monthSales = await query
                .Where(s => s.SaleDate >= firstOfMonth && s.Status != "Cancelled")
                .Select(s => new { s.Total })
                .ToListAsync(ct);

            var pendingSales = await query
                .Where(s => s.Status == "Draft")
                .Select(s => new { s.Total })
                .ToListAsync(ct);

            var profitData = await query
                .Where(s => s.SaleDate >= firstOfMonth && s.Status == "Completed")
                .SelectMany(s => s.Details)
                .Select(d => new { d.Total, Cost = d.TotalCost ?? 0 })
                .ToListAsync(ct);

            var revenue = profitData.Sum(d => d.Total);
            var cost = profitData.Sum(d => d.Cost);
            var grossProfit = revenue - cost;
            var margin = revenue > 0 ? Math.Round(grossProfit / revenue * 100, 2) : 0;

            var topProducts = await query
                .Where(s => s.SaleDate >= firstOfMonth && s.Status != "Cancelled")
                .SelectMany(s => s.Details)
                .GroupBy(d => d.ProductName)
                .Select(g => new TopProductSaleDto
                {
                    ProductName = g.Key,
                    QuantitySold = (int)g.Sum(d => d.Quantity),
                    TotalRevenue = g.Sum(d => d.Total)
                })
                .OrderByDescending(p => p.TotalRevenue)
                .Take(5)
                .ToListAsync(ct);

            var recentSales = await query
                .OrderByDescending(s => s.SaleDate)
                .Take(5)
                .Select(s => new RecentSaleDto
                {
                    Code = s.Code,
                    SaleDate = s.SaleDate,
                    CustomerName = s.CustomerName ?? "Público General",
                    Total = s.Total,
                    Status = s.Status
                })
                .ToListAsync(ct);

            return new SalesDashboardSectionDto
            {
                SalesTodayCount = todaySales.Count,
                SalesTodayAmount = todaySales.Sum(s => s.Total),
                SalesThisMonthCount = monthSales.Count,
                SalesThisMonthAmount = monthSales.Sum(s => s.Total),
                SalesPendingCount = pendingSales.Count,
                SalesPendingAmount = pendingSales.Sum(s => s.Total),
                GrossProfitThisMonth = grossProfit,
                ProfitMarginThisMonth = margin,
                TopProducts = topProducts,
                RecentSales = recentSales
            };
        }

        // ══════════════════════════════════════════════════════════════════
        // INVENTARIO
        // ══════════════════════════════════════════════════════════════════

        public async Task<InventoryDashboardSectionDto> GetInventorySectionAsync(int? companyId, DateTime today, DateTime firstOfMonth, CancellationToken ct)
        {
            var stockQuery = _context.ProductStock
                .AsNoTracking()
                .Where(ps => ps.Product.IsActive);

            if (companyId.HasValue)
                stockQuery = stockQuery.Where(ps => ps.Warehouse.Branch.CompanyId == companyId.Value);

            var stockList = await stockQuery
                .Select(ps => new
                {
                    ps.Quantity,
                    ps.MinimumStock,
                    ps.AverageCost,
                    ProductCode = ps.Product.code,
                    ProductName = ps.Product.name,
                    WarehouseName = ps.Warehouse.Name
                })
                .ToListAsync(ct);

            var totalActiveProducts = await _context.Products.CountAsync(p => p.IsActive, ct);

            var lowStock = stockList
                .Where(s => s.MinimumStock.HasValue && s.MinimumStock > 0 && s.Quantity <= s.MinimumStock.Value)
                .ToList();

            var outOfStock = stockList.Count(s => s.Quantity <= 0);
            var totalStockValue = stockList.Sum(s => s.Quantity * (s.AverageCost ?? 0));

            var movQuery = _context.InventoryMovements.AsNoTracking();
            if (companyId.HasValue)
                movQuery = movQuery.Where(m => m.Warehouse.Branch.CompanyId == companyId.Value);

            var movementsToday = await movQuery
                .CountAsync(m => m.MovementDate >= today && m.MovementDate < today.AddDays(1), ct);

            var movementsMonth = await movQuery.CountAsync(m => m.MovementDate >= firstOfMonth, ct);

            var pendingTransfers = await _context.StockTransfers
                .CountAsync(t => t.Status == "Pending" || t.Status == "InTransit", ct);

            var lowStockDtos = lowStock
                .OrderBy(s => s.Quantity)
                .Take(10)
                .Select(s => new LowStockProductDto
                {
                    ProductCode = s.ProductCode,
                    ProductName = s.ProductName,
                    CurrentStock = s.Quantity,
                    MinStock = s.MinimumStock ?? 0,
                    WarehouseName = s.WarehouseName
                })
                .ToList();

            return new InventoryDashboardSectionDto
            {
                TotalActiveProducts = totalActiveProducts,
                LowStockProductsCount = lowStock.Count,
                OutOfStockCount = outOfStock,
                TotalStockValue = Math.Round(totalStockValue, 2),
                MovementsToday = movementsToday,
                MovementsThisMonth = movementsMonth,
                PendingTransfersCount = pendingTransfers,
                LowStockProducts = lowStockDtos
            };
        }

        // ══════════════════════════════════════════════════════════════════
        // FACTURACIÓN CFDI
        // ══════════════════════════════════════════════════════════════════

        public async Task<BillingDashboardSectionDto> GetBillingSectionAsync(int? companyId, DateTime firstOfMonth, CancellationToken ct)
        {
            var pendingQuery = _context.SalesNew
                .AsNoTracking()
                .Where(s => s.Status == "Completed" && s.IsPaid && s.InvoiceUuid == null && s.RequiresInvoice);

            if (companyId.HasValue)
                pendingQuery = pendingQuery.Where(s => s.CompanyId == companyId.Value);

            var pending = await pendingQuery.Select(s => new { s.Total }).ToListAsync(ct);

            var invoiceQuery = _context.Invoices.AsNoTracking()
                .Where(i => i.InvoiceDate >= firstOfMonth);

            if (companyId.HasValue)
                invoiceQuery = invoiceQuery.Where(i => i.CompanyId == companyId.Value);

            var issuedThisMonth = await invoiceQuery
                .Where(i => i.Status != "Cancelled")
                .Select(i => new { i.Total })
                .ToListAsync(ct);

            var cancelledThisMonth = await invoiceQuery.CountAsync(i => i.Status == "Cancelled", ct);

            return new BillingDashboardSectionDto
            {
                PendingToInvoiceCount = pending.Count,
                PendingToInvoiceAmount = pending.Sum(s => s.Total),
                IssuedThisMonthCount = issuedThisMonth.Count,
                IssuedThisMonthAmount = issuedThisMonth.Sum(i => i.Total),
                CancelledThisMonthCount = cancelledThisMonth
            };
        }

        // ══════════════════════════════════════════════════════════════════
        // CUENTAS POR COBRAR
        // ══════════════════════════════════════════════════════════════════

        public async Task<AccountsReceivableDashboardSectionDto> GetAccountsReceivableSectionAsync(int? companyId, DateTime firstOfMonth, CancellationToken ct)
        {
            var invoiceQuery = _context.Invoices
                .AsNoTracking()
                .Where(i => i.Status != "Cancelled" && i.PaymentStatus == "Pending");

            if (companyId.HasValue)
                invoiceQuery = invoiceQuery.Where(i => i.CompanyId == companyId.Value);

            var pendingInvoices = await invoiceQuery
                .Select(i => new { i.BalanceAmount, i.DueDate })
                .ToListAsync(ct);

            var today = DateTime.UtcNow.Date;
            var totalReceivable = pendingInvoices.Sum(i => i.BalanceAmount ?? 0);
            var totalOverdue = pendingInvoices.Where(i => i.DueDate.HasValue && i.DueDate < today).Sum(i => i.BalanceAmount ?? 0);
            var overdueCount = pendingInvoices.Count(i => i.DueDate.HasValue && i.DueDate < today);
            var overduePercentage = totalReceivable > 0 ? Math.Round(totalOverdue / totalReceivable * 100, 2) : 0;

            var collectionsQuery = _context.PaymentBatches
                .AsNoTracking()
                .Where(pb => pb.PaymentDate >= firstOfMonth && pb.Status == "Applied");

            if (companyId.HasValue)
                collectionsQuery = collectionsQuery.Where(pb => pb.CompanyId == companyId.Value);

            var collectionsThisMonth = await collectionsQuery.SumAsync(pb => (decimal?)pb.TotalAmount ?? 0, ct);

            return new AccountsReceivableDashboardSectionDto
            {
                TotalReceivable = Math.Round(totalReceivable, 2),
                TotalOverdue = Math.Round(totalOverdue, 2),
                CollectionsThisMonth = Math.Round(collectionsThisMonth, 2),
                InvoicesPendingCount = pendingInvoices.Count,
                OverdueInvoicesCount = overdueCount,
                OverduePercentage = overduePercentage
            };
        }

        // ══════════════════════════════════════════════════════════════════
        // COMPRAS
        // ══════════════════════════════════════════════════════════════════

        public async Task<PurchasingDashboardSectionDto> GetPurchasingSectionAsync(int? companyId, DateTime firstOfMonth, CancellationToken ct)
        {
            var poQuery = _context.PurchaseOrders.AsNoTracking();

            if (companyId.HasValue)
                poQuery = poQuery.Where(po => po.Warehouse.Branch.CompanyId == companyId.Value);

            var pendingStatuses = new[] { "Pending", "Approved", "InTransit", "PartiallyReceived" };

            var pendingPOs = await poQuery
                .Where(po => pendingStatuses.Contains(po.Status))
                .Select(po => new { po.Total })
                .ToListAsync(ct);

            var monthPOs = await poQuery
                .Where(po => po.OrderDate >= firstOfMonth)
                .Select(po => new { po.Total })
                .ToListAsync(ct);

            var pendingReceivings = await poQuery
                .CountAsync(po => po.Status == "Approved" || po.Status == "PartiallyReceived", ct);

            return new PurchasingDashboardSectionDto
            {
                PendingPOsCount = pendingPOs.Count,
                PendingPOsAmount = pendingPOs.Sum(po => po.Total),
                POsThisMonthCount = monthPOs.Count,
                POsThisMonthAmount = monthPOs.Sum(po => po.Total),
                PendingReceivingsCount = pendingReceivings
            };
        }

        // ══════════════════════════════════════════════════════════════════
        // CLIENTES
        // ══════════════════════════════════════════════════════════════════

        public async Task<CustomersDashboardSectionDto> GetCustomersSectionAsync(int? companyId, DateTime firstOfMonth, CancellationToken ct)
        {
            var totalActive = await _context.Customer.CountAsync(c => c.IsActive, ct);

            var newThisMonth = await _context.Customer
                .CountAsync(c => c.IsActive && c.CreatedAt != null && c.CreatedAt >= firstOfMonth, ct);

            var invoiceQuery = _context.Invoices
                .AsNoTracking()
                .Where(i => i.Status != "Cancelled" && i.PaymentStatus == "Pending" && i.CustomerId != null);

            if (companyId.HasValue)
                invoiceQuery = invoiceQuery.Where(i => i.CompanyId == companyId.Value);

            var customersWithBalance = await invoiceQuery
                .Select(i => i.CustomerId)
                .Distinct()
                .CountAsync(ct);

            var totalBalance = await invoiceQuery.SumAsync(i => i.BalanceAmount ?? 0, ct);

            return new CustomersDashboardSectionDto
            {
                TotalActiveCustomers = totalActive,
                NewCustomersThisMonth = newThisMonth,
                CustomersWithBalance = customersWithBalance,
                TotalCreditBalance = Math.Round(totalBalance, 2)
            };
        }

        // ══════════════════════════════════════════════════════════════════
        // PRODUCTOS
        // ══════════════════════════════════════════════════════════════════

        public async Task<ProductsDashboardSectionDto> GetProductsSectionAsync(CancellationToken ct)
        {
            var totalActive = await _context.Products.CountAsync(p => p.IsActive, ct);
            var totalCategories = await _context.ProductCategories.CountAsync(c => c.IsActive, ct);

            var productsWithPrice = await _context.ProductPrices
                .Select(pp => pp.ProductId)
                .Distinct()
                .CountAsync(ct);

            var productsWithImage = await _context.ProductImages
                .Select(pi => pi.ProductId)
                .Distinct()
                .CountAsync(ct);

            return new ProductsDashboardSectionDto
            {
                TotalActiveProducts = totalActive,
                TotalCategories = totalCategories,
                ProductsWithoutPrice = Math.Max(0, totalActive - productsWithPrice),
                ProductsWithoutImage = Math.Max(0, totalActive - productsWithImage)
            };
        }

        // ══════════════════════════════════════════════════════════════════
        // ALERTAS
        // ══════════════════════════════════════════════════════════════════

        public async Task<AlertsDashboardSectionDto> GetAlertsSectionAsync(int userId, int roleId, int? companyId, CancellationToken ct)
        {
            var alertQuery = _context.Alerts
                .AsNoTracking()
                .Where(a => a.Status != "Resolved" &&
                            (a.UserId == userId || a.TargetRoleId == roleId || (a.UserId == null && a.TargetRoleId == null)));

            if (companyId.HasValue)
                alertQuery = alertQuery.Where(a => a.CompanyId == companyId.Value || a.CompanyId == null);

            var alerts = await alertQuery
                .Select(a => new { a.Severity, a.Status })
                .ToListAsync(ct);

            return new AlertsDashboardSectionDto
            {
                TotalActiveAlerts = alerts.Count,
                HighPriorityAlerts = alerts.Count(a => a.Severity == "Critical"),
                UnreadAlerts = alerts.Count(a => a.Status == "Pending")
            };
        }
    }
}
