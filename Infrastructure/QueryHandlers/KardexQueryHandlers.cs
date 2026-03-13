using Application.Core.Inventory.Queries;
using Application.DTOs.Inventory;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.QueryHandlers
{
    /// <summary>
    /// Handler para obtener el kardex de inventario
    /// </summary>
    public class GetKardexQueryHandler : IRequestHandler<GetKardexQuery, KardexResponseDto>
    {
        private readonly POSDbContext _context;

        public GetKardexQueryHandler(POSDbContext context)
        {
            _context = context;
        }

        public async Task<KardexResponseDto> Handle(GetKardexQuery request, CancellationToken cancellationToken)
        {
            // Query base
            var query = _context.InventoryMovements
                .Include(m => m.Product)
                .Include(m => m.Warehouse)
                .Include(m => m.CreatedBy) // ? Agregado
                .Include(m => m.PurchaseOrderReceiving)
                .Include(m => m.Sale)
                .AsQueryable();

            // Filtros
            if (request.ProductId.HasValue)
            {
                query = query.Where(m => m.ProductId == request.ProductId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.ProductSearch))
            {
                var search = request.ProductSearch.ToLower();
                query = query.Where(m =>
                    m.Product.code.ToLower().Contains(search) ||
                    m.Product.name.ToLower().Contains(search)
                    // InternalReference no existe en Product
                );
            }

            if (request.WarehouseId.HasValue)
            {
                query = query.Where(m => m.WarehouseId == request.WarehouseId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.MovementType))
            {
                query = query.Where(m => m.MovementType.StartsWith(request.MovementType));
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(m => m.MovementDate >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                var toDateEnd = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(m => m.MovementDate <= toDateEnd);
            }

            // Contar total
            var totalRecords = await query.CountAsync(cancellationToken);

            // Ordenar por fecha descendente (más recientes primero)
            query = query.OrderByDescending(m => m.MovementDate)
                        .ThenByDescending(m => m.Id);

            // Paginación
            var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);
            
            // Obtener datos de la base de datos SIN mapear MovementTypeName
            var rawMovements = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(m => new 
                {
                    m.Id,
                    m.Code,
                    m.MovementDate,
                    m.MovementType,
                    m.ProductId,
                    ProductCode = m.Product.code,
                    ProductName = m.Product.name,
                    m.WarehouseId,
                    WarehouseName = m.Warehouse != null ? m.Warehouse.Name : null,
                    WarehouseCode = m.Warehouse != null ? m.Warehouse.Code : null,
                    m.Quantity,
                    m.StockBefore,
                    m.StockAfter,
                    m.UnitCost,
                    m.TotalCost,
                    m.PurchaseOrderReceivingId,
                    PurchaseOrderReceivingCode = m.PurchaseOrderReceiving != null ? m.PurchaseOrderReceiving.Code : null,
                    m.SaleId,
                    SaleCode = m.Sale != null ? m.Sale.Code : null,
                    m.Notes,
                    CreatedByUserName = m.CreatedBy != null ? m.CreatedBy.Name : "Sistema"
                })
                .ToListAsync(cancellationToken);

            // Mapear a DTO después de traer los datos (en memoria)
            var movements = rawMovements.Select(m => new KardexMovementDto
            {
                Id = m.Id,
                MovementCode = m.Code,
                MovementDate = m.MovementDate,
                MovementType = m.MovementType,
                MovementTypeName = FormatMovementType(m.MovementType), // ? Ahora en memoria
                ProductId = m.ProductId,
                ProductCode = m.ProductCode,
                ProductName = m.ProductName,
                WarehouseId = m.WarehouseId,
                WarehouseName = m.WarehouseName,
                WarehouseCode = m.WarehouseCode,
                Quantity = m.Quantity,
                StockBefore = m.StockBefore,
                StockAfter = m.StockAfter,
                UnitCost = m.UnitCost,
                TotalCost = m.TotalCost,
                PurchaseOrderReceivingId = m.PurchaseOrderReceivingId,
                PurchaseOrderReceivingCode = m.PurchaseOrderReceivingCode,
                SaleId = m.SaleId,
                SaleCode = m.SaleCode,
                Notes = m.Notes,
                CreatedByUserName = m.CreatedByUserName
            }).ToList();

            // Obtener estadísticas
            var statistics = await GetStatistics(request, cancellationToken);

            return new KardexResponseDto
            {
                Message = "Kardex obtenido exitosamente",
                Error = 0,
                Data = movements,
                Statistics = statistics,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }

        private async Task<KardexStatisticsDto> GetStatistics(GetKardexQuery request, CancellationToken cancellationToken)
        {
            var query = _context.InventoryMovements.AsQueryable();

            // Aplicar filtros
            if (request.ProductId.HasValue)
            {
                query = query.Where(m => m.ProductId == request.ProductId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.ProductSearch))
            {
                var search = request.ProductSearch.ToLower();
                query = query.Where(m =>
                    m.Product.code.ToLower().Contains(search) ||
                    m.Product.name.ToLower().Contains(search)
                );
            }

            if (request.WarehouseId.HasValue)
            {
                query = query.Where(m => m.WarehouseId == request.WarehouseId.Value);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(m => m.MovementDate >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                var toDateEnd = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(m => m.MovementDate <= toDateEnd);
            }

            // Calcular estadísticas
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var totalMovements = await query.CountAsync(cancellationToken);

            var entriesToday = await query
                .Where(m => m.MovementDate >= today && m.MovementDate < tomorrow && m.MovementType.StartsWith("IN"))
                .CountAsync(cancellationToken);

            var exitsToday = await query
                .Where(m => m.MovementDate >= today && m.MovementDate < tomorrow && m.MovementType.StartsWith("OUT"))
                .CountAsync(cancellationToken);

            var entries = await query
                .Where(m => m.MovementType.StartsWith("IN"))
                .GroupBy(m => 1)
                .Select(g => new
                {
                    Quantity = g.Sum(m => m.Quantity),
                    Value = g.Sum(m => m.TotalCost ?? 0)
                })
                .FirstOrDefaultAsync(cancellationToken);

            var exits = await query
                .Where(m => m.MovementType.StartsWith("OUT"))
                .GroupBy(m => 1)
                .Select(g => new
                {
                    Quantity = g.Sum(m => m.Quantity),
                    Value = g.Sum(m => m.TotalCost ?? 0)
                })
                .FirstOrDefaultAsync(cancellationToken);

            var totalValue = await query.SumAsync(m => m.TotalCost ?? 0, cancellationToken);

            return new KardexStatisticsDto
            {
                TotalMovements = totalMovements,
                EntriesToday = entriesToday,
                ExitsToday = exitsToday,
                TotalValue = totalValue,
                TotalEntriesQuantity = entries?.Quantity ?? 0,
                TotalExitsQuantity = exits?.Quantity ?? 0,
                TotalEntriesValue = entries?.Value ?? 0,
                TotalExitsValue = exits?.Value ?? 0
            };
        }

        private static string FormatMovementType(string type)
        {
            return type switch
            {
                "IN/PURCHASE" => "Entrada - Compra",
                "IN/ADJUSTMENT" => "Entrada - Ajuste",
                "IN/TRANSFER" => "Entrada - Traspaso",
                "IN/RETURN" => "Entrada - Devolución",
                "OUT/SALE" => "Salida - Venta",
                "OUT/ADJUSTMENT" => "Salida - Ajuste",
                "OUT/TRANSFER" => "Salida - Traspaso",
                "OUT/RETURN" => "Salida - Devolución",
                _ => type
            };
        }
    }

    /// <summary>
    /// Handler para obtener estadísticas del kardex
    /// </summary>
    public class GetKardexStatisticsQueryHandler : IRequestHandler<GetKardexStatisticsQuery, KardexStatisticsDto>
    {
        private readonly POSDbContext _context;

        public GetKardexStatisticsQueryHandler(POSDbContext context)
        {
            _context = context;
        }

        public async Task<KardexStatisticsDto> Handle(GetKardexStatisticsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.InventoryMovements.AsQueryable();

            // Aplicar filtros
            if (request.ProductId.HasValue)
            {
                query = query.Where(m => m.ProductId == request.ProductId.Value);
            }

            if (request.WarehouseId.HasValue)
            {
                query = query.Where(m => m.WarehouseId == request.WarehouseId.Value);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(m => m.MovementDate >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                var toDateEnd = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(m => m.MovementDate <= toDateEnd);
            }

            // Calcular estadísticas
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var totalMovements = await query.CountAsync(cancellationToken);

            var entriesToday = await query
                .Where(m => m.MovementDate >= today && m.MovementDate < tomorrow && m.MovementType.StartsWith("IN"))
                .CountAsync(cancellationToken);

            var exitsToday = await query
                .Where(m => m.MovementDate >= today && m.MovementDate < tomorrow && m.MovementType.StartsWith("OUT"))
                .CountAsync(cancellationToken);

            var entries = await query
                .Where(m => m.MovementType.StartsWith("IN"))
                .GroupBy(m => 1)
                .Select(g => new
                {
                    Quantity = g.Sum(m => m.Quantity),
                    Value = g.Sum(m => m.TotalCost ?? 0)
                })
                .FirstOrDefaultAsync(cancellationToken);

            var exits = await query
                .Where(m => m.MovementType.StartsWith("OUT"))
                .GroupBy(m => 1)
                .Select(g => new
                {
                    Quantity = g.Sum(m => m.Quantity),
                    Value = g.Sum(m => m.TotalCost ?? 0)
                })
                .FirstOrDefaultAsync(cancellationToken);

            var totalValue = await query.SumAsync(m => m.TotalCost ?? 0, cancellationToken);

            return new KardexStatisticsDto
            {
                TotalMovements = totalMovements,
                EntriesToday = entriesToday,
                ExitsToday = exitsToday,
                TotalValue = totalValue,
                TotalEntriesQuantity = entries?.Quantity ?? 0,
                TotalExitsQuantity = exits?.Quantity ?? 0,
                TotalEntriesValue = entries?.Value ?? 0,
                TotalExitsValue = exits?.Value ?? 0
            };
        }
    }
}
