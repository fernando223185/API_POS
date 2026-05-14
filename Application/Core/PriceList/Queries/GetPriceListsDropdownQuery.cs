using MediatR;

namespace Application.Core.PriceList.Queries
{
    public class GetPriceListsDropdownQuery : IRequest<List<Domain.Entities.PriceList>>
    {
    }
}
