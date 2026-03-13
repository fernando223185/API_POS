using Application.Abstractions.Common;
using Application.Abstractions.Inventory;
using Application.Abstractions.Purchasing;
using Application.Core.PurchaseOrderReceiving.Commands;
using Application.DTOs.PurchaseOrderReceiving;
using Domain.Entities;
using MediatR;

namespace Application.Core.PurchaseOrderReceiving.CommandHandlers
{
    /// <summary>
    /// Handler para crear recepción de mercancía
    /// </summary>
    public class CreatePurchaseOrderReceivingCommandHandler : IRequestHandler<CreatePurchaseOrderReceivingCommand, PurchaseOrderReceivingResponseDto>
    {
        private readonly IPurchaseOrderReceivingRepository _receivingRepository;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly ICodeGeneratorService _codeGenerator;

        public CreatePurchaseOrderReceivingCommandHandler(
            IPurchaseOrderReceivingRepository receivingRepository,
            IPurchaseOrderRepository purchaseOrderRepository,
            ICodeGeneratorService codeGenerator)
        {
            _receivingRepository = receivingRepository;
            _purchaseOrderRepository = purchaseOrderRepository;
            _codeGenerator = codeGenerator;
        }

        public async Task<PurchaseOrderReceivingResponseDto> Handle(CreatePurchaseOrderReceivingCommand request, CancellationToken cancellationToken)
        {
            // 1. Validar que la OC existe y puede recibir
            var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(request.ReceivingData.PurchaseOrderId);
            if (purchaseOrder == null)
            {
                throw new KeyNotFoundException($"Orden de compra con ID {request.ReceivingData.PurchaseOrderId} no encontrada");
            }

            var validStatuses = new[] { "Approved", "InTransit", "PartiallyReceived" };
            if (!validStatuses.Contains(purchaseOrder.Status))
            {
                throw new InvalidOperationException($"La orden de compra debe estar en estado Approved, InTransit o PartiallyReceived. Estado actual: {purchaseOrder.Status}");
            }

            // 2. Validar cantidades de cada detalle
            foreach (var detailDto in request.ReceivingData.Details)
            {
                var poDetail = purchaseOrder.Details.FirstOrDefault(d => d.Id == detailDto.PurchaseOrderDetailId);
                if (poDetail == null)
                {
                    throw new KeyNotFoundException($"Detalle de OC con ID {detailDto.PurchaseOrderDetailId} no encontrado");
                }

                // Validar que no se reciba más de lo ordenado
                var totalReceived = poDetail.QuantityReceived + detailDto.QuantityReceived;
                if (totalReceived > poDetail.QuantityOrdered)
                {
                    throw new InvalidOperationException(
                        $"No se puede recibir más de lo ordenado para el producto {poDetail.Product.name}. " +
                        $"Ordenado: {poDetail.QuantityOrdered}, Ya recibido: {poDetail.QuantityReceived}, " +
                        $"Intentando recibir: {detailDto.QuantityReceived}");
                }

                // Validar que aceptado + rechazado = recibido
                if (detailDto.QuantityAccepted + detailDto.QuantityRejected != detailDto.QuantityReceived)
                {
                    throw new InvalidOperationException(
                        $"La suma de cantidad aceptada ({detailDto.QuantityAccepted}) " +
                        $"y rechazada ({detailDto.QuantityRejected}) debe ser igual a la cantidad recibida ({detailDto.QuantityReceived})");
                }
            }

            // 3. Generar código automático
            var code = await _codeGenerator.GenerateNextCodeAsync("REC", "PurchaseOrderReceivings");

            // 4. Crear recepción
            var receiving = new Domain.Entities.PurchaseOrderReceiving
            {
                Code = code,
                PurchaseOrderId = request.ReceivingData.PurchaseOrderId,
                WarehouseId = purchaseOrder.WarehouseId,
                ReceivingDate = request.ReceivingData.ReceivingDate,
                SupplierInvoiceNumber = request.ReceivingData.SupplierInvoiceNumber,
                CarrierName = request.ReceivingData.CarrierName,
                TrackingNumber = request.ReceivingData.TrackingNumber,
                Status = "Received",
                IsPostedToInventory = false,
                Notes = request.ReceivingData.Notes,
                CreatedByUserId = request.CreatedByUserId,
                CreatedAt = DateTime.UtcNow
            };

            // 5. Crear detalles de recepción
            foreach (var detailDto in request.ReceivingData.Details)
            {
                var detail = new PurchaseOrderReceivingDetail
                {
                    PurchaseOrderDetailId = detailDto.PurchaseOrderDetailId,
                    ProductId = detailDto.ProductId,
                    QuantityReceived = detailDto.QuantityReceived,
                    QuantityApproved = detailDto.QuantityAccepted,  // ? CORREGIDO
                    QuantityRejected = detailDto.QuantityRejected,
                    StorageLocation = detailDto.StorageLocation,
                    LotNumber = detailDto.LotNumber,
                    ExpiryDate = detailDto.ExpirationDate,  // ? CORREGIDO
                    Notes = detailDto.Notes
                };

                receiving.Details.Add(detail);
            }

            // 6. Guardar recepción
            var createdReceiving = await _receivingRepository.CreateAsync(receiving);

            // 7. Actualizar cantidades recibidas en la OC
            foreach (var detail in createdReceiving.Details)
            {
                var poDetail = purchaseOrder.Details.First(d => d.Id == detail.PurchaseOrderDetailId);
                poDetail.QuantityReceived += detail.QuantityReceived;
                poDetail.QuantityPending = poDetail.QuantityOrdered - poDetail.QuantityReceived;
            }

            // 8. Actualizar estado de la OC
            var allReceived = purchaseOrder.Details.All(d => d.QuantityPending == 0);
            var partiallyReceived = purchaseOrder.Details.Any(d => d.QuantityReceived > 0);

            if (allReceived)
            {
                purchaseOrder.Status = "Received";
            }
            else if (partiallyReceived)
            {
                purchaseOrder.Status = "PartiallyReceived";
            }

            await _purchaseOrderRepository.UpdateAsync(purchaseOrder);

            // 9. Mapear respuesta
            return MapToResponseDto(createdReceiving);
        }

