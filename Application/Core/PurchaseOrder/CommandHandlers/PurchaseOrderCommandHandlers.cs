using Application.Abstractions.Common;
using Application.Abstractions.Purchasing;
using Application.Core.PurchaseOrder.Commands;
using Application.DTOs.PurchaseOrder;
using MediatR;

namespace Application.Core.PurchaseOrder.CommandHandlers
{
    /// <summary>
    /// Handler para crear orden de compra
    /// </summary>
    public class CreatePurchaseOrderCommandHandler : IRequestHandler<CreatePurchaseOrderCommand, PurchaseOrderResponseDto>
    {
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly ICodeGeneratorService _codeGenerator;

        public CreatePurchaseOrderCommandHandler(
            IPurchaseOrderRepository purchaseOrderRepository,
            ICodeGeneratorService codeGenerator)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
            _codeGenerator = codeGenerator;
        }

        public async Task<PurchaseOrderResponseDto> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
        {
            // Validar proveedor
            if (!await _purchaseOrderRepository.SupplierExistsAsync(request.OrderData.SupplierId))
            {
                throw new KeyNotFoundException($"Proveedor con ID {request.OrderData.SupplierId} no encontrado o inactivo");
            }

            // Validar almacén
            if (!await _purchaseOrderRepository.WarehouseExistsAsync(request.OrderData.WarehouseId))
            {
                throw new KeyNotFoundException($"Almacén con ID {request.OrderData.WarehouseId} no encontrado, inactivo o no permite recepción");
            }

            // Validar productos
            foreach (var detail in request.OrderData.Details)
            {
                if (!await _purchaseOrderRepository.ProductExistsAsync(detail.ProductId))
                {
                    throw new KeyNotFoundException($"Producto con ID {detail.ProductId} no encontrado o inactivo");
                }

                if (detail.QuantityOrdered <= 0)
                {
                    throw new ArgumentException($"La cantidad debe ser mayor a cero");
                }

                if (detail.UnitPrice < 0)
                {
                    throw new ArgumentException($"El precio unitario no puede ser negativo");
                }
            }

            // Generar código automático
            var code = await _codeGenerator.GenerateNextCodeAsync("OC", "PurchaseOrders");

            // Crear orden de compra
            var purchaseOrder = new Domain.Entities.PurchaseOrder
            {
                Code = code,
                SupplierId = request.OrderData.SupplierId,
                WarehouseId = request.OrderData.WarehouseId,
                OrderDate = request.OrderData.OrderDate,
                ExpectedDeliveryDate = request.OrderData.ExpectedDeliveryDate,
                Status = "Pending",
                Notes = request.OrderData.Notes,
                SupplierReference = request.OrderData.SupplierReference,
                PaymentTerms = request.OrderData.PaymentTerms,
                DeliveryTerms = request.OrderData.DeliveryTerms,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = request.CreatedByUserId
            };

            // Agregar detalles y calcular totales
            decimal subTotal = 0;
            decimal tax = 0;

            foreach (var detailDto in request.OrderData.Details)
            {
                var lineSubTotal = detailDto.QuantityOrdered * detailDto.UnitPrice - detailDto.Discount;
                var lineTax = lineSubTotal * (detailDto.TaxPercentage / 100);
                var lineTotal = lineSubTotal + lineTax;

                var detail = new Domain.Entities.PurchaseOrderDetail
                {
                    ProductId = detailDto.ProductId,
                    QuantityOrdered = detailDto.QuantityOrdered,
                    QuantityReceived = 0,
                    QuantityPending = detailDto.QuantityOrdered,
                    UnitPrice = detailDto.UnitPrice,
                    Discount = detailDto.Discount,
                    TaxPercentage = detailDto.TaxPercentage,
                    SubTotal = lineSubTotal,
                    Total = lineTotal,
                    Notes = detailDto.Notes
                };

                purchaseOrder.Details.Add(detail);
                subTotal += lineSubTotal;
                tax += lineTax;
            }

            purchaseOrder.SubTotal = subTotal;
            purchaseOrder.Tax = tax;
            purchaseOrder.Total = subTotal + tax;

            // Guardar
            var createdOrder = await _purchaseOrderRepository.CreateAsync(purchaseOrder);

            // Retornar DTO
            return MapToResponseDto(createdOrder);
        }

