using Application.Abstractions.Catalogue;
using Application.Core.ProductPrice.Commands;
using Application.Core.ProductPrice.Queries;
using Application.DTOs.PriceList;
using MediatR;

namespace Application.Core.ProductPrice.CommandHandlers
{
    // ─────────────────────────────────────────────
    // UPSERT (Create-or-Update por (ProductId, PriceListId))
    // ─────────────────────────────────────────────
    public class UpsertProductPriceCommandHandler : IRequestHandler<UpsertProductPriceCommand, ProductPriceDto>
    {
        private readonly IProductPriceRepository _repo;

        public UpsertProductPriceCommandHandler(IProductPriceRepository repo) => _repo = repo;

        public async Task<ProductPriceDto> Handle(UpsertProductPriceCommand request, CancellationToken cancellationToken)
        {
            var d = request.Data;

            if (!await _repo.ProductExistsAsync(d.ProductId))
                throw new InvalidOperationException($"El producto con ID {d.ProductId} no existe");

            if (!await _repo.PriceListExistsAndActiveAsync(d.PriceListId))
                throw new InvalidOperationException($"La lista de precios con ID {d.PriceListId} no existe o está inactiva");

            var existing = await _repo.GetByProductAndListAsync(d.ProductId, d.PriceListId, includeInactive: true);

            Domain.Entities.ProductPrice saved;
            if (existing is null)
            {
                saved = await _repo.CreateAsync(new Domain.Entities.ProductPrice
                {
                    ProductId = d.ProductId,
                    PriceListId = d.PriceListId,
                    Price = d.Price,
                    DiscountPercentage = d.DiscountPercentage,
                    ValidFrom = DateTime.UtcNow,
                    ValidTo = null,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = request.UserId
                });
            }
            else
            {
                existing.Price = d.Price;
                existing.DiscountPercentage = d.DiscountPercentage;
                existing.IsActive = true; // reactivar al upsert
                saved = await _repo.UpdateAsync(existing);
            }

            var reloaded = await _repo.GetByIdAsync(saved.Id);
            return ProductPriceMapper.Map(reloaded!);
        }
    }

    // ─────────────────────────────────────────────
    // UPDATE por ID
    // ─────────────────────────────────────────────
    public class UpdateProductPriceCommandHandler : IRequestHandler<UpdateProductPriceCommand, ProductPriceDto>
    {
        private readonly IProductPriceRepository _repo;

        public UpdateProductPriceCommandHandler(IProductPriceRepository repo) => _repo = repo;

        public async Task<ProductPriceDto> Handle(UpdateProductPriceCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repo.GetByIdAsync(request.Id)
                ?? throw new KeyNotFoundException($"Precio con ID {request.Id} no encontrado");

            var d = request.Data;
            entity.Price = d.Price;
            entity.DiscountPercentage = d.DiscountPercentage;
            entity.IsActive = d.IsActive;

            var updated = await _repo.UpdateAsync(entity);
            var reloaded = await _repo.GetByIdAsync(updated.Id);
            return ProductPriceMapper.Map(reloaded!);
        }
    }

    // ─────────────────────────────────────────────
    // DELETE (soft)
    // ─────────────────────────────────────────────
    public class DeleteProductPriceCommandHandler : IRequestHandler<DeleteProductPriceCommand, bool>
    {
        private readonly IProductPriceRepository _repo;

        public DeleteProductPriceCommandHandler(IProductPriceRepository repo) => _repo = repo;

        public async Task<bool> Handle(DeleteProductPriceCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repo.GetByIdAsync(request.Id)
                ?? throw new KeyNotFoundException($"Precio con ID {request.Id} no encontrado");

            return await _repo.SetActiveAsync(entity.Id, false);
        }
    }

    // ─────────────────────────────────────────────
    // BULK UPSERT (varios productos en una lista)
    // ─────────────────────────────────────────────
    public class BulkUpsertProductPricesCommandHandler : IRequestHandler<BulkUpsertProductPricesCommand, BulkProductPricesResultDto>
    {
        private readonly IProductPriceRepository _repo;

        public BulkUpsertProductPricesCommandHandler(IProductPriceRepository repo) => _repo = repo;

