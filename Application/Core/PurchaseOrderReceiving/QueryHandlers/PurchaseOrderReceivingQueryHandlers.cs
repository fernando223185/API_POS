using Application.Abstractions.Purchasing;
using Application.Core.PurchaseOrderReceiving.Queries;
using Application.DTOs.PurchaseOrderReceiving;
using MediatR;

namespace Application.Core.PurchaseOrderReceiving.QueryHandlers
{
    /// <summary>
    /// Helper para mapear entidad a DTO
    /// </summary>
    internal static class ReceivingMapper
    {
        internal static PurchaseOrderReceivingResponseDto MapToResponseDto(Domain.Entities.PurchaseOrderReceiving receiving)
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
                    ProductCode = d.Product?.code ?? "N/A",
                    ProductName = d.Product?.name ?? "Producto no disponible",
                    QuantityOrdered = d.PurchaseOrderDetail?.QuantityOrdered ?? 0,
                    QuantityReceived = d.QuantityReceived,
                    QuantityAccepted = d.QuantityApproved ?? 0,
                    QuantityRejected = d.QuantityRejected ?? 0,
                    QuantityPending = d.PurchaseOrderDetail?.QuantityPending ?? 0,
                    StorageLocation = d.StorageLocation,
                    LotNumber = d.LotNumber,
                    ExpirationDate = d.ExpiryDate,
                    Notes = d.Notes
                }).ToList(),
                TotalItems = receiving.Details.Count,
                TotalQuantityReceived = receiving.Details.Sum(d => d.QuantityReceived),
                TotalQuantityAccepted = receiving.Details.Sum(d => d.QuantityApproved ?? 0),
                TotalQuantityRejected = receiving.Details.Sum(d => d.QuantityRejected ?? 0)
            };
        }
    }

    /// <summary>
    /// Handler para obtener todas las recepciones
    /// </summary>
    public class GetAllReceivingsQueryHandler : IRequestHandler<GetAllReceivingsQuery, ReceivingsListResponseDto>
    {
        private readonly IPurchaseOrderReceivingRepository _receivingRepository;

        public GetAllReceivingsQueryHandler(IPurchaseOrderReceivingRepository receivingRepository)
        {
            _receivingRepository = receivingRepository;
        }

        public async Task<ReceivingsListResponseDto> Handle(GetAllReceivingsQuery request, CancellationToken cancellationToken)
        {
            var receivings = await _receivingRepository.GetAllAsync(request.IncludePosted);

            var receivingDtos = receivings.Select(r => ReceivingMapper.MapToResponseDto(r)).ToList();

            var totalReceivings = await _receivingRepository.GetTotalCountAsync();
            var pendingToPost = await _receivingRepository.GetPendingToPostCountAsync();

            return new ReceivingsListResponseDto
            {
                Message = "Recepciones obtenidas exitosamente",
                Error = 0,
                Receivings = receivingDtos,
                TotalReceivings = totalReceivings,
                PendingToPost = pendingToPost,
                Posted = totalReceivings - pendingToPost
            };
        }
    }

    /// <summary>
    /// Handler para obtener recepciones paginadas
    /// </summary>
    public class GetReceivingsPagedQueryHandler : IRequestHandler<GetReceivingsPagedQuery, ReceivingsPagedResponseDto>
    {
        private readonly IPurchaseOrderReceivingRepository _receivingRepository;

        public GetReceivingsPagedQueryHandler(IPurchaseOrderReceivingRepository receivingRepository)
        {
            _receivingRepository = receivingRepository;
        }

        public async Task<ReceivingsPagedResponseDto> Handle(GetReceivingsPagedQuery request, CancellationToken cancellationToken)
        {
            var (receivings, totalRecords) = await _receivingRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                request.PurchaseOrderId,
                request.WarehouseId,
                request.Status,
                request.OnlyPendingToPost
            );

            var receivingDtos = receivings.Select(r => ReceivingMapper.MapToResponseDto(r)).ToList();

            var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

            return new ReceivingsPagedResponseDto
            {
                Message = "Recepciones obtenidas exitosamente",
                Error = 0,
                Data = receivingDtos,
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
    /// Handler para obtener recepción por ID
    /// </summary>
    public class GetReceivingByIdQueryHandler : IRequestHandler<GetReceivingByIdQuery, PurchaseOrderReceivingResponseDto?>
    {
        private readonly IPurchaseOrderReceivingRepository _receivingRepository;

        public GetReceivingByIdQueryHandler(IPurchaseOrderReceivingRepository receivingRepository)
        {
            _receivingRepository = receivingRepository;
        }

        public async Task<PurchaseOrderReceivingResponseDto?> Handle(GetReceivingByIdQuery request, CancellationToken cancellationToken)
        {
            var receiving = await _receivingRepository.GetByIdAsync(request.ReceivingId);
            if (receiving == null)
            {
                return null;
            }

            return ReceivingMapper.MapToResponseDto(receiving);
        }
    }

    /// <summary>
    /// Handler para obtener recepciones de una OC
    /// </summary>
    public class GetReceivingsByPurchaseOrderQueryHandler : IRequestHandler<GetReceivingsByPurchaseOrderQuery, ReceivingsListResponseDto>
    {
        private readonly IPurchaseOrderReceivingRepository _receivingRepository;

        public GetReceivingsByPurchaseOrderQueryHandler(IPurchaseOrderReceivingRepository receivingRepository)
        {
            _receivingRepository = receivingRepository;
        }

        public async Task<ReceivingsListResponseDto> Handle(GetReceivingsByPurchaseOrderQuery request, CancellationToken cancellationToken)
        {
            var receivings = await _receivingRepository.GetByPurchaseOrderAsync(request.PurchaseOrderId);

            var receivingDtos = receivings.Select(r => ReceivingMapper.MapToResponseDto(r)).ToList();

            return new ReceivingsListResponseDto
            {
                Message = "Recepciones obtenidas exitosamente",
                Error = 0,
                Receivings = receivingDtos,
                TotalReceivings = receivingDtos.Count,
                PendingToPost = receivingDtos.Count(r => !r.IsPostedToInventory),
                Posted = receivingDtos.Count(r => r.IsPostedToInventory)
            };
        }
    }

    /// <summary>
    /// Handler para obtener recepciones pendientes de aplicar
    /// </summary>
    public class GetPendingToPostQueryHandler : IRequestHandler<GetPendingToPostQuery, ReceivingsListResponseDto>
    {
        private readonly IPurchaseOrderReceivingRepository _receivingRepository;

        public GetPendingToPostQueryHandler(IPurchaseOrderReceivingRepository receivingRepository)
        {
            _receivingRepository = receivingRepository;
        }

        public async Task<ReceivingsListResponseDto> Handle(GetPendingToPostQuery request, CancellationToken cancellationToken)
        {
            var receivings = await _receivingRepository.GetPendingToPostAsync();

            var receivingDtos = receivings.Select(r => ReceivingMapper.MapToResponseDto(r)).ToList();

            return new ReceivingsListResponseDto
            {
                Message = "Recepciones pendientes obtenidas exitosamente",
                Error = 0,
                Receivings = receivingDtos,
                TotalReceivings = receivingDtos.Count,
                PendingToPost = receivingDtos.Count,
                Posted = 0
            };
        }
    }
}
