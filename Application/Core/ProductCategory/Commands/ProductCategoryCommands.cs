using Application.DTOs.Product;
using MediatR;

namespace Application.Core.ProductCategory.Commands
{
    public record CreateProductCategoryCommand(CreateProductCategoryDto Data) : IRequest<ProductCategoryDto>;
    public record UpdateProductCategoryCommand(int Id, UpdateProductCategoryDto Data) : IRequest<ProductCategoryDto>;
    public record DeleteProductCategoryCommand(int Id) : IRequest<bool>;
}
