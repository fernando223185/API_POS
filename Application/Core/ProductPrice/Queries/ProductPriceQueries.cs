using Application.DTOs.PriceList;
using MediatR;

namespace Application.Core.ProductPrice.Queries
{
    public record GetProductPricesQuery(int? ProductId, int? PriceListId, bool OnlyActive = true) : IRequest<List<ProductPriceDto>>;
    public record GetProductPriceByIdQuery(int Id) : IRequest<ProductPriceDto?>;
    public record GetPricesByProductQuery(int ProductId, bool OnlyActive = true) : IRequest<List<ProductPriceDto>>;
    public record GetPricesByPriceListQuery(int PriceListId, bool OnlyActive = true) : IRequest<List<ProductPriceDto>>;
}
