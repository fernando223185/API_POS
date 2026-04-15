using Application.DTOs.Dashboard;

namespace Application.Abstractions.Dashboard
{
    /// <summary>
    /// Proporciona los datos crudos para cada sección del dashboard.
    /// Implementado en Infrastructure para acceder directamente al DbContext.
    /// </summary>
    public interface IDashboardRepository
    {
        Task<SalesDashboardSectionDto> GetSalesSectionAsync(int? companyId, DateTime today, DateTime firstOfMonth, CancellationToken ct);
        Task<InventoryDashboardSectionDto> GetInventorySectionAsync(int? companyId, DateTime today, DateTime firstOfMonth, CancellationToken ct);
        Task<BillingDashboardSectionDto> GetBillingSectionAsync(int? companyId, DateTime firstOfMonth, CancellationToken ct);
        Task<AccountsReceivableDashboardSectionDto> GetAccountsReceivableSectionAsync(int? companyId, DateTime firstOfMonth, CancellationToken ct);
        Task<PurchasingDashboardSectionDto> GetPurchasingSectionAsync(int? companyId, DateTime firstOfMonth, CancellationToken ct);
        Task<CustomersDashboardSectionDto> GetCustomersSectionAsync(int? companyId, DateTime firstOfMonth, CancellationToken ct);
        Task<ProductsDashboardSectionDto> GetProductsSectionAsync(CancellationToken ct);
        Task<AlertsDashboardSectionDto> GetAlertsSectionAsync(int userId, int roleId, int? companyId, CancellationToken ct);

        /// <summary>Devuelve los nombres de módulos a los que tiene acceso el usuario (vía rol o permisos personales)</summary>
        Task<HashSet<string>> GetAccessibleModuleNamesAsync(int userId, int roleId, CancellationToken ct);

        Task<(string UserName, string RoleName, int RoleId)> GetUserInfoAsync(int userId, CancellationToken ct);
    }
}
