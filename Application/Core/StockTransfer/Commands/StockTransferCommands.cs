using Application.DTOs.Inventory;
using MediatR;

namespace Application.Core.StockTransfer.Commands
{
    public class CreateStockTransferCommand : IRequest<StockTransferResponseDto>
    {
        public CreateStockTransferDto TransferData { get; set; }
        public int CreatedByUserId { get; set; }

        public CreateStockTransferCommand(CreateStockTransferDto transferData, int createdByUserId)
        {
            TransferData = transferData;
            CreatedByUserId = createdByUserId;
        }
    }

    public class ApplyStockTransferCommand : IRequest<ApplyStockTransferResponseDto>
    {
        public int TransferId { get; set; }
        public int UserId { get; set; }

        public ApplyStockTransferCommand(int transferId, int userId)
        {
            TransferId = transferId;
            UserId = userId;
        }
    }

    public class UpdateStockTransferCommand : IRequest<StockTransferResponseDto>
    {
        public int TransferId { get; set; }
        public UpdateStockTransferDto TransferData { get; set; }
        public int UserId { get; set; }

        public UpdateStockTransferCommand(int transferId, UpdateStockTransferDto transferData, int userId)
        {
            TransferId = transferId;
            TransferData = transferData;
            UserId = userId;
        }
    }

    public class CancelStockTransferCommand : IRequest<bool>
    {
        public int TransferId { get; set; }
        public int UserId { get; set; }

        public CancelStockTransferCommand(int transferId, int userId)
        {
            TransferId = transferId;
            UserId = userId;
        }
    }
}
