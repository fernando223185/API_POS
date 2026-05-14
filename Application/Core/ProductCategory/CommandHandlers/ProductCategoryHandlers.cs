using Application.Abstractions.Catalogue;
using Application.Core.ProductCategory.Commands;
using Application.Core.ProductCategory.Queries;
using Application.DTOs.Product;
using MediatR;

namespace Application.Core.ProductCategory.CommandHandlers
{
    // ─────────────────────────────────────────────
    // CREATE
    // ─────────────────────────────────────────────
    public class CreateProductCategoryCommandHandler : IRequestHandler<CreateProductCategoryCommand, ProductCategoryDto>
    {
        private readonly IProductCategoryRepository _repo;

        public CreateProductCategoryCommandHandler(IProductCategoryRepository repo) => _repo = repo;

        public async Task<ProductCategoryDto> Handle(CreateProductCategoryCommand request, CancellationToken cancellationToken)
        {
            var d = request.Data;
            var name = d.Name.Trim();
            var code = d.Code.Trim().ToUpper();

            if (await _repo.CodeExistsAsync(code))
                throw new InvalidOperationException($"Ya existe una categoría con el código '{code}'");

            if (await _repo.NameExistsAsync(name))
                throw new InvalidOperationException($"Ya existe una categoría con el nombre '{name}'");

            var entity = new Domain.Entities.ProductCategory
            {
                Name        = name,
                Description = string.IsNullOrWhiteSpace(d.Description) ? null : d.Description.Trim(),
                Code        = code,
                IsActive    = d.IsActive,
                CreatedAt   = DateTime.UtcNow,
            };

            var created = await _repo.CreateAsync(entity);
            return ProductCategoryMapper.MapToDto(created, productsCount: 0, subcategoriesCount: 0);
        }
    }

    // ─────────────────────────────────────────────
    // UPDATE
    // ─────────────────────────────────────────────
    public class UpdateProductCategoryCommandHandler : IRequestHandler<UpdateProductCategoryCommand, ProductCategoryDto>
    {
        private readonly IProductCategoryRepository _repo;

        public UpdateProductCategoryCommandHandler(IProductCategoryRepository repo) => _repo = repo;

        public async Task<ProductCategoryDto> Handle(UpdateProductCategoryCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repo.GetByIdAsync(request.Id)
                ?? throw new KeyNotFoundException($"Categoría {request.Id} no encontrada");

            var d = request.Data;
            var newName = d.Name.Trim();
            var newCode = d.Code.Trim().ToUpper();

            if (newCode != entity.Code && await _repo.CodeExistsAsync(newCode, entity.Id))
                throw new InvalidOperationException($"Ya existe otra categoría con el código '{newCode}'");

            if (!string.Equals(newName, entity.Name, StringComparison.OrdinalIgnoreCase)
                && await _repo.NameExistsAsync(newName, entity.Id))
                throw new InvalidOperationException($"Ya existe otra categoría con el nombre '{newName}'");

            entity.Name        = newName;
            entity.Description = string.IsNullOrWhiteSpace(d.Description) ? null : d.Description.Trim();
            entity.Code        = newCode;
            entity.IsActive    = d.IsActive;

            var updated = await _repo.UpdateAsync(entity);

            var productsCount = await _repo.CountProductsAsync(updated.Id);
            var subcatsCount  = await _repo.CountSubcategoriesAsync(updated.Id);
            return ProductCategoryMapper.MapToDto(updated, productsCount, subcatsCount);
        }
    }

    // ─────────────────────────────────────────────
    // DELETE (soft delete: IsActive = false)
    // ─────────────────────────────────────────────
    public class DeleteProductCategoryCommandHandler : IRequestHandler<DeleteProductCategoryCommand, bool>
    {
        private readonly IProductCategoryRepository _repo;

        public DeleteProductCategoryCommandHandler(IProductCategoryRepository repo) => _repo = repo;

        public async Task<bool> Handle(DeleteProductCategoryCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repo.GetByIdAsync(request.Id)
                ?? throw new KeyNotFoundException($"Categoría {request.Id} no encontrada");

            if (await _repo.HasProductsAsync(entity.Id))
                throw new InvalidOperationException(
                    $"No se puede eliminar la categoría '{entity.Name}' porque tiene productos asociados. " +
                    "Reasigna o elimina los productos primero.");

            return await _repo.DeleteAsync(entity.Id);
        }
    }

    // ─────────────────────────────────────────────
    // QUERIES
    // ─────────────────────────────────────────────
    public class GetAllProductCategoriesQueryHandler : IRequestHandler<GetAllProductCategoriesQuery, List<ProductCategoryDto>>
    {
        private readonly IProductCategoryRepository _repo;

        public GetAllProductCategoriesQueryHandler(IProductCategoryRepository repo) => _repo = repo;

