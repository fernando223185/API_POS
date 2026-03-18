using MediatR;

namespace Application.Core.PriceList.Queries
{
    public class GetAllPriceListsQuery : IRequest<List<Domain.Entities.PriceList>>
    {
        public bool? IsActive { get; set; }
    }
}
