namespace Application.DTOs.Dashboard
{
    /// <summary>
    /// Respuesta principal del dashboard — cada sección es null si el usuario no tiene acceso al módulo
    /// </summary>
    public class DashboardResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Sección de Ventas (módulo "Ventas")</summary>
        public SalesDashboardSectionDto? Sales { get; set; }

        /// <summary>Sección de Inventario (módulo "Inventario")</summary>
        public InventoryDashboardSectionDto? Inventory { get; set; }

        /// <summary>Sección de Facturación CFDI (módulo "CFDI")</summary>
        public BillingDashboardSectionDto? Billing { get; set; }

        /// <summary>Sección de Cuentas por Cobrar (módulo "CFDI" + pagos)</summary>
        public AccountsReceivableDashboardSectionDto? AccountsReceivable { get; set; }

        /// <summary>Sección de Compras (módulo "Compras")</summary>
        public PurchasingDashboardSectionDto? Purchasing { get; set; }

        /// <summary>Sección de Clientes (módulo "Clientes")</summary>
        public CustomersDashboardSectionDto? Customers { get; set; }

        /// <summary>Sección de Productos (módulo "Productos")</summary>
        public ProductsDashboardSectionDto? Products { get; set; }

        /// <summary>Alertas del sistema (si tiene acceso)</summary>
        public AlertsDashboardSectionDto? Alerts { get; set; }
    }

    // ─────────────────────────────────────────────────────────────
    // VENTAS
    // ─────────────────────────────────────────────────────────────

    public class SalesDashboardSectionDto
    {
        public int SalesTodayCount { get; set; }
        public decimal SalesTodayAmount { get; set; }

        public int SalesThisMonthCount { get; set; }
        public decimal SalesThisMonthAmount { get; set; }

        public int SalesPendingCount { get; set; }
        public decimal SalesPendingAmount { get; set; }

        public decimal GrossProfitThisMonth { get; set; }
        public decimal ProfitMarginThisMonth { get; set; }

        public List<TopProductSaleDto> TopProducts { get; set; } = new();
        public List<RecentSaleDto> RecentSales { get; set; } = new();
    }

    public class TopProductSaleDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class RecentSaleDto
    {
        public string Code { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // ─────────────────────────────────────────────────────────────
    // INVENTARIO
    // ─────────────────────────────────────────────────────────────

    public class InventoryDashboardSectionDto
    {
        public int TotalActiveProducts { get; set; }
        public int LowStockProductsCount { get; set; }
        public int OutOfStockCount { get; set; }
        public decimal TotalStockValue { get; set; }

        public int MovementsToday { get; set; }
        public int MovementsThisMonth { get; set; }

        public int PendingTransfersCount { get; set; }

        public List<LowStockProductDto> LowStockProducts { get; set; } = new();
    }

    public class LowStockProductDto
    {
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal MinStock { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
    }

    // ─────────────────────────────────────────────────────────────
    // FACTURACIÓN CFDI
    // ─────────────────────────────────────────────────────────────

    public class BillingDashboardSectionDto
    {
        public int PendingToInvoiceCount { get; set; }
        public decimal PendingToInvoiceAmount { get; set; }

        public int IssuedThisMonthCount { get; set; }
        public decimal IssuedThisMonthAmount { get; set; }

        public int CancelledThisMonthCount { get; set; }
    }

    // ─────────────────────────────────────────────────────────────
    // CUENTAS POR COBRAR
    // ─────────────────────────────────────────────────────────────

    public class AccountsReceivableDashboardSectionDto
    {
        public decimal TotalReceivable { get; set; }
        public decimal TotalOverdue { get; set; }
        public decimal CollectionsThisMonth { get; set; }

        public int InvoicesPendingCount { get; set; }
        public int OverdueInvoicesCount { get; set; }

        public decimal OverduePercentage { get; set; }
    }

    // ─────────────────────────────────────────────────────────────
    // COMPRAS
    // ─────────────────────────────────────────────────────────────

    public class PurchasingDashboardSectionDto
    {
        public int PendingPOsCount { get; set; }
        public decimal PendingPOsAmount { get; set; }

        public int POsThisMonthCount { get; set; }
        public decimal POsThisMonthAmount { get; set; }

        public int PendingReceivingsCount { get; set; }
    }

    // ─────────────────────────────────────────────────────────────
    // CLIENTES
    // ─────────────────────────────────────────────────────────────

    public class CustomersDashboardSectionDto
    {
        public int TotalActiveCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int CustomersWithBalance { get; set; }
        public decimal TotalCreditBalance { get; set; }
    }

    // ─────────────────────────────────────────────────────────────
    // PRODUCTOS (CATÁLOGO)
    // ─────────────────────────────────────────────────────────────

    public class ProductsDashboardSectionDto
    {
        public int TotalActiveProducts { get; set; }
        public int TotalCategories { get; set; }
        public int ProductsWithoutPrice { get; set; }
        public int ProductsWithoutImage { get; set; }
    }

    // ─────────────────────────────────────────────────────────────
    // ALERTAS
    // ─────────────────────────────────────────────────────────────

    public class AlertsDashboardSectionDto
    {
        public int TotalActiveAlerts { get; set; }
        public int HighPriorityAlerts { get; set; }
        public int UnreadAlerts { get; set; }
    }
}
