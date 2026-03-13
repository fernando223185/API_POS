using Application.Abstractions.Purchasing;
using Application.Core.PurchaseOrder.Queries;
using Application.DTOs.PurchaseOrder;
using MediatR;

namespace Application.Core.PurchaseOrder.QueryHandlers
{
    /// <summary>
    /// Helper method para mapear entidad a DTO
    /// </summary>
    internal static class PurchaseOrderMapper
    {
        internal static PurchaseOrderResponseDto MapToResponseDto(Domain.Entities.PurchaseOrder order)
        {
            return new PurchaseOrderResponseDto
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
                    ProductCode = d.Product?.code ?? "N/A",
                    ProductName = d.Product?.name ?? "Producto no disponible",
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
        }
    }

    /// <summary>
    /// Handler para obtener todas las órdenes
    /// </summary>
    public class GetAllPurchaseOrdersQueryHandler : IRequestHandler<GetAllPurchaseOrdersQuery, PurchaseOrdersListResponseDto>
    {
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;

        public GetAllPurchaseOrdersQueryHandler(IPurchaseOrderRepository purchaseOrderRepository)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
        }

        public async Task<PurchaseOrdersListResponseDto> Handle(GetAllPurchaseOrdersQuery request, CancellationToken cancellationToken)
        {
            var orders = await _purchaseOrderRepository.GetAllAsync(request.IncludeInactive);
            var orderDtos = orders.Select(PurchaseOrderMapper.MapToResponseDto).ToList();

            var totalOrders = await _purchaseOrderRepository.GetTotalCountAsync();
            var pendingOrders = await _purchaseOrderRepository.GetCountByStatusAsync("Pending");
            var approvedOrders = await _purchaseOrderRepository.GetCountByStatusAsync("Approved");
            var receivedOrders = await _purchaseOrderRepository.GetCountByStatusAsync("Received");

            return new PurchaseOrdersListResponseDto
            {
                Message = "Órdenes de compra obtenidas exitosamente",
                Error = 0,
                PurchaseOrders = orderDtos,
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                ApprovedOrders = approvedOrders,
                ReceivedOrders = receivedOrders
            };
        }
    }

    /// <summary>
    /// Handler para obtener órdenes paginadas
    /// </summary>
    public class GetPurchaseOrdersPagedQueryHandler : IRequestHandler<GetPurchaseOrdersPagedQuery, PurchaseOrdersPagedResponseDto>
    {
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;

        public GetPurchaseOrdersPagedQueryHandler(IPurchaseOrderRepository purchaseOrderRepository)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
        }

        public async Task<PurchaseOrdersPagedResponseDto> Handle(GetPurchaseOrdersPagedQuery request, CancellationToken cancellationToken)
        {
            var (orders, totalRecords) = await _purchaseOrderRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                request.IncludeInactive,
                request.SearchTerm,
                request.Status,
                request.SupplierId,
                request.WarehouseId
            );

            var orderDtos = orders.Select(PurchaseOrderMapper.MapToResponseDto).ToList();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

            return new PurchaseOrdersPagedResponseDto
            {
                Message = "Órdenes de compra obtenidas exitosamente",
                Error = 0,
                Data = orderDtos,
                CurrentPage = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                TotalRecords = totalRecords,
                HasPreviousPage = request.PageNumber > 1,
                HasNextPage = request.PageNumber < totalPages
            };
        }
    }

    /// <summary>
    /// Handler para obtener orden por ID
    /// </summary>
    public class GetPurchaseOrderByIdQueryHandler : IRequestHandler<GetPurchaseOrderByIdQuery, PurchaseOrderResponseDto?>
    {
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;

        public GetPurchaseOrderByIdQueryHandler(IPurchaseOrderRepository purchaseOrderRepository)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
        }

        public async Task<PurchaseOrderResponseDto?> Handle(GetPurchaseOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await _purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderId);
            return order == null ? null : PurchaseOrderMapper.MapToResponseDto(order);
        }
    }

    /// <summary>
    /// Handler para obtener orden por código
    /// </summary>
    public class GetPurchaseOrderByCodeQueryHandler : IRequestHandler<GetPurchaseOrderByCodeQuery, PurchaseOrderResponseDto?>
    {
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;

        public GetPurchaseOrderByCodeQueryHandler(IPurchaseOrderRepository purchaseOrderRepository)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
        }

        public async Task<PurchaseOrderResponseDto?> Handle(GetPurchaseOrderByCodeQuery request, CancellationToken cancellationToken)
        {
            var order = await _purchaseOrderRepository.GetByCodeAsync(request.Code);
            return order == null ? null : PurchaseOrderMapper.MapToResponseDto(order);
        }
    }

    /// <summary>
    /// Handler para obtener órdenes pendientes de recibir
    /// </summary>
    public class GetPendingToReceiveOrdersQueryHandler : IRequestHandler<GetPendingToReceiveOrdersQuery, PurchaseOrdersListResponseDto>
    {
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;

        public GetPendingToReceiveOrdersQueryHandler(IPurchaseOrderRepository purchaseOrderRepository)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
        }

        public async Task<PurchaseOrdersListResponseDto> Handle(GetPendingToReceiveOrdersQuery request, CancellationToken cancellationToken)
        {
            var orders = await _purchaseOrderRepository.GetPendingToReceiveAsync();
            var orderDtos = orders.Select(PurchaseOrderMapper.MapToResponseDto).ToList();

            return new PurchaseOrdersListResponseDto
            {
                Message = "Órdenes pendientes de recibir obtenidas exitosamente",
                Error = 0,
                PurchaseOrders = orderDtos,
                TotalOrders = orderDtos.Count,
                PendingOrders = 0,
                ApprovedOrders = 0,
                ReceivedOrders = 0
            };
        }
    }
}
