using Application.DTOs.PriceList;
using MediatR;

namespace Application.Core.ProductPrice.Commands
{
    public record UpsertProductPriceCommand(CreateProductPriceDto Data, int UserId) : IRequest<ProductPriceDto>;
    public record UpdateProductPriceCommand(int Id, UpdateProductPriceDto Data) : IRequest<ProductPriceDto>;
    public record DeleteProductPriceCommand(int Id) : IRequest<bool>;
    public record BulkUpsertProductPricesCommand(BulkProductPricesDto Data, int UserId) : IRequest<BulkProductPricesResultDto>;
}