        private PurchaseOrderResponseDto MapToResponseDto(Domain.Entities.PurchaseOrder order)
        {
            var response = new PurchaseOrderResponseDto
            {
                Id = order.Id,
                Code = order.Code,
                SupplierId = order.SupplierId,
                SupplierCode = order.Supplier.Code,
                SupplierName = order.Supplier.Name,
                WarehouseId = order.WarehouseId,
                WarehouseCode = order.Warehouse.Code,
                WarehouseName = order.Warehouse.Name,
                OrderDate = order.OrderDate,
                ExpectedDeliveryDate = order.ExpectedDeliveryDate,
                Status = order.Status,
                SubTotal = order.SubTotal,
                Tax = order.Tax,
                Total = order.Total,
                Notes = order.Notes,
                SupplierReference = order.SupplierReference,
                PaymentTerms = order.PaymentTerms,
                DeliveryTerms = order.DeliveryTerms,
                IsActive = order.IsActive,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                CreatedByUserName = order.CreatedBy?.Name,
                UpdatedByUserName = order.UpdatedBy?.Name,
                Details = order.Details.Select(d => new PurchaseOrderDetailResponseDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductCode = d.Product.code,  // ? CORREGIDO
                    ProductName = d.Product.name,  // ? CORREGIDO
                    QuantityOrdered = d.QuantityOrdered,
                    QuantityReceived = d.QuantityReceived,
                    QuantityPending = d.QuantityPending,
                    UnitPrice = d.UnitPrice,
                    Discount = d.Discount,
                    TaxPercentage = d.TaxPercentage,
                    SubTotal = d.SubTotal,
                    TaxAmount = d.SubTotal * (d.TaxPercentage / 100),
                    Total = d.Total,
                    Notes = d.Notes
                }).ToList(),
                TotalItems = order.Details.Count,
                TotalQuantityOrdered = order.Details.Sum(d => d.QuantityOrdered),
                TotalQuantityReceived = order.Details.Sum(d => d.QuantityReceived),
                CompletionPercentage = order.Details.Sum(d => d.QuantityOrdered) > 0
                    ? (order.Details.Sum(d => d.QuantityReceived) / order.Details.Sum(d => d.QuantityOrdered)) * 100
                    : 0
            };

            return response;
        }
    }

    /// <summary>
    /// Handler para aprobar orden de compra
    /// </summary>
    public class ApprovePurchaseOrderCommandHandler : IRequestHandler<ApprovePurchaseOrderCommand, bool>
    {
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;

        public ApprovePurchaseOrderCommandHandler(IPurchaseOrderRepository purchaseOrderRepository)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
        }

        public async Task<bool> Handle(ApprovePurchaseOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderId);
            if (order == null)
            {
                return false;
            }

            if (order.Status != "Pending")
            {
                throw new InvalidOperationException($"Solo se pueden aprobar órdenes en estado Pending. Estado actual: {order.Status}");
            }

            order.Status = "Approved";
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedByUserId = request.UpdatedByUserId;

            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                order.Notes = string.IsNullOrWhiteSpace(order.Notes)
                    ? $"Aprobación: {request.Notes}"
                    : $"{order.Notes}\nAprobación: {request.Notes}";
            }

            await _purchaseOrderRepository.UpdateAsync(order);
            return true;
        }
    }

    /// <summary>
    /// Handler para cancelar orden de compra
    /// </summary>
    public class CancelPurchaseOrderCommandHandler : IRequestHandler<CancelPurchaseOrderCommand, bool>
    {
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;

        public CancelPurchaseOrderCommandHandler(IPurchaseOrderRepository purchaseOrderRepository)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
        }

        public async Task<bool> Handle(CancelPurchaseOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderId);
            if (order == null)
            {
                return false;
            }

            if (order.Status == "Received" || order.Status == "PartiallyReceived")
            {
                throw new InvalidOperationException($"No se puede cancelar una orden que ya tiene recepciones");
            }

            order.Status = "Cancelled";
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedByUserId = request.UpdatedByUserId;

            if (!string.IsNullOrWhiteSpace(request.CancellationReason))
            {
                order.Notes = string.IsNullOrWhiteSpace(order.Notes)
                    ? $"Cancelación: {request.CancellationReason}"
                    : $"{order.Notes}\nCancelación: {request.CancellationReason}";
            }

            await _purchaseOrderRepository.UpdateAsync(order);
            return true;
        }
    }

    /// <summary>
    /// Handler para marcar como en tránsito
    /// </summary>
    public class MarkAsInTransitCommandHandler : IRequestHandler<MarkAsInTransitCommand, bool>
    {
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;

        public MarkAsInTransitCommandHandler(IPurchaseOrderRepository purchaseOrderRepository)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
        }

        public async Task<bool> Handle(MarkAsInTransitCommand request, CancellationToken cancellationToken)
        {
            var order = await _purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderId);
            if (order == null)
            {
                return false;
            }

            if (order.Status != "Approved")
            {
                throw new InvalidOperationException($"Solo se pueden marcar como en tránsito órdenes aprobadas. Estado actual: {order.Status}");
            }

            order.Status = "InTransit";
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedByUserId = request.UpdatedByUserId;

            if (!string.IsNullOrWhiteSpace(request.TrackingInfo))
            {
                order.Notes = string.IsNullOrWhiteSpace(order.Notes)
                    ? $"En tránsito: {request.TrackingInfo}"
                    : $"{order.Notes}\nEn tránsito: {request.TrackingInfo}";
            }

            await _purchaseOrderRepository.UpdateAsync(order);
            return true;
        }
    }
}
