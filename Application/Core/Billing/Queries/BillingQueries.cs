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
    /// Incluye toda la informaci�n necesaria para el proceso de timbrado
    /// </summary>
    public record GetSaleForInvoicingQuery(
        int SaleId
    ) : IRequest<SaleForInvoicingDto>;

    /// <summary>
    /// Query para obtener facturas con filtros opcionales (para dashboard/listados)
    /// </summary>
    public record GetInvoicesQuery(
        int Page = 1,
        int PageSize = 50,
        int? CompanyId = null,
        int? CustomerId = null,
        string? Status = null,  // "Borrador", "Timbrada", "Cancelada"
        DateTime? FromDate = null,
        DateTime? ToDate = null,
        string? Serie = null,
        string? Rfc = null
    ) : IRequest<InvoicesPagedResponseDto>;

    /// <summary>
    /// Query para obtener una factura por ID con todos sus detalles
    /// </summary>
    public record GetInvoiceByIdQuery(
        int InvoiceId
    ) : IRequest<InvoiceResponseDto>;
}
