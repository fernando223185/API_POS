using Application.DTOs.PurchaseOrderReceiving;
using MediatR;

namespace Application.Core.PurchaseOrderReceiving.Queries
{
    /// <summary>
    /// Query para obtener todas las recepciones
    /// </summary>
    public class GetAllReceivingsQuery : IRequest<ReceivingsListResponseDto>
    {
        public bool IncludePosted { get; set; }

        public GetAllReceivingsQuery(bool includePosted = true)
        {
            IncludePosted = includePosted;
        }
    }

    /// <summary>
    /// Query para obtener recepciones paginadas
    /// </summary>
    public class GetReceivingsPagedQuery : IRequest<ReceivingsPagedResponseDto>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public int? PurchaseOrderId { get; set; }
        public int? WarehouseId { get; set; }
        public string? Status { get; set; }
        public bool? OnlyPendingToPost { get; set; }

        public GetReceivingsPagedQuery(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            int? purchaseOrderId = null,
            int? warehouseId = null,
            string? status = null,
            bool? onlyPendingToPost = null)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            SearchTerm = searchTerm;
            PurchaseOrderId = purchaseOrderId;
            WarehouseId = warehouseId;
            Status = status;
            OnlyPendingToPost = onlyPendingToPost;
        }
    }

    /// <summary>
    /// Query para obtener recepción por ID
    /// </summary>
    public class GetReceivingByIdQuery : IRequest<PurchaseOrderReceivingResponseDto?>
    {
        public int ReceivingId { get; set; }

        public GetReceivingByIdQuery(int receivingId)
        {
            ReceivingId = receivingId;
        }
    }

    /// <summary>
    /// Query para obtener recepciones de una OC
    /// </summary>
    public class GetReceivingsByPurchaseOrderQuery : IRequest<ReceivingsListResponseDto>
    {
        public int PurchaseOrderId { get; set; }

        public GetReceivingsByPurchaseOrderQuery(int purchaseOrderId)
        {
            PurchaseOrderId = purchaseOrderId;
        }
    }

    /// <summary>
    /// Query para obtener recepciones pendientes de aplicar
    /// </summary>
    public class GetPendingToPostQuery : IRequest<ReceivingsListResponseDto>
    {
    }
}
