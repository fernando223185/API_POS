using Application.Abstractions.Inventory;
using Application.Abstractions.Sales;
using Application.Abstractions.Common;
using Application.Core.Sales.Commands;
using Application.DTOs.Sales;
using Domain.Entities;
using MediatR;

namespace Application.Core.Sales.CommandHandlers
{
    /// <summary>
    /// Confirma la entrega de una venta Delivery:
    /// - Registra DeliveredAt
    /// - Procesa los pagos recibidos contra entrega
    /// - Descuenta inventario
    /// - Cierra la venta como Completed
    /// </summary>
    public class ConfirmDeliveryCommandHandler : IRequestHandler<ConfirmDeliveryCommand, SaleResponseDto>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IInventoryService _inventoryService;
        private readonly ICodeGeneratorService _codeGenerator;

        public ConfirmDeliveryCommandHandler(
            ISaleRepository saleRepository,
            IInventoryService inventoryService,
            ICodeGeneratorService codeGenerator)
        {
            _saleRepository = saleRepository;
            _inventoryService = inventoryService;
            _codeGenerator = codeGenerator;
        }

        public async Task<SaleResponseDto> Handle(ConfirmDeliveryCommand request, CancellationToken cancellationToken)
        {
            var sale = await _saleRepository.GetByIdAsync(request.SaleId)
                ?? throw new KeyNotFoundException($"Venta con ID {request.SaleId} no encontrada");

            if (sale.SaleType != "Delivery")
                throw new InvalidOperationException("Solo se puede confirmar la entrega de ventas de tipo Delivery");

            if (sale.Status != "Draft")
                throw new InvalidOperationException(
                    $"Solo se pueden confirmar ventas en estado Draft. Estado actual: {sale.Status}");

            if (sale.IsPostedToInventory)
                throw new InvalidOperationException("Esta venta ya fue aplicada al inventario");

            if (!sale.Details.Any())
                throw new InvalidOperationException("La venta no tiene productos");

            if (!request.DeliveryData.Payments.Any())
                throw new InvalidOperationException("Debe agregar al menos una forma de pago");

            // Validar stock antes de procesar
            foreach (var detail in sale.Details)
            {
                var stock = await _inventoryService.GetStockAsync(detail.ProductId, sale.WarehouseId);
                if (stock == null || stock.Quantity < detail.Quantity)
                    throw new InvalidOperationException(
                        $"Stock insuficiente para '{detail.ProductName}'. " +
                        $"Disponible: {stock?.Quantity ?? 0}, Solicitado: {detail.Quantity}");
            }

            // Procesar pagos
            decimal totalPaid = 0;
            foreach (var paymentDto in request.DeliveryData.Payments)
            {
                sale.Payments.Add(new SalePayment
                {
                    SaleId = sale.Id,
                    PaymentMethod = paymentDto.PaymentMethod,
                    Amount = paymentDto.Amount,
                    PaymentDate = DateTime.UtcNow,
                    CardNumber = paymentDto.CardNumber,
                    CardType = paymentDto.CardType,
                    AuthorizationCode = paymentDto.AuthorizationCode,
                    TransactionReference = paymentDto.TransactionReference,
                    TransferReference = paymentDto.TransferReference,
                    CheckNumber = paymentDto.CheckNumber,
                    CheckBank = paymentDto.CheckBank,
                    Status = "Completed",
                    Notes = paymentDto.Notes,
                    CreatedAt = DateTime.UtcNow
                });
                totalPaid += paymentDto.Amount;
            }

            if (totalPaid < sale.Total)
                throw new InvalidOperationException(
                    $"El total pagado ({totalPaid:C2}) es menor al total de la venta ({sale.Total:C2})");

            // Actualizar datos de entrega y pago
            sale.DeliveredAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(request.DeliveryData.DeliveryAddress))
                sale.DeliveryAddress = request.DeliveryData.DeliveryAddress;

            if (!string.IsNullOrWhiteSpace(request.DeliveryData.Notes))
                sale.Notes = request.DeliveryData.Notes;

            sale.AmountPaid = totalPaid;
            sale.ChangeAmount = totalPaid - sale.Total;
            sale.IsPaid = true;
            sale.Status = "Completed";
            sale.UpdatedAt = DateTime.UtcNow;

            // Descontar inventario
            foreach (var detail in sale.Details)
            {
                var movementCode = await _codeGenerator.GenerateNextCodeAsync("MOV", "InventoryMovements");
                var currentStock = await _inventoryService.GetStockAsync(detail.ProductId, sale.WarehouseId);

                if (currentStock == null)
                    throw new InvalidOperationException(
                        $"No se encontró registro de stock para '{detail.ProductName}' en el almacén");

                decimal stockBefore = currentStock.Quantity;
                decimal stockAfter = stockBefore - detail.Quantity;

                await _inventoryService.CreateMovementAsync(new InventoryMovement
                {
                    Code = movementCode,
                    ProductId = detail.ProductId,
                    WarehouseId = sale.WarehouseId,
                    MovementType = "OUT",
                    MovementSubType = "SALE",
                    Quantity = -detail.Quantity,
                    StockBefore = stockBefore,
                    StockAfter = stockAfter,
                    UnitCost = detail.UnitCost,
                    TotalCost = detail.TotalCost,
                    ReferenceDocumentType = "Sale",
                    ReferenceDocumentId = sale.Id,
                    ReferenceDocumentCode = sale.Code,
                    SaleId = sale.Id,
                    MovementDate = DateTime.UtcNow,
                    Notes = $"Delivery {sale.Code} - Cliente: {sale.CustomerName}",
                    CreatedByUserId = request.UserId,
                    CreatedAt = DateTime.UtcNow
                });

                currentStock.Quantity -= detail.Quantity;
                currentStock.LastMovementDate = DateTime.UtcNow;
                await _inventoryService.UpdateStockAsync(currentStock);
            }

            sale.IsPostedToInventory = true;
            sale.PostedToInventoryDate = DateTime.UtcNow;

            await _saleRepository.UpdateAsync(sale);

            var updated = await _saleRepository.GetByIdAsync(sale.Id)
                ?? throw new InvalidOperationException("Error al obtener la venta actualizada");

            return SaleMapper.ToResponseDto(updated);
        }
    }
}
