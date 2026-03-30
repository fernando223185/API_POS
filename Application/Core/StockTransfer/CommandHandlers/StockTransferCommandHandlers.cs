using Application.Abstractions.Common;
using Application.Abstractions.Inventory;
using Application.Core.StockTransfer.Commands;
using Application.DTOs.Inventory;
using Domain.Entities;
using MediatR;

namespace Application.Core.StockTransfer.CommandHandlers
{
    // ─── CREATE ──────────────────────────────────────────────────────────────────

    public class CreateStockTransferCommandHandler : IRequestHandler<CreateStockTransferCommand, StockTransferResponseDto>
    {
        private readonly IStockTransferRepository _transferRepo;
        private readonly ICodeGeneratorService _codeGenerator;

        public CreateStockTransferCommandHandler(
            IStockTransferRepository transferRepo,
            ICodeGeneratorService codeGenerator)
        {
            _transferRepo = transferRepo;
            _codeGenerator = codeGenerator;
        }

        public async Task<StockTransferResponseDto> Handle(CreateStockTransferCommand request, CancellationToken cancellationToken)
        {
            var dto = request.TransferData;

            if (dto.SourceWarehouseId == dto.DestinationWarehouseId)
                throw new InvalidOperationException("El almacén de origen y destino no pueden ser el mismo.");

            if (!dto.Details.Any())
                throw new InvalidOperationException("El traspaso debe incluir al menos un producto.");

            if (dto.Details.Any(d => d.Quantity <= 0))
                throw new InvalidOperationException("La cantidad de cada producto debe ser mayor a cero.");

            var code = await _codeGenerator.GenerateNextCodeAsync("TRF", "StockTransfers");

            var transfer = new Domain.Entities.StockTransfer
            {
                Code = code,
                SourceWarehouseId = dto.SourceWarehouseId,
                DestinationWarehouseId = dto.DestinationWarehouseId,
                CompanyId = dto.CompanyId,
                TransferDate = dto.TransferDate,
                Status = "Draft",
                Notes = dto.Notes,
                IsApplied = false,
                CreatedByUserId = request.CreatedByUserId,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var d in dto.Details)
            {
                transfer.Details.Add(new StockTransferDetail
                {
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    Notes = d.Notes
                });
            }

            var created = await _transferRepo.CreateAsync(transfer);
            return MapToResponse(created);
        }

