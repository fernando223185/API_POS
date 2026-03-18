using MediatR;

namespace Application.Core.PriceList.Queries
{
    public class GetPriceListByIdQuery : IRequest<Domain.Entities.PriceList?>
    {
        public int Id { get; set; }
    }
}