        public async Task<BulkProductPricesResultDto> Handle(BulkUpsertProductPricesCommand request, CancellationToken cancellationToken)
        {
            var d = request.Data;

            if (!await _repo.PriceListExistsAndActiveAsync(d.PriceListId))
                throw new InvalidOperationException($"La lista de precios con ID {d.PriceListId} no existe o está inactiva");

            var validItems = new List<(int ProductId, decimal Price, decimal DiscountPercentage)>();
            var skipped = new List<int>();

            foreach (var item in d.Items)
            {
                if (!await _repo.ProductExistsAsync(item.ProductId))
                {
                    skipped.Add(item.ProductId);
                    continue;
                }

                validItems.Add((item.ProductId, item.Price, item.DiscountPercentage));
            }

            var (inserted, updated) = await _repo.BulkUpsertAsync(d.PriceListId, validItems, request.UserId);

            return new BulkProductPricesResultDto
            {
                PriceListId = d.PriceListId,
                TotalReceived = d.Items.Count,
                Inserted = inserted,
                Updated = updated,
                SkippedProductIds = skipped
            };
        }
    }

    // ─────────────────────────────────────────────
    // QUERIES
    // ─────────────────────────────────────────────
    public class GetProductPricesQueryHandler : IRequestHandler<GetProductPricesQuery, List<ProductPriceDto>>
    {
        private readonly IProductPriceRepository _repo;
        public GetProductPricesQueryHandler(IProductPriceRepository repo) => _repo = repo;

        public async Task<List<ProductPriceDto>> Handle(GetProductPricesQuery request, CancellationToken cancellationToken)
        {
            var list = await _repo.ListAsync(request.ProductId, request.PriceListId, request.OnlyActive);
            return list.Select(ProductPriceMapper.Map).ToList();
        }
    }

    public class GetProductPriceByIdQueryHandler : IRequestHandler<GetProductPriceByIdQuery, ProductPriceDto?>
    {
        private readonly IProductPriceRepository _repo;
        public GetProductPriceByIdQueryHandler(IProductPriceRepository repo) => _repo = repo;

        public async Task<ProductPriceDto?> Handle(GetProductPriceByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _repo.GetByIdAsync(request.Id);
            return entity is null ? null : ProductPriceMapper.Map(entity);
        }
    }

    public class GetPricesByProductQueryHandler : IRequestHandler<GetPricesByProductQuery, List<ProductPriceDto>>
    {
        private readonly IProductPriceRepository _repo;
        public GetPricesByProductQueryHandler(IProductPriceRepository repo) => _repo = repo;

        public async Task<List<ProductPriceDto>> Handle(GetPricesByProductQuery request, CancellationToken cancellationToken)
        {
            var list = await _repo.GetByProductAsync(request.ProductId, request.OnlyActive);
            return list.Select(ProductPriceMapper.Map).ToList();
        }
    }

    public class GetPricesByPriceListQueryHandler : IRequestHandler<GetPricesByPriceListQuery, List<ProductPriceDto>>
    {
        private readonly IProductPriceRepository _repo;
        public GetPricesByPriceListQueryHandler(IProductPriceRepository repo) => _repo = repo;

        public async Task<List<ProductPriceDto>> Handle(GetPricesByPriceListQuery request, CancellationToken cancellationToken)
        {
            var list = await _repo.GetByPriceListAsync(request.PriceListId, request.OnlyActive);
            return list.Select(ProductPriceMapper.Map).ToList();
        }
    }

    // ─────────────────────────────────────────────
    // MAPPER
    // ─────────────────────────────────────────────
    internal static class ProductPriceMapper
    {
        internal static ProductPriceDto Map(Domain.Entities.ProductPrice p) => new()
        {
            Id = p.Id,
            ProductId = p.ProductId,
            ProductName = p.Product?.name,
            ProductCode = p.Product?.code,
            PriceListId = p.PriceListId,
            PriceListName = p.PriceList?.Name,
            PriceListCode = p.PriceList?.Code,
            Price = p.Price,
            DiscountPercentage = p.DiscountPercentage,
            FinalPrice = p.Price * (1 - p.DiscountPercentage / 100m),
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt,
            CreatedByName = p.CreatedBy?.Name
        };
    }
}
