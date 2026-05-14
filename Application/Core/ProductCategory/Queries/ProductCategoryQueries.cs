using Application.DTOs.Product;
using MediatR;

namespace Application.Core.ProductCategory.Queries
{
    public record GetAllProductCategoriesQuery(bool IncludeInactive = false) : IRequest<List<ProductCategoryDto>>;
    public record GetProductCategoryByIdQuery(int Id) : IRequest<ProductCategoryDetailDto?>;
    public record GetProductCategoriesDropdownQuery() : IRequest<List<ProductCategoryDropdownDto>>;
    public record GetProductCategoryStatsQuery() : IRequest<ProductCategoryStatsResultDto>;
}
