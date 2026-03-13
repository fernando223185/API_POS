using Application.DTOs.PurchaseOrder;
using MediatR;

namespace Application.Core.PurchaseOrder.Commands
{
    /// <summary>
    /// Comando para crear orden de compra
    /// </summary>
    public class CreatePurchaseOrderCommand : IRequest<PurchaseOrderResponseDto>
    {
        public CreatePurchaseOrderDto OrderData { get; set; }
        public int CreatedByUserId { get; set; }

        public CreatePurchaseOrderCommand(CreatePurchaseOrderDto orderData, int createdByUserId)
        {
            OrderData = orderData;
            CreatedByUserId = createdByUserId;
        }
    }

    /// <summary>
    /// Comando para actualizar orden de compra
    /// </summary>
    public class UpdatePurchaseOrderCommand : IRequest<PurchaseOrderResponseDto>
    {
        public int PurchaseOrderId { get; set; }
        public UpdatePurchaseOrderDto OrderData { get; set; }
        public int UpdatedByUserId { get; set; }

        public UpdatePurchaseOrderCommand(int purchaseOrderId, UpdatePurchaseOrderDto orderData, int updatedByUserId)
        {
            PurchaseOrderId = purchaseOrderId;
            OrderData = orderData;
            UpdatedByUserId = updatedByUserId;
        }
    }

    /// <summary>
    /// Comando para aprobar orden de compra
    /// </summary>
    public class ApprovePurchaseOrderCommand : IRequest<bool>
    {
        public int PurchaseOrderId { get; set; }
        public int UpdatedByUserId { get; set; }
        public string? Notes { get; set; }

        public ApprovePurchaseOrderCommand(int purchaseOrderId, int updatedByUserId, string? notes = null)
        {
            PurchaseOrderId = purchaseOrderId;
            UpdatedByUserId = updatedByUserId;
            Notes = notes;
        }
    }

    /// <summary>
    /// Comando para cancelar orden de compra
    /// </summary>
    public class CancelPurchaseOrderCommand : IRequest<bool>
    {
        public int PurchaseOrderId { get; set; }
        public int UpdatedByUserId { get; set; }
        public string? CancellationReason { get; set; }

        public CancelPurchaseOrderCommand(int purchaseOrderId, int updatedByUserId, string? cancellationReason = null)
        {
            PurchaseOrderId = purchaseOrderId;
            UpdatedByUserId = updatedByUserId;
            CancellationReason = cancellationReason;
        }
    }

    /// <summary>
    /// Comando para marcar como en tránsito
    /// </summary>
    public class MarkAsInTransitCommand : IRequest<bool>
    {
        public int PurchaseOrderId { get; set; }
        public int UpdatedByUserId { get; set; }
        public string? TrackingInfo { get; set; }

        public MarkAsInTransitCommand(int purchaseOrderId, int updatedByUserId, string? trackingInfo = null)
        {
            PurchaseOrderId = purchaseOrderId;
            UpdatedByUserId = updatedByUserId;
            TrackingInfo = trackingInfo;
        }
    }
}
