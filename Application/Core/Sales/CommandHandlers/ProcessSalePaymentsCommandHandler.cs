using Application.Abstractions.Common;
using Application.Abstractions.Inventory;
using Application.Abstractions.Sales;
using Application.Core.Sales.Commands;
using Application.DTOs.Sales;
using Domain.Entities;
using MediatR;

namespace Application.Core.Sales.CommandHandlers
{
    /// <summary>
    /// Handler para procesar pagos y completar la venta
    /// Descuenta inventario automáticamente
    /// </summary>
    public class ProcessSalePaymentsCommandHandler : IRequestHandler<ProcessSalePaymentsCommand, ProcessSalePaymentsResponseDto>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IInventoryService _inventoryService;
        private readonly ICodeGeneratorService _codeGenerator;

        public ProcessSalePaymentsCommandHandler(
            ISaleRepository saleRepository,
            IInventoryService inventoryService,
            ICodeGeneratorService codeGenerator)
        {
            _saleRepository = saleRepository;
            _inventoryService = inventoryService;
            _codeGenerator = codeGenerator;
        }

        public async Task<ProcessSalePaymentsResponseDto> Handle(
            ProcessSalePaymentsCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Obtener venta
            var sale = await _saleRepository.GetByIdAsync(request.SaleId);
            if (sale == null)
            {
                throw new KeyNotFoundException($"Venta con ID {request.SaleId} no encontrada");
            }

            // 2. Validar estado
            if (sale.Status != "Draft")
            {
                throw new InvalidOperationException(
                    $"Solo se pueden procesar pagos de ventas en estado Draft. Estado actual: {sale.Status}");
            }

            // 3. Validar que no esté ya aplicada a inventario
            if (sale.IsPostedToInventory)
            {
                throw new InvalidOperationException("Esta venta ya fue aplicada al inventario");
            }

            // 4. Validar que haya detalles
            if (!sale.Details.Any())
            {
                throw new InvalidOperationException("La venta no tiene productos");
            }

            // 5. Validar stock disponible ANTES de procesar pagos
            Console.WriteLine($"?? Validando stock para venta {sale.Code}...");
            
            foreach (var detail in sale.Details)
            {
                var stock = await _inventoryService.GetStockAsync(detail.ProductId, sale.WarehouseId);

                if (stock == null || stock.Quantity < detail.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Stock insuficiente para '{detail.ProductName}'. " +
                        $"Disponible: {stock?.Quantity ?? 0}, Solicitado: {detail.Quantity}");
                }
            }

            Console.WriteLine($"? Stock validado correctamente");

            // 6. Validar que haya pagos
            if (!request.PaymentsData.Payments.Any())
            {
                throw new InvalidOperationException("Debe agregar al menos una forma de pago");
            }

            // 7. Procesar pagos
            decimal totalPaid = 0;
            var payments = new List<SalePayment>();

