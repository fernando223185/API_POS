using Application.Abstractions.Common;
using Application.Abstractions.Inventory;
using Application.Core.WarehouseTransfer.Commands;
using Application.DTOs.Inventory;
using Domain.Entities;
using MediatR;

namespace Application.Core.WarehouseTransfer.CommandHandlers
{
    // ─── CREATE ──────────────────────────────────────────────────────────────────

    public class CreateWarehouseTransferCommandHandler
        : IRequestHandler<CreateWarehouseTransferCommand, WarehouseTransferResponseDto>
    {
        private readonly IWarehouseTransferRepository _repo;
        private readonly ICodeGeneratorService _codeGen;

        public CreateWarehouseTransferCommandHandler(
            IWarehouseTransferRepository repo,
            ICodeGeneratorService codeGen)
        {
            _repo = repo;
            _codeGen = codeGen;
        }

        public async Task<WarehouseTransferResponseDto> Handle(
            CreateWarehouseTransferCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Data;

            if (dto.SourceWarehouseId == dto.DestinationWarehouseId)
                throw new InvalidOperationException("El almacén de origen y destino no pueden ser el mismo.");

            if (!dto.Details.Any())
                throw new InvalidOperationException("La orden de traspaso debe incluir al menos un producto.");

            if (dto.Details.Any(d => d.QuantityRequested <= 0))
                throw new InvalidOperationException("La cantidad solicitada de cada producto debe ser mayor a cero.");

            var code = await _codeGen.GenerateNextCodeAsync("WTR", "WarehouseTransfers");

            var transfer = new Domain.Entities.WarehouseTransfer
            {
                Code = code,
                SourceWarehouseId = dto.SourceWarehouseId,
                DestinationWarehouseId = dto.DestinationWarehouseId,
                CompanyId = dto.CompanyId,
                TransferDate = dto.TransferDate,
                Status = "Draft",
                Notes = dto.Notes,
                CreatedByUserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var d in dto.Details)
            {
                transfer.Details.Add(new WarehouseTransferDetail
                {
                    ProductId = d.ProductId,
                    QuantityRequested = d.QuantityRequested,
                    QuantityDispatched = 0,
                    QuantityReceived = 0,
                    Notes = d.Notes
                });
            }

            var created = await _repo.CreateAsync(transfer);
            return WarehouseTransferMapper.ToResponseDto(created);
        }
    }

    // ─── UPDATE ──────────────────────────────────────────────────────────────────

    public class UpdateWarehouseTransferCommandHandler
        : IRequestHandler<UpdateWarehouseTransferCommand, WarehouseTransferResponseDto>
    {
        private readonly IWarehouseTransferRepository _repo;

        public UpdateWarehouseTransferCommandHandler(IWarehouseTransferRepository repo)
        {
            _repo = repo;
        }

        public async Task<WarehouseTransferResponseDto> Handle(
            UpdateWarehouseTransferCommand request, CancellationToken cancellationToken)
        {
            var transfer = await _repo.GetByIdAsync(request.TransferId);
            if (transfer == null)
                throw new KeyNotFoundException($"Orden de traspaso con ID {request.TransferId} no encontrada.");

            if (transfer.Status != "Draft")
                throw new InvalidOperationException(
                    $"Solo se pueden modificar órdenes en estado Draft. Estado actual: {transfer.Status}.");

            var dto = request.Data;

            if (!dto.Details.Any())
                throw new InvalidOperationException("La orden debe incluir al menos un producto.");

            if (dto.Details.Any(d => d.QuantityRequested <= 0))
                throw new InvalidOperationException("La cantidad solicitada de cada producto debe ser mayor a cero.");

            transfer.TransferDate = dto.TransferDate;
            transfer.Notes = dto.Notes;
            transfer.UpdatedAt = DateTime.UtcNow;
            transfer.UpdatedByUserId = request.UserId;

            // Reemplazar detalles
            transfer.Details.Clear();
            foreach (var d in dto.Details)
            {
                transfer.Details.Add(new WarehouseTransferDetail
                {
                    ProductId = d.ProductId,
                    QuantityRequested = d.QuantityRequested,
                    QuantityDispatched = 0,
                    QuantityReceived = 0,
                    Notes = d.Notes
                });
            }

            await _repo.UpdateAsync(transfer);
            var updated = await _repo.GetByIdAsync(transfer.Id);
            return WarehouseTransferMapper.ToResponseDto(updated!);
        }
    }

