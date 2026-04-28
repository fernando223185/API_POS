using Application.Core.Inventory.Queries;
using Application.DTOs.Inventory;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.QueryHandlers
{
    // ═══════════════════════════════════════════════════════════════════════════
    // OBTENER LISTADO PAGINADO DE CONTEOS
    // ═══════════════════════════════════════════════════════════════════════════

    public class GetInventoryCountsQueryHandler :
        IRequestHandler<GetInventoryCountsQuery, PagedInventoryCountsResponseDto>
    {
        private readonly POSDbContext _context;

        public GetInventoryCountsQueryHandler(POSDbContext context)
        {
            _context = context;
        }

        public async Task<PagedInventoryCountsResponseDto> Handle(GetInventoryCountsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.InventoryCounts
                .Include(c => c.Warehouse)
                .Include(c => c.AssignedToUser)
                .AsQueryable();

            // Filtros
            if (request.WarehouseId.HasValue)
                query = query.Where(c => c.WarehouseId == request.WarehouseId.Value);

            if (!string.IsNullOrEmpty(request.CountType))
                query = query.Where(c => c.CountType == request.CountType);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(c => c.Status == request.Status);

            if (request.FromDate.HasValue)
                query = query.Where(c => c.ScheduledDate >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                query = query.Where(c => c.ScheduledDate <= request.ToDate.Value);

            if (request.AssignedToUserId.HasValue)
                query = query.Where(c => c.AssignedToUserId == request.AssignedToUserId.Value);

            if (request.CompanyId.HasValue)
                query = query.Where(c => c.CompanyId == request.CompanyId.Value);

            // Contar total
            var totalRecords = await query.CountAsync(cancellationToken);

            // Paginación
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new InventoryCountSummaryDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    WarehouseName = c.Warehouse != null ? c.Warehouse.Name : "N/A",
                    CountType = c.CountType,
                    CountTypeLabel = CountType.GetLabel(c.CountType),
                    Status = c.Status,
                    StatusLabel = CountStatus.GetLabel(c.Status),
                    ScheduledDate = c.ScheduledDate,
                    StartedAt = c.StartedAt,
                    CompletedAt = c.CompletedAt,
                    AssignedToUserName = c.AssignedToUser != null ? c.AssignedToUser.Name : "N/A",
                    TotalProducts = c.TotalProducts,
                    CountedProducts = c.CountedProducts,
                    ProgressPercentage = c.TotalProducts > 0
                        ? (decimal)c.CountedProducts / c.TotalProducts * 100
                        : 0,
                    ProductsWithVariance = c.ProductsWithVariance,
                    TotalVarianceCost = c.TotalVarianceCost,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return new PagedInventoryCountsResponseDto
            {
                Items = items,
                TotalRecords = totalRecords,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize)
            };
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // OBTENER DETALLE COMPLETO DE UN CONTEO
    // ═══════════════════════════════════════════════════════════════════════════

    public class GetInventoryCountByIdQueryHandler :
        IRequestHandler<GetInventoryCountByIdQuery, InventoryCountResponseDto>
    {
        private readonly POSDbContext _context;

        public GetInventoryCountByIdQueryHandler(POSDbContext context)
        {
            _context = context;
        }

        public async Task<InventoryCountResponseDto> Handle(GetInventoryCountByIdQuery request, CancellationToken cancellationToken)
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
                .FirstOrDefaultAsync(c => c.Id == request.CountId, cancellationToken);

            if (count == null)
                throw new KeyNotFoundException($"Conteo con ID {request.CountId} no encontrado");

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
    // OBTENER PRODUCTOS PENDIENTES DE CONTAR
    // ═══════════════════════════════════════════════════════════════════════════

    public class GetPendingCountDetailsQueryHandler :
        IRequestHandler<GetPendingCountDetailsQuery, List<InventoryCountDetailResponseDto>>
    {
        private readonly POSDbContext _context;

        public GetPendingCountDetailsQueryHandler(POSDbContext context)
        {
            _context = context;
        }

        public async Task<List<InventoryCountDetailResponseDto>> Handle(GetPendingCountDetailsQuery request, CancellationToken cancellationToken)
        {
            var details = await _context.InventoryCountDetails
                .Include(d => d.Product)
                .Where(d => d.InventoryCountId == request.CountId && d.Status == CountDetailStatus.PENDING)
                .Select(d => new InventoryCountDetailResponseDto
                {
                    Id = d.Id,
                    InventoryCountId = d.InventoryCountId,
                    ProductId = d.ProductId,
                    ProductCode = d.Product != null ? d.Product.code : "N/A",
                    ProductName = d.Product != null ? d.Product.name : "N/A",
                    SystemQuantity = d.SystemQuantity,
                    PhysicalQuantity = d.PhysicalQuantity,
                    Variance = d.Variance,
                    VariancePercentage = d.VariancePercentage,
                    VarianceCost = d.VarianceCost,
                    UnitCost = d.UnitCost,
                    Status = d.Status,
                    StatusLabel = CountDetailStatus.GetLabel(d.Status),
                    Notes = d.Notes
                })
                .ToListAsync(cancellationToken);

            return details;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // OBTENER PRODUCTOS CON DISCREPANCIAS
    // ═══════════════════════════════════════════════════════════════════════════

    public class GetCountDetailsWithVarianceQueryHandler :
        IRequestHandler<GetCountDetailsWithVarianceQuery, List<InventoryCountDetailResponseDto>>
    {
        private readonly POSDbContext _context;

        public GetCountDetailsWithVarianceQueryHandler(POSDbContext context)
        {
            _context = context;
        }

        public async Task<List<InventoryCountDetailResponseDto>> Handle(GetCountDetailsWithVarianceQuery request, CancellationToken cancellationToken)
        {
            var details = await _context.InventoryCountDetails
                .Include(d => d.Product)
                .Include(d => d.CountedByUser)
                .Where(d => d.InventoryCountId == request.CountId && d.Variance.HasValue && d.Variance.Value != 0)
                .Select(d => new InventoryCountDetailResponseDto
                {
                    Id = d.Id,
                    InventoryCountId = d.InventoryCountId,
                    ProductId = d.ProductId,
                    ProductCode = d.Product != null ? d.Product.code : "N/A",
                    ProductName = d.Product != null ? d.Product.name : "N/A",
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
                    CountedByUserName = d.CountedByUser != null ? d.CountedByUser.Name : null,
                    CountedAt = d.CountedAt
                })
                .OrderByDescending(d => Math.Abs(d.VariancePercentage ?? 0))
                .ToListAsync(cancellationToken);

            return details;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // OBTENER ESTADÍSTICAS DE UN CONTEO
    // ═══════════════════════════════════════════════════════════════════════════

    public class GetCountStatisticsQueryHandler :
        IRequestHandler<GetCountStatisticsQuery, CountStatisticsDto>
    {
        private readonly POSDbContext _context;

        public GetCountStatisticsQueryHandler(POSDbContext context)
        {
            _context = context;
        }

        public async Task<CountStatisticsDto> Handle(GetCountStatisticsQuery request, CancellationToken cancellationToken)
        {
            var count = await _context.InventoryCounts
                .Include(c => c.Details)
                .FirstOrDefaultAsync(c => c.Id == request.CountId, cancellationToken);

            if (count == null)
                throw new KeyNotFoundException($"Conteo con ID {request.CountId} no encontrado");

            var details = count.Details.Where(d => d.Status != CountDetailStatus.PENDING).ToList();

            var totalProducts = count.TotalProducts;
            var countedProducts = count.CountedProducts;
            var pendingProducts = totalProducts - countedProducts;
            var progressPercentage = totalProducts > 0
                ? (decimal)countedProducts / totalProducts * 100
                : 0;

            var productsWithPositiveVariance = details.Count(d => d.Variance.HasValue && d.Variance.Value > 0);
            var productsWithNegativeVariance = details.Count(d => d.Variance.HasValue && d.Variance.Value < 0);
            var productsWithoutVariance = details.Count(d => d.Variance.HasValue && d.Variance.Value == 0);

            var totalPositiveVarianceCost = details
                .Where(d => d.VarianceCost.HasValue && d.VarianceCost.Value > 0)
                .Sum(d => d.VarianceCost!.Value);

            var totalNegativeVarianceCost = details
                .Where(d => d.VarianceCost.HasValue && d.VarianceCost.Value < 0)
                .Sum(d => d.VarianceCost!.Value);

            var netVarianceCost = totalPositiveVarianceCost + totalNegativeVarianceCost;

            var averageVariancePercentage = details.Any(d => d.VariancePercentage.HasValue)
                ? details.Where(d => d.VariancePercentage.HasValue).Average(d => Math.Abs(d.VariancePercentage!.Value))
                : 0;

            var inventoryAccuracyPercentage = countedProducts > 0
                ? (decimal)productsWithoutVariance / countedProducts * 100
                : 0;

            return new CountStatisticsDto
            {
                TotalProducts = totalProducts,
                CountedProducts = countedProducts,
                PendingProducts = pendingProducts,
                ProgressPercentage = progressPercentage,
                ProductsWithPositiveVariance = productsWithPositiveVariance,
                ProductsWithNegativeVariance = productsWithNegativeVariance,
                ProductsWithoutVariance = productsWithoutVariance,
                TotalPositiveVarianceCost = totalPositiveVarianceCost,
                TotalNegativeVarianceCost = totalNegativeVarianceCost,
                NetVarianceCost = netVarianceCost,
                AverageVariancePercentage = averageVariancePercentage,
                InventoryAccuracyPercentage = inventoryAccuracyPercentage
            };
        }
    }
}