            foreach (var paymentDto in request.PaymentsData.Payments)
            {
                var payment = new SalePayment
                {
                    SaleId = sale.Id,
                    PaymentMethod = paymentDto.PaymentMethod,
                    Amount = paymentDto.Amount,
                    PaymentDate = DateTime.UtcNow,

                    // Datos de tarjeta/terminal
                    CardNumber = paymentDto.CardNumber,
                    CardType = paymentDto.CardType,
                    AuthorizationCode = paymentDto.AuthorizationCode,
                    TransactionReference = paymentDto.TransactionReference,
                    TerminalId = paymentDto.TerminalId,
                    BankName = paymentDto.BankName,

                    // Transferencias/Cheques
                    TransferReference = paymentDto.TransferReference,
                    CheckNumber = paymentDto.CheckNumber,
                    CheckBank = paymentDto.CheckBank,

                    Status = "Completed",
                    Notes = paymentDto.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                sale.Payments.Add(payment);
                payments.Add(payment);
                totalPaid += payment.Amount;
            }

            Console.WriteLine($"?? Total pagado: {totalPaid:C2}, Total venta: {sale.Total:C2}");

            // 8. Validar que el total pagado cubra el total de la venta
            if (totalPaid < sale.Total)
            {
                throw new InvalidOperationException(
                    $"El total pagado ({totalPaid:C2}) es menor al total de la venta ({sale.Total:C2})");
            }

            // 9. Actualizar montos de la venta
            sale.AmountPaid = totalPaid;
            sale.ChangeAmount = totalPaid - sale.Total;
            sale.IsPaid = true;
            sale.Status = "Completed";

            Console.WriteLine($"?? Cambio a devolver: {sale.ChangeAmount:C2}");

            // 10. Descontar inventario (CRÍTICO)
            Console.WriteLine($"?? Descontando inventario...");
            
            var movements = new List<InventoryMovementSummaryDto>();

            foreach (var detail in sale.Details)
            {
                // Generar código de movimiento
                var movementCode = await _codeGenerator.GenerateNextCodeAsync("MOV", "InventoryMovements");

                // Obtener stock actual
                var currentStock = await _inventoryService.GetStockAsync(detail.ProductId, sale.WarehouseId);

                if (currentStock == null)
                {
                    throw new InvalidOperationException(
                        $"No se encontró registro de stock para '{detail.ProductName}' en el almacén");
                }

                decimal stockBefore = currentStock.Quantity;
                decimal stockAfter = stockBefore - detail.Quantity;

                // Crear movimiento de salida (OUT)
                var movement = new InventoryMovement
                {
                    Code = movementCode,
                    ProductId = detail.ProductId,
                    WarehouseId = sale.WarehouseId,
                    MovementType = "OUT",
                    MovementSubType = "SALE",
                    Quantity = -detail.Quantity, // ? NEGATIVO para salida
                    StockBefore = stockBefore,
                    StockAfter = stockAfter,
                    UnitCost = detail.UnitCost,
                    TotalCost = detail.TotalCost,
                    ReferenceDocumentType = "Sale",
                    ReferenceDocumentId = sale.Id,
                    ReferenceDocumentCode = sale.Code,
                    SaleId = sale.Id,
                    MovementDate = DateTime.UtcNow,
                    Notes = $"Venta {sale.Code} - Cliente: {sale.CustomerName}",
                    CreatedByUserId = request.UserId,
                    CreatedAt = DateTime.UtcNow
                };

                await _inventoryService.CreateMovementAsync(movement);

                // Actualizar stock
                currentStock.Quantity -= detail.Quantity;
                currentStock.LastMovementDate = DateTime.UtcNow;
                await _inventoryService.UpdateStockAsync(currentStock);

                Console.WriteLine($"   ? {detail.ProductName}: {stockBefore} ? {stockAfter} ({movementCode})");

                // Agregar a resumen
                movements.Add(new InventoryMovementSummaryDto
                {
                    MovementCode = movementCode,
                    ProductCode = detail.ProductCode,
                    ProductName = detail.ProductName,
                    WarehouseCode = sale.Warehouse?.Code,
                    Quantity = -detail.Quantity,
                    StockBefore = stockBefore,
                    StockAfter = stockAfter
                });
            }

            // 11. Marcar venta como aplicada a inventario
            sale.IsPostedToInventory = true;
            sale.PostedToInventoryDate = DateTime.UtcNow;

            // 12. Actualizar venta
            await _saleRepository.UpdateAsync(sale);

            Console.WriteLine($"?? Venta {sale.Code} completada exitosamente");
            Console.WriteLine($"   ?? {movements.Count} movimientos de inventario creados");
            Console.WriteLine($"   ?? {payments.Count} formas de pago registradas");

            // 13. Retornar respuesta
            return new ProcessSalePaymentsResponseDto
            {
                Message = "Venta completada exitosamente",
                Error = 0,
                Data = new ProcessSalePaymentsDataDto
                {
                    SaleId = sale.Id,
                    SaleCode = sale.Code,
                    Status = sale.Status,
                    AmountPaid = sale.AmountPaid,
                    ChangeAmount = sale.ChangeAmount,
                    TotalPayments = payments.Count,
                    TotalMovements = movements.Count,
                    Payments = payments.Select(p => new SalePaymentResponseDto
                    {
                        Id = p.Id,
                        PaymentMethod = p.PaymentMethod,
                        Amount = p.Amount,
                        PaymentDate = p.PaymentDate,
                        CardNumber = p.CardNumber,
                        CardType = p.CardType,
                        AuthorizationCode = p.AuthorizationCode,
                        TransactionReference = p.TransactionReference,
                        TerminalId = p.TerminalId,
                        BankName = p.BankName,
                        TransferReference = p.TransferReference,
                        CheckNumber = p.CheckNumber,
                        CheckBank = p.CheckBank,
                        Status = p.Status,
                        Notes = p.Notes
                    }).ToList(),
                    InventoryMovements = movements
                }
            };
        }
    }
}