        private static StockTransferResponseDto MapToResponse(Domain.Entities.StockTransfer t) => new()
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
            IsApplied = t.IsApplied,
            AppliedAt = t.AppliedAt,
            AppliedByUserName = t.AppliedBy?.Name,
            CreatedAt = t.CreatedAt,
            CreatedByUserName = t.CreatedBy?.Name,
            Details = t.Details.Select(d => new StockTransferDetailResponseDto
            {
                Id = d.Id,
                ProductId = d.ProductId,
                ProductCode = d.Product?.code ?? string.Empty,
                ProductName = d.Product?.name ?? string.Empty,
                Quantity = d.Quantity,
                UnitCost = d.UnitCost,
                Notes = d.Notes
            }).ToList(),
            TotalProducts = t.Details.Count,
            TotalQuantity = t.Details.Sum(d => d.Quantity)
        };
    }

    // ─── APPLY (POST TO INVENTORY) ────────────────────────────────────────────

    public class ApplyStockTransferCommandHandler : IRequestHandler<ApplyStockTransferCommand, ApplyStockTransferResponseDto>
    {
        private readonly IStockTransferRepository _transferRepo;
        private readonly IInventoryService _inventoryService;
        private readonly ICodeGeneratorService _codeGenerator;

        public ApplyStockTransferCommandHandler(
            IStockTransferRepository transferRepo,
            IInventoryService inventoryService,
            ICodeGeneratorService codeGenerator)
        {
            _transferRepo = transferRepo;
            _inventoryService = inventoryService;
            _codeGenerator = codeGenerator;
        }

        public async Task<ApplyStockTransferResponseDto> Handle(ApplyStockTransferCommand request, CancellationToken cancellationToken)
        {
            var transfer = await _transferRepo.GetByIdAsync(request.TransferId);
            if (transfer == null)
                throw new KeyNotFoundException($"Traspaso con ID {request.TransferId} no encontrado.");

            if (transfer.IsApplied)
                throw new InvalidOperationException("Este traspaso ya fue aplicado al inventario.");

            if (transfer.Status != "Draft")
                throw new InvalidOperationException($"Solo se pueden aplicar traspasos en estado Draft. Estado actual: {transfer.Status}.");

            // Validar stock disponible en origen antes de aplicar
            foreach (var detail in transfer.Details)
            {
                var sourceStock = await _inventoryService.GetStockAsync(detail.ProductId, transfer.SourceWarehouseId);
                var available = sourceStock?.Quantity ?? 0;

                if (available < detail.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Stock insuficiente para '{detail.Product?.name}' en el almacén de origen. " +
                        $"Disponible: {available}, Solicitado: {detail.Quantity}.");
                }
            }

            var movementSummaries = new List<TransferMovementSummaryDto>();

            foreach (var detail in transfer.Details)
            {
                // ── Stock origen ──────────────────────────────────────────
                var sourceStock = await _inventoryService.GetStockAsync(detail.ProductId, transfer.SourceWarehouseId);
                decimal sourceBefore = sourceStock!.Quantity;
                decimal sourceAfter = sourceBefore - detail.Quantity;

                // ── Stock destino ─────────────────────────────────────────
                var destStock = await _inventoryService.GetStockAsync(detail.ProductId, transfer.DestinationWarehouseId);
                decimal destBefore = destStock?.Quantity ?? 0;
                decimal destAfter = destBefore + detail.Quantity;

                // Guardar el costo unitario en el detalle
                detail.UnitCost = sourceStock.AverageCost;

                // ── Movimiento OUT (origen) ────────────────────────────────
                var outCode = await _codeGenerator.GenerateNextCodeAsync("MOV", "InventoryMovements");
                var outMovement = new InventoryMovement
                {
                    Code = outCode,
                    ProductId = detail.ProductId,
                    WarehouseId = transfer.SourceWarehouseId,
                    MovementType = "TRANSFER",
                    MovementSubType = "TRANSFER_OUT",
                    Quantity = -detail.Quantity,
                    UnitCost = sourceStock.AverageCost,
                    TotalCost = detail.Quantity * (sourceStock.AverageCost ?? 0),
                    StockBefore = sourceBefore,
                    StockAfter = sourceAfter,
                    ReferenceDocumentType = "StockTransfer",
                    ReferenceDocumentId = transfer.Id,
                    ReferenceDocumentCode = transfer.Code,
                    StockTransferId = transfer.Id,
                    MovementDate = DateTime.UtcNow,
                    Notes = $"Traspaso salida → {transfer.DestinationWarehouse?.Name} ({transfer.Code})",
                    CreatedByUserId = request.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _inventoryService.CreateMovementAsync(outMovement);

                // ── Actualizar stock origen ───────────────────────────────
                sourceStock.Quantity = sourceAfter;
                sourceStock.AvailableQuantity = sourceAfter - sourceStock.ReservedQuantity;
                sourceStock.LastMovementDate = DateTime.UtcNow;
                await _inventoryService.UpdateStockAsync(sourceStock);

                // ── Movimiento IN (destino) ───────────────────────────────
                var inCode = await _codeGenerator.GenerateNextCodeAsync("MOV", "InventoryMovements");
                var inMovement = new InventoryMovement
                {
                    Code = inCode,
                    ProductId = detail.ProductId,
                    WarehouseId = transfer.DestinationWarehouseId,
                    MovementType = "TRANSFER",
                    MovementSubType = "TRANSFER_IN",
                    Quantity = detail.Quantity,
                    UnitCost = sourceStock.AverageCost,
                    TotalCost = detail.Quantity * (sourceStock.AverageCost ?? 0),
                    StockBefore = destBefore,
                    StockAfter = destAfter,
                    ReferenceDocumentType = "StockTransfer",
                    ReferenceDocumentId = transfer.Id,
                    ReferenceDocumentCode = transfer.Code,
                    StockTransferId = transfer.Id,
                    MovementDate = DateTime.UtcNow,
                    Notes = $"Traspaso entrada ← {transfer.SourceWarehouse?.Name} ({transfer.Code})",
                    CreatedByUserId = request.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _inventoryService.CreateMovementAsync(inMovement);

                // ── Actualizar/crear stock destino ────────────────────────
                if (destStock == null)
                {
                    destStock = new ProductStock
                    {
                        ProductId = detail.ProductId,
                        WarehouseId = transfer.DestinationWarehouseId,
                        Quantity = destAfter,
                        AvailableQuantity = destAfter,
                        AverageCost = sourceStock.AverageCost,
                        LastMovementDate = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _inventoryService.CreateStockAsync(destStock);
                }
                else
                {
                    destStock.Quantity = destAfter;
                    destStock.AvailableQuantity = destAfter - destStock.ReservedQuantity;
                    destStock.LastMovementDate = DateTime.UtcNow;
                    destStock.UpdatedAt = DateTime.UtcNow;
                    await _inventoryService.UpdateStockAsync(destStock);
                }

                movementSummaries.Add(new TransferMovementSummaryDto
                {
                    ProductCode = detail.Product?.code ?? string.Empty,
                    ProductName = detail.Product?.name ?? string.Empty,
                    Quantity = detail.Quantity,
                    OutMovementCode = outCode,
                    InMovementCode = inCode,
                    SourceStockBefore = sourceBefore,
                    SourceStockAfter = sourceAfter,
                    DestinationStockBefore = destBefore,
                    DestinationStockAfter = destAfter
                });
            }

            // Marcar traspaso como completado
            transfer.IsApplied = true;
            transfer.AppliedAt = DateTime.UtcNow;
            transfer.AppliedByUserId = request.UserId;
            transfer.Status = "Completed";
            await _transferRepo.UpdateAsync(transfer);

            return new ApplyStockTransferResponseDto
            {
                Message = $"Traspaso {transfer.Code} aplicado correctamente. {movementSummaries.Count * 2} movimientos creados.",
                Error = 0,
                Data = new ApplyStockTransferDataDto
                {
                    TransferId = transfer.Id,
                    TransferCode = transfer.Code,
                    TotalMovementsCreated = movementSummaries.Count * 2,
                    Movements = movementSummaries
                }
            };
        }
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

    public class UpdateStockTransferCommandHandler : IRequestHandler<UpdateStockTransferCommand, StockTransferResponseDto>
    {
        private readonly IStockTransferRepository _transferRepo;

        public UpdateStockTransferCommandHandler(IStockTransferRepository transferRepo)
        {
            _transferRepo = transferRepo;
        }

        public async Task<StockTransferResponseDto> Handle(UpdateStockTransferCommand request, CancellationToken cancellationToken)
        {
            var transfer = await _transferRepo.GetByIdAsync(request.TransferId);
            if (transfer == null)
                throw new KeyNotFoundException($"Traspaso con ID {request.TransferId} no encontrado.");

            if (transfer.Status != "Draft")
                throw new InvalidOperationException("Solo se pueden editar traspasos en estado Draft.");

            var dto = request.TransferData;

            if (dto.Details.Any(d => d.Quantity <= 0))
                throw new InvalidOperationException("La cantidad de cada producto debe ser mayor a cero.");

            transfer.TransferDate = dto.TransferDate;
            transfer.Notes = dto.Notes;
            transfer.UpdatedAt = DateTime.UtcNow;

            transfer.Details.Clear();
            foreach (var d in dto.Details)
            {
                transfer.Details.Add(new StockTransferDetail
                {
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    Notes = d.Notes
                });
            }

            await _transferRepo.UpdateAsync(transfer);

            var updated = await _transferRepo.GetByIdAsync(request.TransferId);
            return MapToResponse(updated!);
        }

        private static StockTransferResponseDto MapToResponse(Domain.Entities.StockTransfer t) => new()
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
            IsApplied = t.IsApplied,
            AppliedAt = t.AppliedAt,
            AppliedByUserName = t.AppliedBy?.Name,
            CreatedAt = t.CreatedAt,
            CreatedByUserName = t.CreatedBy?.Name,
            Details = t.Details.Select(d => new StockTransferDetailResponseDto
            {
                Id = d.Id,
                ProductId = d.ProductId,
                ProductCode = d.Product?.code ?? string.Empty,
                ProductName = d.Product?.name ?? string.Empty,
                Quantity = d.Quantity,
                UnitCost = d.UnitCost,
                Notes = d.Notes
            }).ToList(),
            TotalProducts = t.Details.Count,
            TotalQuantity = t.Details.Sum(d => d.Quantity)
        };
    }

    // ─── CANCEL ───────────────────────────────────────────────────────────────

    public class CancelStockTransferCommandHandler : IRequestHandler<CancelStockTransferCommand, bool>
    {
        private readonly IStockTransferRepository _transferRepo;

        public CancelStockTransferCommandHandler(IStockTransferRepository transferRepo)
        {
            _transferRepo = transferRepo;
        }

        public async Task<bool> Handle(CancelStockTransferCommand request, CancellationToken cancellationToken)
        {
            var transfer = await _transferRepo.GetByIdAsync(request.TransferId);
            if (transfer == null)
                throw new KeyNotFoundException($"Traspaso con ID {request.TransferId} no encontrado.");

            if (transfer.Status != "Draft")
                throw new InvalidOperationException("Solo se pueden cancelar traspasos en estado Draft.");

            transfer.Status = "Cancelled";
            transfer.UpdatedAt = DateTime.UtcNow;
            await _transferRepo.UpdateAsync(transfer);
            return true;
        }
    }
}
