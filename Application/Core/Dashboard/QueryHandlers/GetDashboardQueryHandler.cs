using Application.Abstractions.Dashboard;
using Application.Core.Dashboard.Queries;
using Application.DTOs.Dashboard;
using MediatR;

namespace Application.Core.Dashboard.QueryHandlers
{
    /// <summary>
    /// Ensambla el dashboard consultando solo las secciones a las que el usuario tiene acceso.
    /// </summary>
    public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardResponseDto>
    {
        private readonly IDashboardRepository _dashboardRepo;

        public GetDashboardQueryHandler(IDashboardRepository dashboardRepo)
        {
            _dashboardRepo = dashboardRepo;
        }

        public async Task<DashboardResponseDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
        {
            var (userName, roleName, roleId) = await _dashboardRepo.GetUserInfoAsync(request.UserId, cancellationToken);

            if (string.IsNullOrEmpty(userName))
                return new DashboardResponseDto { Message = "Usuario no encontrado", Error = 1 };

            var accessibleModules = await _dashboardRepo.GetAccessibleModuleNamesAsync(request.UserId, roleId, cancellationToken);

            var now = DateTime.UtcNow;
            var today = now.Date;
            var firstOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var response = new DashboardResponseDto
            {
                Message = "Dashboard generado exitosamente",
                Error = 0,
                UserName = userName,
                RoleName = roleName,
                GeneratedAt = now
            };

            // DbContext es Scoped (no thread-safe) — ejecutar en secuencia
            var ct = cancellationToken;

            if (accessibleModules.Contains("Ventas"))
                response.Sales = await _dashboardRepo.GetSalesSectionAsync(request.CompanyId, today, firstOfMonth, ct);

            if (accessibleModules.Contains("Inventario"))
                response.Inventory = await _dashboardRepo.GetInventorySectionAsync(request.CompanyId, today, firstOfMonth, ct);

            if (accessibleModules.Contains("CFDI"))
            {
                response.Billing = await _dashboardRepo.GetBillingSectionAsync(request.CompanyId, firstOfMonth, ct);
                response.AccountsReceivable = await _dashboardRepo.GetAccountsReceivableSectionAsync(request.CompanyId, firstOfMonth, ct);
            }

            if (accessibleModules.Contains("Compras"))
                response.Purchasing = await _dashboardRepo.GetPurchasingSectionAsync(request.CompanyId, firstOfMonth, ct);

            if (accessibleModules.Contains("Clientes"))
                response.Customers = await _dashboardRepo.GetCustomersSectionAsync(request.CompanyId, firstOfMonth, ct);

            if (accessibleModules.Contains("Productos"))
                response.Products = await _dashboardRepo.GetProductsSectionAsync(ct);

            if (accessibleModules.Any())
                response.Alerts = await _dashboardRepo.GetAlertsSectionAsync(request.UserId, roleId, request.CompanyId, ct);

            return response;
        }
    }
}
