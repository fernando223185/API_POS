using Application.DTOs.Billing;
using MediatR;

namespace Application.Core.Billing.Queries
{
    /// <summary>
    /// Query para obtener ventas pendientes de timbrar (facturar)
    /// </summary>
    public record GetPendingInvoiceSalesQuery(
        int Page,
        int PageSize,
        bool? OnlyRequiresInvoice,
        int? WarehouseId,
        int? BranchId,
        int? CompanyId,
        DateTime? FromDate,
        DateTime? ToDate
    ) : IRequest<PendingInvoiceSalesResponseDto>;

    /// <summary>
    /// Query para obtener una venta individual para facturar
    /// Incluye toda la información necesaria para el proceso de timbrado
    /// </summary>
    public record GetSaleForInvoicingQuery(
        int SaleId
    ) : IRequest<SaleForInvoicingDto>;
}
