using Application.DTOs.Inventory;
using MediatR;

namespace Application.Core.Inventory.Commands
{
    /// <summary>
    /// Comando para crear un nuevo ajuste de inventario
    /// </summary>
    public record CreateStockAdjustmentCommand(CreateStockAdjustmentDto Dto, int UserId, int? CompanyId) 
        : IRequest<StockAdjustmentResponseDto>;
}
