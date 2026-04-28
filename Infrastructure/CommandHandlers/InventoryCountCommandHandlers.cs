using Application.Abstractions.Common;
using Application.Core.Inventory.Commands;
using Application.DTOs.Inventory;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.CommandHandlers
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CREAR NUEVA SESIÓN DE CONTEO
    // ═══════════════════════════════════════════════════════════════════════════

    public class CreateInventoryCountCommandHandler :
        IRequestHandler<CreateInventoryCountCommand, InventoryCountResponseDto>
    {
        private readonly POSDbContext _context;
        private readonly ICodeGeneratorService _codeGenerator;

        public CreateInventoryCountCommandHandler(
            POSDbContext context,
            ICodeGeneratorService codeGenerator)
        {
            _context = context;
            _codeGenerator = codeGenerator;
        }

        public async Task<InventoryCountResponseDto> Handle(CreateInventoryCountCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            // Validar almacén
            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Id == dto.WarehouseId, cancellationToken);

            if (warehouse == null)
                throw new ArgumentException("Almacén no encontrado");

            // Validar usuario asignado
            var assignedUser = await _context.User
                .FirstOrDefaultAsync(u => u.Id == dto.AssignedToUserId, cancellationToken);

            if (assignedUser == null)
                throw new ArgumentException("Usuario asignado no encontrado");

            // Validar tipo de conteo
            var validTypes = new[] { CountType.CYCLE, CountType.FULL, CountType.CATEGORY, CountType.LOCATION };
            if (!validTypes.Contains(dto.CountType))
                throw new ArgumentException("Tipo de conteo inválido");

            // Si es por categoría, validar que exista
            if (dto.CountType == CountType.CATEGORY && !dto.CategoryId.HasValue)
                throw new ArgumentException("Debe especificar una categoría para conteo por categoría");

            // Generar código único (CIC-000001, CIC-000002, etc.)
            var code = await _codeGenerator.GenerateNextCodeAsync("CIC", "InventoryCounts");

            // Crear sesión de conteo
            var inventoryCount = new InventoryCount
            {
                Code = code,
                WarehouseId = dto.WarehouseId,
                CountType = dto.CountType,
                Status = CountStatus.DRAFT,
                ScheduledDate = dto.ScheduledDate,
                AssignedToUserId = dto.AssignedToUserId,
                CategoryId = dto.CategoryId,
                Notes = dto.Notes,
                CreatedByUserId = request.CreatedByUserId,
                CreatedAt = DateTime.UtcNow,
                CompanyId = request.CompanyId
            };

            _context.InventoryCounts.Add(inventoryCount);
            await _context.SaveChangesAsync(cancellationToken);

            // Seleccionar productos a contar según criterios
            List<int> productIds;

            if (dto.ProductIds != null && dto.ProductIds.Any())
            {
                // Lista manual proporcionada
                productIds = dto.ProductIds;
            }
            else
            {
                // Selección automática según tipo
                productIds = await SelectProductsForCount(dto.WarehouseId, dto.CountType, dto.CategoryId, cancellationToken);
            }

            // Crear detalles del conteo
            foreach (var productId in productIds.Distinct())
            {
                var productStock = await _context.ProductStock
                    .FirstOrDefaultAsync(ps => ps.ProductId == productId && ps.WarehouseId == dto.WarehouseId, cancellationToken);

                var detail = new InventoryCountDetail
                {
                    InventoryCountId = inventoryCount.Id,
                    ProductId = productId,
                    SystemQuantity = productStock?.Quantity ?? 0,
                    UnitCost = productStock?.AverageCost,
                    Status = CountDetailStatus.PENDING
                };

                _context.InventoryCountDetails.Add(detail);
            }

            inventoryCount.TotalProducts = productIds.Distinct().Count();
            inventoryCount.CountedProducts = 0;

            await _context.SaveChangesAsync(cancellationToken);

            // Cargar y retornar el resultado completo
            return await LoadInventoryCountResponse(inventoryCount.Id, cancellationToken);
        }

        private async Task<List<int>> SelectProductsForCount(
            int warehouseId,
            string countType,
            int? categoryId,
            CancellationToken cancellationToken)
        {
            var query = _context.ProductStock
                .Where(ps => ps.WarehouseId == warehouseId)
                .Join(_context.Products,
                    ps => ps.ProductId,
                    p => p.ID,
                    (ps, p) => new { ps, p })
                .Where(x => x.p.IsActive);

            switch (countType)
            {
                case CountType.FULL:
                    // Todos los productos del almacén
                    return await query.Select(x => x.ps.ProductId).ToListAsync(cancellationToken);

                case CountType.CATEGORY:
                    // Productos de una categoría específica
                    return await query
                        .Where(x => x.p.CategoryId == categoryId)
                        .Select(x => x.ps.ProductId)
                        .ToListAsync(cancellationToken);

                case CountType.CYCLE:
                    // Conteo cíclico: seleccionar productos según criterios
                    // Por ahora, primeros 50 productos (puede mejorarse con lógica ABC)
                    return await query
                        .OrderBy(x => x.p.name)
                        .Take(50)
                        .Select(x => x.ps.ProductId)
                        .ToListAsync(cancellationToken);

                case CountType.LOCATION:
                    // Por ahora, todos los productos (puede mejorarse con ubicaciones físicas)
                    return await query.Select(x => x.ps.ProductId).ToListAsync(cancellationToken);

                default:
                    return new List<int>();
            }
        }

        private async Task<InventoryCountResponseDto> LoadInventoryCountResponse(int countId, CancellationToken cancellationToken)
        {
            var count = await _context.InventoryCounts
                .Include(c => c.Warehouse)
                .Include(c => c.AssignedToUser)
                .Include(c => c.ApprovedByUser)
                .Include(c => c.Category)
                .Include(c => c.CreatedByUser)
                .Include(c => c.Details)
                    .ThenInclude(d => d.Product)
                .Include(c => c.Details)
                    .ThenInclude(d => d.CountedByUser)
                .Include(c => c.Details)
                    .ThenInclude(d => d.RecountedByUser)
                .FirstAsync(c => c.Id == countId, cancellationToken);

            return MapToResponse(count);
        }

        private InventoryCountResponseDto MapToResponse(InventoryCount count)
        {
            var progressPercentage = count.TotalProducts > 0
                ? (decimal)count.CountedProducts / count.TotalProducts * 100
                : 0;

            return new InventoryCountResponseDto
            {
                Id = count.Id,
                Code = count.Code,
                WarehouseId = count.WarehouseId,
                WarehouseName = count.Warehouse?.Name ?? "N/A",
                CountType = count.CountType,
                CountTypeLabel = CountType.GetLabel(count.CountType),
                Status = count.Status,
                StatusLabel = CountStatus.GetLabel(count.Status),
                ScheduledDate = count.ScheduledDate,
                StartedAt = count.StartedAt,
                CompletedAt = count.CompletedAt,
                AssignedToUserId = count.AssignedToUserId,
                AssignedToUserName = count.AssignedToUser?.Name ?? "N/A",
                ApprovedByUserId = count.ApprovedByUserId,
                ApprovedByUserName = count.ApprovedByUser?.Name,
                ApprovedAt = count.ApprovedAt,
                CategoryId = count.CategoryId,
                CategoryName = count.Category?.Name,
                Notes = count.Notes,
                TotalProducts = count.TotalProducts,
                CountedProducts = count.CountedProducts,
                PendingProducts = count.TotalProducts - count.CountedProducts,
                ProductsWithVariance = count.ProductsWithVariance,
                ProgressPercentage = progressPercentage,
                TotalVarianceCost = count.TotalVarianceCost,
                CreatedByUserId = count.CreatedByUserId,
                CreatedByUserName = count.CreatedByUser?.Name ?? "N/A",
                CreatedAt = count.CreatedAt,
                CompanyId = count.CompanyId,
                Details = count.Details?.Select(d => new InventoryCountDetailResponseDto
                {
                    Id = d.Id,
                    InventoryCountId = d.InventoryCountId,
                    ProductId = d.ProductId,
                    ProductCode = d.Product?.code ?? "N/A",
                    ProductName = d.Product?.name ?? "N/A",
                    SystemQuantity = d.SystemQuantity,
                    PhysicalQuantity = d.PhysicalQuantity,
                    Variance = d.Variance,
                    VariancePercentage = d.VariancePercentage,
                    VarianceCost = d.VarianceCost,
                    UnitCost = d.UnitCost,
                    Status = d.Status,
                    StatusLabel = CountDetailStatus.GetLabel(d.Status),
                    Notes = d.Notes,
                    CountedByUserId = d.CountedByUserId,
                    CountedByUserName = d.CountedByUser?.Name,
                    CountedAt = d.CountedAt,
                    RecountRequested = d.RecountRequested,
                    RecountedByUserId = d.RecountedByUserId,
                    RecountedByUserName = d.RecountedByUser?.Name,
                    RecountedAt = d.RecountedAt
                }).ToList() ?? new List<InventoryCountDetailResponseDto>()
            };
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INICIAR CONTEO (DRAFT → IN PROGRESS)
    // ═══════════════════════════════════════════════════════════════════════════

    public class StartInventoryCountCommandHandler :
        IRequestHandler<StartInventoryCountCommand, InventoryCountResponseDto>
    {
        private readonly POSDbContext _context;

        public StartInventoryCountCommandHandler(POSDbContext context)
        {
            _context = context;
        }

        public async Task<InventoryCountResponseDto> Handle(StartInventoryCountCommand request, CancellationToken cancellationToken)
        {
            var count = await _context.InventoryCounts
                .Include(c => c.Warehouse)
                .Include(c => c.AssignedToUser)
                .Include(c => c.CreatedByUser)
                .Include(c => c.Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(c => c.Id == request.CountId, cancellationToken);

            if (count == null)
                throw new KeyNotFoundException($"Conteo con ID {request.CountId} no encontrado");

            if (count.Status != CountStatus.DRAFT)
                throw new InvalidOperationException($"Solo los conteos en estado Borrador pueden iniciarse. Estado actual: {count.Status}");

            count.Status = CountStatus.IN_PROGRESS;
            count.StartedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(request.Dto.StartNotes))
            {
                count.Notes = string.IsNullOrEmpty(count.Notes)
                    ? request.Dto.StartNotes
                    : count.Notes + "\n\n" + request.Dto.StartNotes;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return MapToResponse(count);
        }

        private InventoryCountResponseDto MapToResponse(InventoryCount count)
        {
            var progressPercentage = count.TotalProducts > 0
                ? (decimal)count.CountedProducts / count.TotalProducts * 100
                : 0;

            return new InventoryCountResponseDto
            {
                Id = count.Id,
                Code = count.Code,
                WarehouseId = count.WarehouseId,
                WarehouseName = count.Warehouse?.Name ?? "N/A",
                CountType = count.CountType,
                CountTypeLabel = CountType.GetLabel(count.CountType),
                Status = count.Status,
                StatusLabel = CountStatus.GetLabel(count.Status),
                ScheduledDate = count.ScheduledDate,
                StartedAt = count.StartedAt,
                CompletedAt = count.CompletedAt,
                AssignedToUserId = count.AssignedToUserId,
                AssignedToUserName = count.AssignedToUser?.Name ?? "N/A",
                ApprovedByUserId = count.ApprovedByUserId,
                ApprovedByUserName = count.ApprovedByUser?.Name,
                ApprovedAt = count.ApprovedAt,
                CategoryId = count.CategoryId,
                CategoryName = count.Category?.Name,
                Notes = count.Notes,
                TotalProducts = count.TotalProducts,
                CountedProducts = count.CountedProducts,
                PendingProducts = count.TotalProducts - count.CountedProducts,
                ProductsWithVariance = count.ProductsWithVariance,
                ProgressPercentage = progressPercentage,
                TotalVarianceCost = count.TotalVarianceCost,
                CreatedByUserId = count.CreatedByUserId,
                CreatedByUserName = count.CreatedByUser?.Name ?? "N/A",
                CreatedAt = count.CreatedAt,
                CompanyId = count.CompanyId,
                Details = count.Details?.Select(d => new InventoryCountDetailResponseDto
                {
                    Id = d.Id,
                    InventoryCountId = d.InventoryCountId,
                    ProductId = d.ProductId,
                    ProductCode = d.Product?.code ?? "N/A",
                    ProductName = d.Product?.name ?? "N/A",
                    SystemQuantity = d.SystemQuantity,
                    PhysicalQuantity = d.PhysicalQuantity,
                    Variance = d.Variance,
                    VariancePercentage = d.VariancePercentage,
                    VarianceCost = d.VarianceCost,
                    UnitCost = d.UnitCost,
                    Status = d.Status,
                    StatusLabel = CountDetailStatus.GetLabel(d.Status),
                    Notes = d.Notes,
                    CountedByUserId = d.CountedByUserId,
                    CountedByUserName = d.CountedByUser?.Name,
                    CountedAt = d.CountedAt,
                    RecountRequested = d.RecountRequested,
                    RecountedByUserId = d.RecountedByUserId,
                    RecountedByUserName = d.RecountedByUser?.Name,
                    RecountedAt = d.RecountedAt
                }).ToList() ?? new List<InventoryCountDetailResponseDto>()
            };
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ACTUALIZAR CANTIDAD FÍSICA DE UN PRODUCTO
    // ═══════════════════════════════════════════════════════════════════════════

    public class UpdateCountDetailCommandHandler :
        IRequestHandler<UpdateCountDetailCommand, InventoryCountDetailResponseDto>
    {
        private readonly POSDbContext _context;

        public UpdateCountDetailCommandHandler(POSDbContext context)
        {
            _context = context;
        }

        public async Task<InventoryCountDetailResponseDto> Handle(UpdateCountDetailCommand request, CancellationToken cancellationToken)
        {
            var count = await _context.InventoryCounts
                .Include(c => c.Details)
                .FirstOrDefaultAsync(c => c.Id == request.CountId, cancellationToken);

            if (count == null)
                throw new KeyNotFoundException($"Conteo con ID {request.CountId} no encontrado");

            if (count.Status != CountStatus.IN_PROGRESS)
                throw new InvalidOperationException("Solo los conteos en progreso pueden actualizarse");

            var detail = count.Details.FirstOrDefault(d => d.Id == request.DetailId);

            if (detail == null)
                throw new KeyNotFoundException($"Detalle con ID {request.DetailId} no encontrado en este conteo");

            // Actualizar cantidad física
            detail.PhysicalQuantity = request.Dto.PhysicalQuantity;
            detail.Notes = request.Dto.Notes;
            detail.CountedByUserId = request.CountedByUserId;
            detail.CountedAt = DateTime.UtcNow;
            detail.Status = CountDetailStatus.COUNTED;

            // Calcular variaciones
            detail.Variance = detail.PhysicalQuantity - detail.SystemQuantity;
            detail.VariancePercentage = detail.SystemQuantity != 0
                ? (detail.Variance / detail.SystemQuantity) * 100
                : 0;
            detail.VarianceCost = detail.UnitCost.HasValue
                ? detail.Variance * detail.UnitCost.Value
                : null;

            // Actualizar contadores del conteo padre
            count.CountedProducts = count.Details.Count(d => d.Status != CountDetailStatus.PENDING);
            count.ProductsWithVariance = count.Details.Count(d => d.Variance.HasValue && d.Variance.Value != 0);
            count.TotalVarianceCost = count.Details
                .Where(d => d.VarianceCost.HasValue)
                .Sum(d => d.VarianceCost!.Value);

            await _context.SaveChangesAsync(cancellationToken);

            // Cargar detalle completo con relaciones
            var savedDetail = await _context.InventoryCountDetails
                .Include(d => d.Product)
                .Include(d => d.CountedByUser)
                .FirstAsync(d => d.Id == detail.Id, cancellationToken);

            return new InventoryCountDetailResponseDto
            {
                Id = savedDetail.Id,
                ProductId = savedDetail.ProductId,
                ProductCode = savedDetail.Product.code,
                ProductName = savedDetail.Product.name,
                SystemQuantity = savedDetail.SystemQuantity,
                PhysicalQuantity = savedDetail.PhysicalQuantity,
                Variance = savedDetail.Variance,
                VariancePercentage = savedDetail.VariancePercentage,
                VarianceCost = savedDetail.VarianceCost,
                UnitCost = savedDetail.UnitCost,
                Status = savedDetail.Status,
                StatusLabel = CountDetailStatus.GetLabel(savedDetail.Status),
                Notes = savedDetail.Notes,
                CountedByUserId = savedDetail.CountedByUserId,
                CountedByUserName = savedDetail.CountedByUser?.Name,
                CountedAt = savedDetail.CountedAt
            };
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPLETAR Y APROBAR CONTEO (GENERA AJUSTES AUTOMÁTICAMENTE)
    // ═══════════════════════════════════════════════════════════════════════════

    public class CompleteInventoryCountCommandHandler :
        IRequestHandler<CompleteInventoryCountCommand, InventoryCountResponseDto>
    {
        private readonly POSDbContext _context;
        private readonly ICodeGeneratorService _codeGenerator;

        public CompleteInventoryCountCommandHandler(
            POSDbContext context,
            ICodeGeneratorService codeGenerator)
        {
            _context = context;
            _codeGenerator = codeGenerator;
        }

        public async Task<InventoryCountResponseDto> Handle(CompleteInventoryCountCommand request, CancellationToken cancellationToken)
        {
            var count = await _context.InventoryCounts
                .Include(c => c.Warehouse)
                .Include(c => c.AssignedToUser)
                .Include(c => c.CreatedByUser)
                .Include(c => c.Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(c => c.Id == request.CountId, cancellationToken);

            if (count == null)
                throw new KeyNotFoundException($"Conteo con ID {request.CountId} no encontrado");

            if (count.Status != CountStatus.IN_PROGRESS)
                throw new InvalidOperationException("Solo los conteos en progreso pueden completarse");

            // Verificar que todos los productos han sido contados
            var pendingProducts = count.Details.Count(d => d.Status == CountDetailStatus.PENDING);
            if (pendingProducts > 0)
                throw new InvalidOperationException($"Hay {pendingProducts} producto(s) pendiente(s) de contar");

            // Generar ajustes de inventario para productos con variación
            var detailsWithVariance = count.Details
                .Where(d => d.Variance.HasValue && d.Variance.Value != 0)
                .Where(d => request.Dto.ExcludeDetailIds == null || !request.Dto.ExcludeDetailIds.Contains(d.Id))
                .ToList();

            if (detailsWithVariance.Any())
            {
                // Generar código de ajuste
                var adjustmentCode = await _codeGenerator.GenerateNextCodeAsync("ADJ", "StockAdjustments");

                // Crear ajuste de inventario automático
                var adjustment = new StockAdjustment
                {
                    Code = adjustmentCode,
                    WarehouseId = count.WarehouseId,
                    AdjustmentDate = DateTime.UtcNow,
                    Reason = AdjustmentReason.PHYSICAL_COUNT,
                    Notes = $"Ajuste automático generado por conteo {count.Code}. {request.Dto.ApprovalNotes}",
                    CreatedByUserId = request.ApprovedByUserId,
                    CreatedAt = DateTime.UtcNow,
                    CompanyId = count.CompanyId
                };

                _context.StockAdjustments.Add(adjustment);
                await _context.SaveChangesAsync(cancellationToken);

                // Crear detalles y movimientos de kardex
                foreach (var detail in detailsWithVariance)
                {
                    // Crear detalle del ajuste
                    var adjustmentDetail = new StockAdjustmentDetail
                    {
                        StockAdjustmentId = adjustment.Id,
                        ProductId = detail.ProductId,
                        SystemQuantity = detail.SystemQuantity,
                        PhysicalQuantity = detail.PhysicalQuantity!.Value,
                        AdjustmentQuantity = detail.Variance!.Value,
                        UnitCost = detail.UnitCost,
                        TotalCost = detail.VarianceCost,
                        Notes = $"Conteo: {count.Code}. {detail.Notes}"
                    };

                    _context.StockAdjustmentDetails.Add(adjustmentDetail);

                    // Obtener stock actual
                    var productStock = await _context.ProductStock
                        .FirstOrDefaultAsync(ps => ps.ProductId == detail.ProductId && ps.WarehouseId == count.WarehouseId, cancellationToken);

                    var stockBefore = productStock?.Quantity ?? 0;
                    var stockAfter = stockBefore + detail.Variance!.Value;

                    // Generar código del movimiento
                    var movementCode = await _codeGenerator.GenerateNextCodeAsync("MOV", "InventoryMovements");

                    // Crear movimiento de kardex
                    var movementType = detail.Variance.Value > 0 ? "IN/ADJUSTMENT" : "OUT/ADJUSTMENT";

                    var movement = new InventoryMovement
                    {
                        Code = movementCode,
                        ProductId = detail.ProductId,
                        WarehouseId = count.WarehouseId,
                        MovementType = movementType,
                        Quantity = detail.Variance.Value,
                        StockBefore = stockBefore,
                        StockAfter = stockAfter,
                        UnitCost = detail.UnitCost,
                        TotalCost = detail.VarianceCost,
                        MovementDate = DateTime.UtcNow,
                        ReferenceDocumentType = "INVENTORY_COUNT",
                        ReferenceDocumentId = count.Id,
                        Notes = $"Ajuste por conteo {count.Code} - {adjustmentCode}",
                        CreatedByUserId = request.ApprovedByUserId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.InventoryMovements.Add(movement);

                    // Actualizar stock
                    if (productStock == null)
                    {
                        productStock = new ProductStock
                        {
                            ProductId = detail.ProductId,
                            WarehouseId = count.WarehouseId,
                            Quantity = detail.Variance!.Value,
                            ReservedQuantity = 0,
                            AverageCost = detail.UnitCost,
                            LastMovementDate = DateTime.UtcNow
                        };
                        _context.ProductStock.Add(productStock);
                    }
                    else
                    {
                        productStock.Quantity += detail.Variance!.Value;
                        productStock.LastMovementDate = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
            }

            // Completar el conteo
            count.Status = CountStatus.COMPLETED;
            count.CompletedAt = DateTime.UtcNow;
            count.ApprovedByUserId = request.ApprovedByUserId;
            count.ApprovedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(request.Dto.ApprovalNotes))
            {
                count.Notes = string.IsNullOrEmpty(count.Notes)
                    ? request.Dto.ApprovalNotes
                    : count.Notes + "\n\n" + request.Dto.ApprovalNotes;
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Cargar y retornar respuesta completa
            var approvedBy = await _context.User.FindAsync(request.ApprovedByUserId);

            return new InventoryCountResponseDto
            {
                Id = count.Id,
                Code = count.Code,
                WarehouseId = count.WarehouseId,
                WarehouseName = count.Warehouse.Name,
                CountType = count.CountType,
                CountTypeLabel = CountType.GetLabel(count.CountType),
                Status = count.Status,
                StatusLabel = CountStatus.GetLabel(count.Status),
                ScheduledDate = count.ScheduledDate,
                StartedAt = count.StartedAt,
                CompletedAt = count.CompletedAt,
                AssignedToUserId = count.AssignedToUserId,
                AssignedToUserName = count.AssignedToUser.Name,
                ApprovedByUserId = count.ApprovedByUserId,
                ApprovedByUserName = approvedBy?.Name,
                ApprovedAt = count.ApprovedAt,
                Notes = count.Notes,
                TotalProducts = count.TotalProducts,
                CountedProducts = count.CountedProducts,
                PendingProducts = 0,
                ProductsWithVariance = count.ProductsWithVariance,
                ProgressPercentage = 100,
                TotalVarianceCost = count.TotalVarianceCost,
                CreatedByUserName = count.CreatedByUser.Name,
                CreatedAt = count.CreatedAt,
                Details = count.Details.Select(d => new InventoryCountDetailResponseDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductCode = d.Product.code,
                    ProductName = d.Product.name,
                    SystemQuantity = d.SystemQuantity,
                    PhysicalQuantity = d.PhysicalQuantity,
                    Variance = d.Variance,
                    VariancePercentage = d.VariancePercentage,
                    VarianceCost = d.VarianceCost,
                    UnitCost = d.UnitCost,
                    Status = d.Status,
                    StatusLabel = CountDetailStatus.GetLabel(d.Status),
                    Notes = d.Notes
                }).ToList()
            };
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CANCELAR CONTEO
    // ═══════════════════════════════════════════════════════════════════════════

    public class CancelInventoryCountCommandHandler :
        IRequestHandler<CancelInventoryCountCommand, Unit>
    {
        private readonly POSDbContext _context;

        public CancelInventoryCountCommandHandler(POSDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(CancelInventoryCountCommand request, CancellationToken cancellationToken)
        {
            var count = await _context.InventoryCounts
                .FirstOrDefaultAsync(c => c.Id == request.CountId, cancellationToken);

            if (count == null)
                throw new KeyNotFoundException($"Conteo con ID {request.CountId} no encontrado");

            if (count.Status == CountStatus.COMPLETED)
                throw new InvalidOperationException("No se pueden cancelar conteos completados");

            count.Status = CountStatus.CANCELLED;

            if (!string.IsNullOrEmpty(request.Reason))
            {
                count.Notes = string.IsNullOrEmpty(count.Notes)
                    ? $"Cancelado: {request.Reason}"
                    : count.Notes + $"\n\nCancelado: {request.Reason}";
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
