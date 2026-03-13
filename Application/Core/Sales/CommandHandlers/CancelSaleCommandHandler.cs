using Application.Abstractions.Sales;
using Application.Core.Sales.Commands;
using Application.DTOs.Sales;
using MediatR;

namespace Application.Core.Sales.CommandHandlers
{
    /// <summary>
    /// Handler para cancelar una venta
    /// </summary>
    public class CancelSaleCommandHandler : IRequestHandler<CancelSaleCommand, SaleResponseDto>
    {
        private readonly ISaleRepository _saleRepository;

        public CancelSaleCommandHandler(ISaleRepository saleRepository)
        {
            _saleRepository = saleRepository;
        }

        public async Task<SaleResponseDto> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
        {
            // 1. Obtener venta
            var sale = await _saleRepository.GetByIdAsync(request.SaleId);
            if (sale == null)
            {
                throw new KeyNotFoundException($"Venta con ID {request.SaleId} no encontrada");
            }

            // 2. Validar estado
            if (sale.Status == "Cancelled")
            {
                throw new InvalidOperationException("La venta ya está cancelada");
            }

            if (sale.Status == "Completed" && sale.IsPostedToInventory)
            {
                throw new InvalidOperationException(
                    "No se puede cancelar una venta que ya fue aplicada al inventario. " +
                    "Debe crear una devolución en su lugar.");
            }

            // 3. Cancelar venta
            sale.Status = "Cancelled";
            sale.CancelledAt = DateTime.UtcNow;
            sale.CancelledByUserId = request.UserId;
            sale.CancellationReason = request.Reason;

            await _saleRepository.UpdateAsync(sale);

            Console.WriteLine($"? Venta {sale.Code} cancelada. Razón: {request.Reason}");

            // 4. Mapear respuesta (reutilizar CreateSaleCommandHandler)
            var saleWithRelations = await _saleRepository.GetByIdAsync(sale.Id);

            return new SaleResponseDto
            {
                Id = saleWithRelations!.Id,
                Code = saleWithRelations.Code,
                SaleDate = saleWithRelations.SaleDate,
                CustomerName = saleWithRelations.CustomerName,
                WarehouseName = saleWithRelations.Warehouse.Name,
                UserName = saleWithRelations.User.Name,
                Total = saleWithRelations.Total,
                Status = saleWithRelations.Status,
                CancelledAt = saleWithRelations.CancelledAt,
                CancellationReason = saleWithRelations.CancellationReason,
                Details = saleWithRelations.Details.Select(d => new SaleDetailResponseDto
                {
                    ProductName = d.ProductName,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    Total = d.Total
                }).ToList()
            };
        }
    }
}
