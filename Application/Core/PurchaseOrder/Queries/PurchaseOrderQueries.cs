using Application.DTOs.PurchaseOrder;
using MediatR;

namespace Application.Core.PurchaseOrder.Queries
{
    /// <summary>
    /// Query para obtener todas las órdenes de compra
    /// </summary>
    public class GetAllPurchaseOrdersQuery : IRequest<PurchaseOrdersListResponseDto>
    {
        public bool IncludeInactive { get; set; }

        public GetAllPurchaseOrdersQuery(bool includeInactive = false)
        {
            IncludeInactive = includeInactive;
        }
    }

    /// <summary>
    /// Query para obtener órdenes paginadas
    /// </summary>
    public class GetPurchaseOrdersPagedQuery : IRequest<PurchaseOrdersPagedResponseDto>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool IncludeInactive { get; set; }
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public int? SupplierId { get; set; }
        public int? WarehouseId { get; set; }

        public GetPurchaseOrdersPagedQuery(
            int pageNumber,
            int pageSize,
            bool includeInactive = false,
            string? searchTerm = null,
            string? status = null,
            int? supplierId = null,
            int? warehouseId = null)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            IncludeInactive = includeInactive;
            SearchTerm = searchTerm;
            Status = status;
            SupplierId = supplierId;
            WarehouseId = warehouseId;
        }
    }

    /// <summary>
    /// Query para obtener orden por ID
    /// </summary>
    public class GetPurchaseOrderByIdQuery : IRequest<PurchaseOrderResponseDto?>
    {
        public int PurchaseOrderId { get; set; }

        public GetPurchaseOrderByIdQuery(int purchaseOrderId)
        {
            PurchaseOrderId = purchaseOrderId;
        }
    }

    /// <summary>
    /// Query para obtener orden por código
    /// </summary>
    public class GetPurchaseOrderByCodeQuery : IRequest<PurchaseOrderResponseDto?>
    {
        public string Code { get; set; }

        public GetPurchaseOrderByCodeQuery(string code)
        {
            Code = code;
        }
    }

    /// <summary>
    /// Query para obtener órdenes por proveedor
    /// </summary>
    public class GetPurchaseOrdersBySupplierQuery : IRequest<PurchaseOrdersListResponseDto>
    {
        public int SupplierId { get; set; }
        public bool IncludeInactive { get; set; }

        public GetPurchaseOrdersBySupplierQuery(int supplierId, bool includeInactive = false)
        {
            SupplierId = supplierId;
            IncludeInactive = includeInactive;
        }
    }

    /// <summary>
    /// Query para obtener órdenes pendientes de recibir
    /// </summary>
    public class GetPendingToReceiveOrdersQuery : IRequest<PurchaseOrdersListResponseDto>
    {
        public GetPendingToReceiveOrdersQuery()
        {
        }
    }

    /// <summary>
    /// Query para obtener órdenes pendientes de aprobar
    /// </summary>
    public class GetPendingApprovalOrdersQuery : IRequest<PurchaseOrdersListResponseDto>
    {
        public GetPendingApprovalOrdersQuery()
        {
        }
    }
}
