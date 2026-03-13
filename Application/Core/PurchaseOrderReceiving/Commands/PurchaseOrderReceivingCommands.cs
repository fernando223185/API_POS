using Application.DTOs.PurchaseOrderReceiving;
using MediatR;

namespace Application.Core.PurchaseOrderReceiving.Commands
{
    /// <summary>
    /// Comando para crear recepción de mercancía
    /// </summary>
    public class CreatePurchaseOrderReceivingCommand : IRequest<PurchaseOrderReceivingResponseDto>
    {
        public CreatePurchaseOrderReceivingDto ReceivingData { get; set; }
        public int CreatedByUserId { get; set; }

        public CreatePurchaseOrderReceivingCommand(CreatePurchaseOrderReceivingDto receivingData, int createdByUserId)
        {
            ReceivingData = receivingData;
            CreatedByUserId = createdByUserId;
        }
    }

    /// <summary>
    /// Comando para aplicar recepción a inventario
    /// </summary>
    public class PostToInventoryCommand : IRequest<PostToInventoryResponseDto>
    {
        public int ReceivingId { get; set; }
        public int UserId { get; set; }

        public PostToInventoryCommand(int receivingId, int userId)
        {
            ReceivingId = receivingId;
            UserId = userId;
        }
    }
}