    // ─── CANCEL ──────────────────────────────────────────────────────────────────

    public class CancelWarehouseTransferCommandHandler
        : IRequestHandler<CancelWarehouseTransferCommand, bool>
    {
        private readonly IWarehouseTransferRepository _repo;

        public CancelWarehouseTransferCommandHandler(IWarehouseTransferRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> Handle(
            CancelWarehouseTransferCommand request, CancellationToken cancellationToken)
        {
            var transfer = await _repo.GetByIdAsync(request.TransferId);
            if (transfer == null)
                throw new KeyNotFoundException($"Orden de traspaso con ID {request.TransferId} no encontrada.");

            if (transfer.Status != "Draft")
                throw new InvalidOperationException(
                    $"Solo se pueden cancelar órdenes en estado Draft. Estado actual: {transfer.Status}.");

            transfer.Status = "Cancelled";
            transfer.UpdatedAt = DateTime.UtcNow;
            transfer.UpdatedByUserId = request.UserId;

            await _repo.UpdateAsync(transfer);
            return true;
        }
    }

    // ─── DISPATCH (SALIDA desde almacén origen) ──────────────────────────────────

    public class DispatchWarehouseTransferCommandHandler
        : IRequestHandler<DispatchWarehouseTransferCommand, DispatchWarehouseTransferResponseDto>
    {
        private readonly IWarehouseTransferRepository _repo;
        private readonly IInventoryService _inventoryService;
        private readonly ICodeGeneratorService _codeGen;

        public DispatchWarehouseTransferCommandHandler(
            IWarehouseTransferRepository repo,
            IInventoryService inventoryService,
            ICodeGeneratorService codeGen)
        {
            _repo = repo;
            _inventoryService = inventoryService;
            _codeGen = codeGen;
        }

        public async Task<DispatchWarehouseTransferResponseDto> Handle(
            DispatchWarehouseTransferCommand request, CancellationToken cancellationToken)
        {
            var transfer = await _repo.GetByIdAsync(request.TransferId);
            if (transfer == null)
                throw new KeyNotFoundException($"Orden de traspaso con ID {request.TransferId} no encontrada.");

            if (transfer.Status != "Draft")
                throw new InvalidOperationException(
                    $"Solo se pueden despachar órdenes en estado Draft. Estado actual: {transfer.Status}.");

            // Validar stock disponible en origen para todos los productos
            foreach (var detail in transfer.Details)
            {
                var stock = await _inventoryService.GetStockAsync(detail.ProductId, transfer.SourceWarehouseId);
                var available = stock?.Quantity ?? 0;

                if (available < detail.QuantityRequested)
                    throw new InvalidOperationException(
                        $"Stock insuficiente para '{detail.Product?.name}' en el almacén de origen. " +
                        $"Disponible: {available}, Solicitado: {detail.QuantityRequested}.");
            }

            var dispatchedAt = DateTime.UtcNow;
            var summaries = new List<DispatchMovementSummaryDto>();

            // Aplicar nota adicional de despacho si se proporciona
            if (!string.IsNullOrWhiteSpace(request.Data.Notes))
            {
                transfer.Notes = string.IsNullOrWhiteSpace(transfer.Notes)
                    ? request.Data.Notes
                    : $"{transfer.Notes} | Despacho: {request.Data.Notes}";
            }

            foreach (var detail in transfer.Details)
            {
                var sourceStock = await _inventoryService.GetStockAsync(detail.ProductId, transfer.SourceWarehouseId);
                decimal stockBefore = sourceStock!.Quantity;
                decimal stockAfter = stockBefore - detail.QuantityRequested;

                // Guardar costo unitario al momento del despacho
                detail.UnitCost = sourceStock.AverageCost;
                detail.QuantityDispatched = detail.QuantityRequested;

                // Movimiento de SALIDA en almacén origen
                var movCode = await _codeGen.GenerateNextCodeAsync("MOV", "InventoryMovements");
                var movement = new InventoryMovement
                {
                    Code = movCode,
                    ProductId = detail.ProductId,
                    WarehouseId = transfer.SourceWarehouseId,
                    MovementType = "OUT",
                    MovementSubType = "TRANSFER_OUT",
                    Quantity = -detail.QuantityRequested,
                    UnitCost = sourceStock.AverageCost,
                    TotalCost = detail.QuantityRequested * (sourceStock.AverageCost ?? 0),
                    StockBefore = stockBefore,
                    StockAfter = stockAfter,
                    ReferenceDocumentType = "WarehouseTransfer",
                    ReferenceDocumentId = transfer.Id,
                    ReferenceDocumentCode = transfer.Code,
                    MovementDate = dispatchedAt,
                    Notes = $"Salida traspaso → {transfer.DestinationWarehouse?.Name} ({transfer.Code})",
                    CreatedByUserId = request.UserId,
                    CreatedAt = dispatchedAt
                };
                await _inventoryService.CreateMovementAsync(movement);

                // Actualizar stock en almacén origen
                sourceStock.Quantity = stockAfter;
                sourceStock.AvailableQuantity = stockAfter - sourceStock.ReservedQuantity;
                sourceStock.LastMovementDate = dispatchedAt;
                await _inventoryService.UpdateStockAsync(sourceStock);

                summaries.Add(new DispatchMovementSummaryDto
                {
                    ProductCode = detail.Product?.code ?? string.Empty,
                    ProductName = detail.Product?.name ?? string.Empty,
                    QuantityDispatched = detail.QuantityRequested,
                    MovementCode = movCode,
                    StockBefore = stockBefore,
                    StockAfter = stockAfter
                });
            }

            transfer.Status = "Dispatched";
            transfer.DispatchedAt = dispatchedAt;
            transfer.DispatchedByUserId = request.UserId;
            transfer.UpdatedAt = dispatchedAt;

            await _repo.UpdateAsync(transfer);

            return new DispatchWarehouseTransferResponseDto
            {
                TransferId = transfer.Id,
                TransferCode = transfer.Code,
                Status = transfer.Status,
                DispatchedAt = dispatchedAt,
                TotalMovementsCreated = summaries.Count,
                Movements = summaries
            };
        }
    }

    // ─── RECEIVING (ENTRADA en almacén destino) ───────────────────────────────────

    public class CreateWarehouseTransferReceivingCommandHandler
        : IRequestHandler<CreateWarehouseTransferReceivingCommand, WarehouseTransferReceivingResponseDto>
    {
        private readonly IWarehouseTransferRepository _repo;
        private readonly IInventoryService _inventoryService;
        private readonly ICodeGeneratorService _codeGen;

        public CreateWarehouseTransferReceivingCommandHandler(
            IWarehouseTransferRepository repo,
            IInventoryService inventoryService,
            ICodeGeneratorService codeGen)
        {
            _repo = repo;
            _inventoryService = inventoryService;
            _codeGen = codeGen;
        }

        public async Task<WarehouseTransferReceivingResponseDto> Handle(
            CreateWarehouseTransferReceivingCommand request, CancellationToken cancellationToken)
        {
            var transfer = await _repo.GetByIdAsync(request.TransferId);
            if (transfer == null)
                throw new KeyNotFoundException($"Orden de traspaso con ID {request.TransferId} no encontrada.");

            if (transfer.Status != "Dispatched" && transfer.Status != "PartiallyReceived")
                throw new InvalidOperationException(
                    $"Solo se pueden registrar entradas en órdenes con estado Dispatched o PartiallyReceived. " +
                    $"Estado actual: {transfer.Status}.");

            var dto = request.Data;

            if (!dto.Details.Any())
                throw new InvalidOperationException("Debe incluir al menos un producto en la entrada.");

            if (dto.Details.Any(d => d.QuantityReceived <= 0))
                throw new InvalidOperationException("La cantidad recibida de cada producto debe ser mayor a cero.");

            // Validar que cada detalle referenciado exista y no se exceda la cantidad pendiente
            var detailMap = transfer.Details.ToDictionary(d => d.Id);
            foreach (var recvDetail in dto.Details)
            {
                if (!detailMap.TryGetValue(recvDetail.WarehouseTransferDetailId, out var transferDetail))
                    throw new InvalidOperationException(
                        $"El detalle con ID {recvDetail.WarehouseTransferDetailId} no pertenece a esta orden de traspaso.");

                decimal pending = transferDetail.QuantityDispatched - transferDetail.QuantityReceived;
                if (recvDetail.QuantityReceived > pending)
                    throw new InvalidOperationException(
                        $"La cantidad recibida ({recvDetail.QuantityReceived}) para el producto " +
                        $"'{transferDetail.Product?.name}' supera la cantidad pendiente ({pending}).");
            }

            var receivingDate = dto.ReceivingDate;
            var receivingCode = await _codeGen.GenerateNextCodeAsync("WRV", "WarehouseTransferReceivings");
            var movementSummaries = new List<WarehouseTransferReceivingMovementDto>();
            var receivingDetails = new List<WarehouseTransferReceivingDetail>();

            foreach (var recvDetail in dto.Details)
            {
                var transferDetail = detailMap[recvDetail.WarehouseTransferDetailId];

                // Stock antes y después en almacén destino
                var destStock = await _inventoryService.GetStockAsync(
                    transferDetail.ProductId, transfer.DestinationWarehouseId);
                decimal stockBefore = destStock?.Quantity ?? 0;
                decimal stockAfter = stockBefore + recvDetail.QuantityReceived;

                // Movimiento de ENTRADA en almacén destino
                var movCode = await _codeGen.GenerateNextCodeAsync("MOV", "InventoryMovements");
                var movement = new InventoryMovement
                {
                    Code = movCode,
                    ProductId = transferDetail.ProductId,
                    WarehouseId = transfer.DestinationWarehouseId,
                    MovementType = "IN",
                    MovementSubType = "TRANSFER_IN",
                    Quantity = recvDetail.QuantityReceived,
                    UnitCost = transferDetail.UnitCost,
                    TotalCost = recvDetail.QuantityReceived * (transferDetail.UnitCost ?? 0),
                    StockBefore = stockBefore,
                    StockAfter = stockAfter,
                    ReferenceDocumentType = "WarehouseTransfer",
                    ReferenceDocumentId = transfer.Id,
                    ReferenceDocumentCode = transfer.Code,
                    MovementDate = receivingDate,
                    Notes = $"Entrada traspaso ← {transfer.SourceWarehouse?.Name} ({transfer.Code}) - Recibo: {receivingCode}",
                    CreatedByUserId = request.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _inventoryService.CreateMovementAsync(movement);

                // Crear/actualizar stock en almacén destino
                if (destStock == null)
                {
                    await _inventoryService.CreateStockAsync(new ProductStock
                    {
                        ProductId = transferDetail.ProductId,
                        WarehouseId = transfer.DestinationWarehouseId,
                        Quantity = recvDetail.QuantityReceived,
                        ReservedQuantity = 0,
                        AvailableQuantity = recvDetail.QuantityReceived,
                        AverageCost = transferDetail.UnitCost,
                        LastMovementDate = receivingDate,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    destStock.Quantity = stockAfter;
                    destStock.AvailableQuantity = stockAfter - destStock.ReservedQuantity;
                    destStock.LastMovementDate = receivingDate;
                    await _inventoryService.UpdateStockAsync(destStock);
                }

                // Actualizar cantidad recibida en el detalle de la orden
                transferDetail.QuantityReceived += recvDetail.QuantityReceived;

                receivingDetails.Add(new WarehouseTransferReceivingDetail
                {
                    WarehouseTransferDetailId = recvDetail.WarehouseTransferDetailId,
                    ProductId = transferDetail.ProductId,
                    QuantityReceived = recvDetail.QuantityReceived,
                    Notes = recvDetail.Notes
                });

                movementSummaries.Add(new WarehouseTransferReceivingMovementDto
                {
                    ProductCode = transferDetail.Product?.code ?? string.Empty,
                    ProductName = transferDetail.Product?.name ?? string.Empty,
                    QuantityReceived = recvDetail.QuantityReceived,
                    MovementCode = movCode,
                    StockBefore = stockBefore,
                    StockAfter = stockAfter
                });
            }

            // Determinar si la recepción es completa (todos los productos de la orden completamente recibidos)
            bool allFullyReceived = transfer.Details.All(
                d => d.QuantityReceived >= d.QuantityDispatched);

            string receivingType = allFullyReceived ? "Complete" : "Partial";

            // Crear el registro de recepción
            var receiving = new WarehouseTransferReceiving
            {
                Code = receivingCode,
                WarehouseTransferId = transfer.Id,
                DestinationWarehouseId = transfer.DestinationWarehouseId,
                ReceivingDate = receivingDate,
                ReceivingType = receivingType,
                Notes = dto.Notes,
                CreatedByUserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var d in receivingDetails)
                receiving.Details.Add(d);

            // Actualizar estado de la orden
            transfer.Status = allFullyReceived ? "Received" : "PartiallyReceived";
            transfer.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateWithReceivingAsync(transfer, receiving);

            return new WarehouseTransferReceivingResponseDto
            {
                Id = receiving.Id,
                Code = receiving.Code,
                WarehouseTransferId = transfer.Id,
                WarehouseTransferCode = transfer.Code,
                DestinationWarehouseId = transfer.DestinationWarehouseId,
                DestinationWarehouseName = transfer.DestinationWarehouse?.Name ?? string.Empty,
                ReceivingDate = receiving.ReceivingDate,
                ReceivingType = receiving.ReceivingType,
                Notes = receiving.Notes,
                CreatedAt = receiving.CreatedAt,
                CreatedByUserName = null,
                Details = receivingDetails.Select((d, i) => new WarehouseTransferReceivingDetailResponseDto
                {
                    Id = d.Id,
                    WarehouseTransferDetailId = d.WarehouseTransferDetailId,
                    ProductId = d.ProductId,
                    ProductCode = movementSummaries[i].ProductCode,
                    ProductName = movementSummaries[i].ProductName,
                    QuantityReceived = d.QuantityReceived,
                    Notes = d.Notes
                }).ToList(),
                Movements = movementSummaries,
                TotalProducts = receivingDetails.Count,
                TotalQuantityReceived = receivingDetails.Sum(d => d.QuantityReceived)
            };
        }
    }

    // ─── Mapper helper (internal) ─────────────────────────────────────────────────

    internal static class WarehouseTransferMapper
    {
        internal static WarehouseTransferResponseDto ToResponseDto(Domain.Entities.WarehouseTransfer t) => new()
        {
            Id = t.Id,
            Code = t.Code,
            Status = t.Status,
            SourceWarehouseId = t.SourceWarehouseId,
            SourceWarehouseName = t.SourceWarehouse?.Name ?? string.Empty,
            SourceWarehouseCode = t.SourceWarehouse?.Code ?? string.Empty,
            DestinationWarehouseId = t.DestinationWarehouseId,
            DestinationWarehouseName = t.DestinationWarehouse?.Name ?? string.Empty,
            DestinationWarehouseCode = t.DestinationWarehouse?.Code ?? string.Empty,
            CompanyId = t.CompanyId,
            TransferDate = t.TransferDate,
            Notes = t.Notes,
            DispatchedAt = t.DispatchedAt,
            DispatchedByUserName = t.DispatchedBy?.Name,
            CreatedAt = t.CreatedAt,
            CreatedByUserName = t.CreatedBy?.Name,
            Details = t.Details.Select(d => new WarehouseTransferDetailResponseDto
            {
                Id = d.Id,
                ProductId = d.ProductId,
                ProductCode = d.Product?.code ?? string.Empty,
                ProductName = d.Product?.name ?? string.Empty,
                QuantityRequested = d.QuantityRequested,
                QuantityDispatched = d.QuantityDispatched,
                QuantityReceived = d.QuantityReceived,
                PendingQuantity = d.QuantityDispatched - d.QuantityReceived,
                UnitCost = d.UnitCost,
                Notes = d.Notes
            }).ToList(),
            Receivings = t.Receivings.Select(r => new WarehouseTransferReceivingListItemDto
            {
                Id = r.Id,
                Code = r.Code,
                ReceivingDate = r.ReceivingDate,
                ReceivingType = r.ReceivingType,
                TotalProducts = r.Details.Count,
                TotalQuantityReceived = r.Details.Sum(d => d.QuantityReceived),
                CreatedAt = r.CreatedAt,
                CreatedByUserName = r.CreatedBy?.Name
            }).ToList(),
            TotalProducts = t.Details.Count,
            TotalQuantityRequested = t.Details.Sum(d => d.QuantityRequested),
            TotalQuantityDispatched = t.Details.Sum(d => d.QuantityDispatched),
            TotalQuantityReceived = t.Details.Sum(d => d.QuantityReceived),
            TotalPendingQuantity = t.Details.Sum(d => d.QuantityDispatched - d.QuantityReceived)
        };
    }
}
