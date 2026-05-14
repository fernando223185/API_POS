using MediatR;

namespace Application.Core.PriceList.Queries
{
    public class GetDefaultPriceListQuery : IRequest<Domain.Entities.PriceList?>
    {
    }
}
