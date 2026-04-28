using Application.Core.Inventory.Queries;
using Application.DTOs.Inventory;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.QueryHandlers
{
    public class StockAdjustmentQueryHandlers :
        IRequestHandler<GetStockAdjustmentsQuery, PagedStockAdjustmentsResponseDto>,
        IRequestHandler<GetStockAdjustmentByIdQuery, StockAdjustmentResponseDto>
    {
        private readonly POSDbContext _context;

        public StockAdjustmentQueryHandlers(POSDbContext context)
        {
            _context = context;
        }

        public async Task<PagedStockAdjustmentsResponseDto> Handle(GetStockAdjustmentsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.StockAdjustments
                .Include(a => a.Warehouse)
                .Include(a => a.CreatedByUser)
                .Include(a => a.Details)
                .AsQueryable();

            // Filtros
            if (request.WarehouseId.HasValue)
                query = query.Where(a => a.WarehouseId == request.WarehouseId.Value);

            if (!string.IsNullOrWhiteSpace(request.Reason))
                query = query.Where(a => a.Reason == request.Reason);

            if (request.FromDate.HasValue)
                query = query.Where(a => a.AdjustmentDate >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                query = query.Where(a => a.AdjustmentDate <= request.ToDate.Value);

            if (request.CompanyId.HasValue)
                query = query.Where(a => a.CompanyId == request.CompanyId.Value);

            // Contar total de registros
            var totalRecords = await query.CountAsync(cancellationToken);

            // Ordenar y paginar
            var adjustments = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Mapear a DTOs
            var items = adjustments.Select(a => new StockAdjustmentSummaryDto
            {
                Id = a.Id,
                Code = a.Code,
                WarehouseName = a.Warehouse.Name,
                AdjustmentDate = a.AdjustmentDate,
                Reason = a.Reason,
                ReasonLabel = AdjustmentReason.GetLabel(a.Reason),
                CreatedByUserName = a.CreatedByUser.Name,
                TotalProducts = a.Details.Count,
                TotalAdjustmentCost = a.Details.Sum(d => d.TotalCost ?? 0),
                CreatedAt = a.CreatedAt
            }).ToList();

            return new PagedStockAdjustmentsResponseDto
            {
                Items = items,
                TotalRecords = totalRecords,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize)
            };
        }

        public async Task<StockAdjustmentResponseDto> Handle(GetStockAdjustmentByIdQuery request, CancellationToken cancellationToken)
        {
            var adjustment = await _context.StockAdjustments
                .Include(a => a.Warehouse)
                .Include(a => a.CreatedByUser)
                .Include(a => a.Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (adjustment == null)
                throw new KeyNotFoundException($"Ajuste de inventario con ID {request.Id} no encontrado");

            return new StockAdjustmentResponseDto
            {
                Id = adjustment.Id,
                Code = adjustment.Code,
                WarehouseId = adjustment.WarehouseId,
                WarehouseName = adjustment.Warehouse.Name,
                AdjustmentDate = adjustment.AdjustmentDate,
                Reason = adjustment.Reason,
                ReasonLabel = AdjustmentReason.GetLabel(adjustment.Reason),
                Notes = adjustment.Notes,
                CreatedByUserName = adjustment.CreatedByUser.Name,
                CreatedAt = adjustment.CreatedAt,
                TotalProducts = adjustment.Details.Count,
                TotalAdjustmentCost = adjustment.Details.Sum(d => d.TotalCost ?? 0),
                Details = adjustment.Details.Select(d => new StockAdjustmentDetailResponseDto
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