        public async Task<List<ProductCategoryDto>> Handle(GetAllProductCategoriesQuery request, CancellationToken cancellationToken)
        {
            var list = await _repo.GetAllAsync(request.IncludeInactive);
            return list
                .Select(c => ProductCategoryMapper.MapToDto(
                    c,
                    productsCount: c.Products?.Count ?? 0,
                    subcategoriesCount: c.Subcategories?.Count(s => s.IsActive) ?? 0))
                .ToList();
        }
    }

    public class GetProductCategoryByIdQueryHandler : IRequestHandler<GetProductCategoryByIdQuery, ProductCategoryDetailDto?>
    {
        private readonly IProductCategoryRepository _repo;

        public GetProductCategoryByIdQueryHandler(IProductCategoryRepository repo) => _repo = repo;

        public async Task<ProductCategoryDetailDto?> Handle(GetProductCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _repo.GetByIdAsync(request.Id, includeSubcategories: true);
            if (entity is null) return null;

            return ProductCategoryMapper.MapToDetailDto(entity);
        }
    }

    public class GetProductCategoriesDropdownQueryHandler : IRequestHandler<GetProductCategoriesDropdownQuery, List<ProductCategoryDropdownDto>>
    {
        private readonly IProductCategoryRepository _repo;

        public GetProductCategoriesDropdownQueryHandler(IProductCategoryRepository repo) => _repo = repo;

        public async Task<List<ProductCategoryDropdownDto>> Handle(GetProductCategoriesDropdownQuery request, CancellationToken cancellationToken)
        {
            var list = await _repo.GetAllAsync(includeInactive: false);
            return list.Select(c => new ProductCategoryDropdownDto
            {
                Value = c.Id,
                Label = c.Name,
                Code  = c.Code.ToLower(),
            }).ToList();
        }
    }

    public class GetProductCategoryStatsQueryHandler : IRequestHandler<GetProductCategoryStatsQuery, ProductCategoryStatsResultDto>
    {
        private readonly IProductCategoryRepository _repo;

        public GetProductCategoryStatsQueryHandler(IProductCategoryRepository repo) => _repo = repo;

        public async Task<ProductCategoryStatsResultDto> Handle(GetProductCategoryStatsQuery request, CancellationToken cancellationToken)
        {
            var rows = await _repo.GetStatsAsync();

            var stats = rows
                .OrderByDescending(r => r.ProductsCount)
                .Select(r => new ProductCategoryStatsDto
                {
                    CategoryId         = r.CategoryId,
                    CategoryName       = r.CategoryName,
                    CategoryCode       = r.CategoryCode,
                    ProductsCount      = r.ProductsCount,
                    TotalProducts      = r.TotalProducts,
                    SubcategoriesCount = r.SubcategoriesCount,
                    AvgPrice           = r.AvgPrice,
                    TotalValue         = r.TotalValue,
                }).ToList();

            var summary = new ProductCategoryStatsSummaryDto
            {
                TotalCategories         = stats.Count,
                TotalProducts           = stats.Sum(s => s.ProductsCount),
                TotalValue              = stats.Sum(s => s.TotalValue),
                AvgProductsPerCategory  = stats.Count > 0 ? stats.Average(s => s.ProductsCount) : 0,
            };

            return new ProductCategoryStatsResultDto
            {
                CategoryStats = stats,
                Summary       = summary,
            };
        }
    }

    // ─────────────────────────────────────────────
    // MAPPER
    // ─────────────────────────────────────────────
    internal static class ProductCategoryMapper
    {
        internal static ProductCategoryDto MapToDto(
            Domain.Entities.ProductCategory c,
            int productsCount,
            int subcategoriesCount) => new()
        {
            Id                 = c.Id,
            Name               = c.Name,
            Description        = c.Description,
            Code               = c.Code,
            IsActive           = c.IsActive,
            CreatedAt          = c.CreatedAt,
            ProductsCount      = productsCount,
            SubcategoriesCount = subcategoriesCount,
        };

        internal static ProductCategoryDetailDto MapToDetailDto(Domain.Entities.ProductCategory c) => new()
        {
            Id                 = c.Id,
            Name               = c.Name,
            Description        = c.Description,
            Code               = c.Code,
            IsActive           = c.IsActive,
            CreatedAt          = c.CreatedAt,
            ProductsCount      = c.Products?.Count ?? 0,
            SubcategoriesCount = c.Subcategories?.Count(s => s.IsActive) ?? 0,
            Subcategories      = c.Subcategories?
                .Where(s => s.IsActive)
                .Select(s => new ProductSubcategoryBriefDto
                {
                    Id          = s.Id,
                    Name        = s.Name,
                    Code        = s.Code,
                    Description = s.Description,
                }).ToList() ?? new List<ProductSubcategoryBriefDto>(),
        };
    }
}
