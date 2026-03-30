using Application.Core.Inventory.Queries;
using Application.DTOs.Inventory;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.QueryHandlers
{
    public class GetProductStockQueryHandler : IRequestHandler<GetProductStockQuery, PagedProductStockResponseDto>
    {
        private readonly POSDbContext _context;

        public GetProductStockQueryHandler(POSDbContext context)
        {
            _context = context;
        }

        public async Task<PagedProductStockResponseDto> Handle(GetProductStockQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ProductStock
                .AsNoTracking()
                .Include(ps => ps.Product)
                .Include(ps => ps.Warehouse)
                    .ThenInclude(w => w.Branch)
                .AsQueryable();

            if (request.ProductId.HasValue)
                query = query.Where(ps => ps.ProductId == request.ProductId.Value);

            if (!string.IsNullOrWhiteSpace(request.ProductSearch))
                query = query.Where(ps =>
                    ps.Product.name.Contains(request.ProductSearch) ||
                    ps.Product.code.Contains(request.ProductSearch));

            if (request.WarehouseId.HasValue)
                query = query.Where(ps => ps.WarehouseId == request.WarehouseId.Value);

            if (request.CompanyId.HasValue)
                query = query.Where(ps => ps.Warehouse.Branch.CompanyId == request.CompanyId.Value);

            var total = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(ps => ps.Product.name)
                .ThenBy(ps => ps.Warehouse.Name)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(ps => new ProductStockDto
                {
                    ProductStockId = ps.Id,
                    ProductId = ps.ProductId,
                    ProductCode = ps.Product.code,
                    ProductName = ps.Product.name,
                    WarehouseId = ps.WarehouseId,
                    WarehouseCode = ps.Warehouse.Code,
                    WarehouseName = ps.Warehouse.Name,
                    Quantity = ps.Quantity,
                    ReservedQuantity = ps.ReservedQuantity,
                    AvailableQuantity = ps.AvailableQuantity,
                    AverageCost = ps.AverageCost,
                    MinimumStock = ps.MinimumStock,
                    MaximumStock = ps.MaximumStock,
                    LastMovementDate = ps.LastMovementDate
                })
                .ToListAsync(cancellationToken);

            return new PagedProductStockResponseDto
            {
                Items = items,
                TotalRecords = total,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
            };
        }
    }

    public class GetProductStockByWarehouseQueryHandler : IRequestHandler<GetProductStockByWarehouseQuery, List<ProductStockDto>>
    {
        private readonly POSDbContext _context;

        public GetProductStockByWarehouseQueryHandler(POSDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductStockDto>> Handle(GetProductStockByWarehouseQuery request, CancellationToken cancellationToken)
        {
            return await _context.ProductStock
                .AsNoTracking()
                .Include(ps => ps.Product)
                .Include(ps => ps.Warehouse)
                .Where(ps => ps.WarehouseId == request.WarehouseId)
                .OrderBy(ps => ps.Product.name)
                .Select(ps => new ProductStockDto
                {
                    ProductStockId = ps.Id,
                    ProductId = ps.ProductId,
                    ProductCode = ps.Product.code,
                    ProductName = ps.Product.name,
                    WarehouseId = ps.WarehouseId,
                    WarehouseCode = ps.Warehouse.Code,
                    WarehouseName = ps.Warehouse.Name,
                    Quantity = ps.Quantity,
                    ReservedQuantity = ps.ReservedQuantity,
                    AvailableQuantity = ps.AvailableQuantity,
                    AverageCost = ps.AverageCost,
                    MinimumStock = ps.MinimumStock,
                    MaximumStock = ps.MaximumStock,
                    LastMovementDate = ps.LastMovementDate
                })
                .ToListAsync(cancellationToken);
        }
    }
}
