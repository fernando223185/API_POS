using Application.Abstractions.Common;
using Application.Core.Inventory.Commands;
using Application.DTOs.Inventory;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.CommandHandlers
{
    public class StockAdjustmentCommandHandlers :
        IRequestHandler<CreateStockAdjustmentCommand, StockAdjustmentResponseDto>
    {
        private readonly POSDbContext _context;
        private readonly ICodeGeneratorService _codeGenerator;

        public StockAdjustmentCommandHandlers(
            POSDbContext context,
            ICodeGeneratorService codeGenerator)
        {
            _context = context;
            _codeGenerator = codeGenerator;
        }

        public async Task<StockAdjustmentResponseDto> Handle(CreateStockAdjustmentCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            // Validaciones
            if (dto.Details == null || dto.Details.Count == 0)
                throw new ArgumentException("El ajuste debe tener al menos un producto");

            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Id == dto.WarehouseId, cancellationToken);

            if (warehouse == null)
                throw new ArgumentException("Almacén no encontrado");

            // Validar que todos los productos existen
            var productIds = dto.Details.Select(d => d.ProductId).Distinct().ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.ID))
                .ToDictionaryAsync(p => p.ID, p => p, cancellationToken);

            if (products.Count != productIds.Count)
                throw new ArgumentException("Uno o más productos no encontrados");

            // Generar código único
            var lastAdjustment = await _context.StockAdjustments
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var newNumber = (lastAdjustment?.Id ?? 0) + 1;
            var code = $"ADJ-{newNumber:D6}";

            // Crear ajuste
            var adjustment = new StockAdjustment
            {
                Code = code,
                WarehouseId = dto.WarehouseId,
                AdjustmentDate = dto.AdjustmentDate,
                Reason = dto.Reason,
                Notes = dto.Notes,
                CreatedByUserId = request.UserId,
                CreatedAt = DateTime.UtcNow,
                CompanyId = request.CompanyId
            };

            _context.StockAdjustments.Add(adjustment);
            await _context.SaveChangesAsync(cancellationToken);

            // Crear detalles y movimientos de kardex
            decimal totalAdjustmentCost = 0;

            foreach (var detailDto in dto.Details)
            {
                // Calcular diferencia
                var adjustmentQuantity = detailDto.PhysicalQuantity - detailDto.SystemQuantity;

                if (adjustmentQuantity == 0)
                    continue; // No hay diferencia, omitir

                // Obtener stock actual y costo promedio
                var productStock = await _context.ProductStock
                    .FirstOrDefaultAsync(ps => ps.ProductId == detailDto.ProductId && ps.WarehouseId == dto.WarehouseId, cancellationToken);

                decimal? unitCost = productStock?.AverageCost;
                decimal? totalCost = unitCost.HasValue ? adjustmentQuantity * unitCost.Value : null;

                if (totalCost.HasValue)
                    totalAdjustmentCost += totalCost.Value;

                // Crear detalle del ajuste
                var detail = new StockAdjustmentDetail
                {
                    StockAdjustmentId = adjustment.Id,
                    ProductId = detailDto.ProductId,
                    SystemQuantity = detailDto.SystemQuantity,
                    PhysicalQuantity = detailDto.PhysicalQuantity,
                    AdjustmentQuantity = adjustmentQuantity,
                    UnitCost = unitCost,
                    TotalCost = totalCost,
                    Notes = detailDto.Notes
                };

                _context.StockAdjustmentDetails.Add(detail);

                // Obtener stock actual antes del ajuste
                var stockBefore = productStock?.Quantity ?? 0;
                var stockAfter = stockBefore + adjustmentQuantity;

                // Generar código del movimiento
                var movementCode = await _codeGenerator.GenerateNextCodeAsync("MOV", "InventoryMovements");

                // Crear movimiento de inventario (Kardex)
                var movementType = adjustmentQuantity > 0 ? "IN/ADJUSTMENT" : "OUT/ADJUSTMENT";

                var movement = new InventoryMovement
                {
                    Code = movementCode,
                    ProductId = detailDto.ProductId,
                    WarehouseId = dto.WarehouseId,
                    MovementType = movementType,
                    Quantity = adjustmentQuantity, // Positivo para entradas, negativo para salidas
                    StockBefore = stockBefore,
                    StockAfter = stockAfter,
                    UnitCost = unitCost,
                    TotalCost = totalCost,
                    MovementDate = dto.AdjustmentDate,
                    ReferenceDocumentType = "STOCK_ADJUSTMENT",
                    ReferenceDocumentId = adjustment.Id,
                    Notes = $"Ajuste: {AdjustmentReason.GetLabel(dto.Reason)} - {code}",
                    CreatedByUserId = request.UserId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.InventoryMovements.Add(movement);

                // Actualizar stock del producto
                if (productStock == null)
                {
                    // Crear registro de stock si no existe
                    productStock = new ProductStock
                    {
                        ProductId = detailDto.ProductId,
                        WarehouseId = dto.WarehouseId,
                        Quantity = adjustmentQuantity,
                        ReservedQuantity = 0,
                        AverageCost = unitCost,
                        LastMovementDate = dto.AdjustmentDate
                    };
                    _context.ProductStock.Add(productStock);
                }
                else
                {
                    // Actualizar stock existente
                    productStock.Quantity += adjustmentQuantity;
                    productStock.LastMovementDate = dto.AdjustmentDate;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Cargar el ajuste completo con sus relaciones
            var result = await _context.StockAdjustments
                .Include(a => a.Warehouse)
                .Include(a => a.CreatedByUser)
                .Include(a => a.Details)
                    .ThenInclude(d => d.Product)
                .FirstAsync(a => a.Id == adjustment.Id, cancellationToken);

            // Mapear a DTO de respuesta
            return new StockAdjustmentResponseDto
            {
                Id = result.Id,
                Code = result.Code,
                WarehouseId = result.WarehouseId,
                WarehouseName = result.Warehouse.Name,
                AdjustmentDate = result.AdjustmentDate,
                Reason = result.Reason,
                ReasonLabel = AdjustmentReason.GetLabel(result.Reason),
                Notes = result.Notes,
                CreatedByUserName = result.CreatedByUser.Name,
                CreatedAt = result.CreatedAt,
                TotalProducts = result.Details.Count,
                TotalAdjustmentCost = totalAdjustmentCost,
                Details = result.Details.Select(d => new StockAdjustmentDetailResponseDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductCode = d.Product.code,
                    ProductName = d.Product.name,
                    SystemQuantity = d.SystemQuantity,
                    PhysicalQuantity = d.PhysicalQuantity,
                    AdjustmentQuantity = d.AdjustmentQuantity,
                    UnitCost = d.UnitCost,
                    TotalCost = d.TotalCost,
                    Notes = d.Notes
                }).ToList()
            };
        }
    }
}