        private PurchaseOrderReceivingResponseDto MapToResponseDto(Domain.Entities.PurchaseOrderReceiving receiving)
        {
            return new PurchaseOrderReceivingResponseDto
            {
                Id = receiving.Id,
                Code = receiving.Code,
                PurchaseOrderId = receiving.PurchaseOrderId,
                PurchaseOrderCode = receiving.PurchaseOrder.Code,
                SupplierName = receiving.PurchaseOrder.Supplier.Name,
                WarehouseName = receiving.Warehouse.Name,
                ReceivingDate = receiving.ReceivingDate,
                SupplierInvoiceNumber = receiving.SupplierInvoiceNumber,
                CarrierName = receiving.CarrierName,
                TrackingNumber = receiving.TrackingNumber,
                Status = receiving.Status,
                IsPostedToInventory = receiving.IsPostedToInventory,
                PostedToInventoryDate = receiving.PostedToInventoryDate,
                Notes = receiving.Notes,
                CreatedAt = receiving.CreatedAt,
                CreatedByUserName = receiving.CreatedBy?.Name,
                Details = receiving.Details.Select(d => new PurchaseOrderReceivingDetailResponseDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductCode = d.Product.code,
                    ProductName = d.Product.name,
                    QuantityOrdered = d.PurchaseOrderDetail.QuantityOrdered,
                    QuantityReceived = d.QuantityReceived,
                    QuantityAccepted = d.QuantityApproved ?? 0,  // ? CORREGIDO
                    QuantityRejected = d.QuantityRejected ?? 0,  // ? CORREGIDO
                    QuantityPending = d.PurchaseOrderDetail.QuantityPending,
                    StorageLocation = d.StorageLocation,
                    LotNumber = d.LotNumber,
                    ExpirationDate = d.ExpiryDate,  // ? CORREGIDO
                    Notes = d.Notes
                }).ToList(),
                TotalItems = receiving.Details.Count,
                TotalQuantityReceived = receiving.Details.Sum(d => d.QuantityReceived),
                TotalQuantityAccepted = receiving.Details.Sum(d => d.QuantityApproved ?? 0),  // ? CORREGIDO
                TotalQuantityRejected = receiving.Details.Sum(d => d.QuantityRejected ?? 0)  // ? CORREGIDO
            };
        }
    }

    /// <summary>
    /// Handler para aplicar recepción a inventario ? CRÍTICO
    /// </summary>
    public class PostToInventoryCommandHandler : IRequestHandler<PostToInventoryCommand, PostToInventoryResponseDto>
    {
        private readonly IPurchaseOrderReceivingRepository _receivingRepository;
        private readonly ICodeGeneratorService _codeGenerator;
        private readonly IInventoryService _inventoryService;

        public PostToInventoryCommandHandler(
            IPurchaseOrderReceivingRepository receivingRepository,
            ICodeGeneratorService codeGenerator,
            IInventoryService inventoryService)
        {
            _receivingRepository = receivingRepository;
            _codeGenerator = codeGenerator;
            _inventoryService = inventoryService;
        }

        public async Task<PostToInventoryResponseDto> Handle(PostToInventoryCommand request, CancellationToken cancellationToken)
        {
            // 1. Obtener recepción
            var receiving = await _receivingRepository.GetByIdAsync(request.ReceivingId);
            if (receiving == null)
            {
                throw new KeyNotFoundException($"Recepción con ID {request.ReceivingId} no encontrada");
            }

            // 2. Validar que no esté aplicada
            if (receiving.IsPostedToInventory)
            {
                throw new InvalidOperationException("Esta recepción ya fue aplicada al inventario");
            }

            // 3. Validar estado
            if (receiving.Status != "Received" && receiving.Status != "QualityCheck")
            {
                throw new InvalidOperationException($"Solo se pueden aplicar recepciones con estado Received o QualityCheck. Estado actual: {receiving.Status}");
            }

            var movements = new List<InventoryMovementSummaryDto>();

            // 4. Por cada producto APROBADO
            foreach (var detail in receiving.Details.Where(d => (d.QuantityApproved ?? 0) > 0))
            {
                // 4.1 Generar código de movimiento
                var movementCode = await _codeGenerator.GenerateNextCodeAsync("MOV", "InventoryMovements");

                // 4.2 Obtener stock actual
                var currentStock = await _inventoryService.GetStockAsync(detail.ProductId, receiving.WarehouseId);

                decimal stockBefore = currentStock?.Quantity ?? 0;
                decimal stockAfter = stockBefore + (detail.QuantityApproved ?? 0);

                // 4.3 Crear movimiento de inventario
                var movement = new InventoryMovement
                {
                    Code = movementCode,
                    ProductId = detail.ProductId,
                    WarehouseId = receiving.WarehouseId,
                    MovementType = "IN",
                    Quantity = detail.QuantityApproved ?? 0,
                    UnitCost = detail.PurchaseOrderDetail.UnitPrice,
                    TotalCost = (detail.QuantityApproved ?? 0) * detail.PurchaseOrderDetail.UnitPrice,
                    StockBefore = stockBefore,
                    StockAfter = stockAfter,
                    ReferenceDocumentType = "PurchaseOrderReceiving",
                    ReferenceDocumentId = receiving.Id,
                    ReferenceDocumentCode = receiving.Code,
                    PurchaseOrderReceivingId = receiving.Id,
                    MovementDate = DateTime.UtcNow,
                    Notes = $"Recepción de mercancía - OC: {receiving.PurchaseOrder.Code}",
                    CreatedByUserId = request.UserId,
                    CreatedAt = DateTime.UtcNow
                };

                await _inventoryService.CreateMovementAsync(movement);

                // 4.4 Actualizar o crear stock
                if (currentStock == null)
                {
                    currentStock = new ProductStock
                    {
                        ProductId = detail.ProductId,
                        WarehouseId = receiving.WarehouseId,
                        Quantity = detail.QuantityApproved ?? 0,
                        MinimumStock = 0,
                        MaximumStock = 0,
                        LastMovementDate = DateTime.UtcNow
                    };
                    await _inventoryService.CreateStockAsync(currentStock);
                }
                else
                {
                    currentStock.Quantity += detail.QuantityApproved ?? 0;
                    currentStock.LastMovementDate = DateTime.UtcNow;
                    await _inventoryService.UpdateStockAsync(currentStock);
                }

                // 4.5 Agregar a resumen
                movements.Add(new InventoryMovementSummaryDto
                {
                    MovementCode = movementCode,
                    ProductCode = detail.Product.code,
                    ProductName = detail.Product.name,
                    WarehouseCode = receiving.Warehouse.Code,
                    Quantity = detail.QuantityApproved ?? 0,
                    StockBefore = stockBefore,
                    StockAfter = stockAfter
                });
            }

            // 5. Marcar recepción como aplicada
            receiving.IsPostedToInventory = true;
            receiving.PostedToInventoryDate = DateTime.UtcNow;
            receiving.Status = "Applied";

            await _receivingRepository.UpdateAsync(receiving);

            Console.WriteLine($"? Recepción {receiving.Code} aplicada al inventario. {movements.Count} movimientos creados.");

            // 6. Retornar respuesta
            return new PostToInventoryResponseDto
            {
                Message = "Recepción aplicada a inventario exitosamente",
                Error = 0,
                Data = new PostToInventoryDataDto
                {
                    ReceivingId = receiving.Id,
                    ReceivingCode = receiving.Code,
                    TotalMovementsCreated = movements.Count,
                    Movements = movements
                }
            };
        }
    }
}
