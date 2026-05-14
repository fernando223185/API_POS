using Application.Abstractions.Catalogue;
using Application.Core.PriceList.Queries;
using MediatR;

namespace Application.Core.PriceList.QueryHandlers
{
    public class GetDefaultPriceListQueryHandler : IRequestHandler<GetDefaultPriceListQuery, Domain.Entities.PriceList?>
    {
        private readonly IPriceListRepository _repository;

        public GetDefaultPriceListQueryHandler(IPriceListRepository repository)
        {
            _repository = repository;
        }

        public Task<Domain.Entities.PriceList?> Handle(GetDefaultPriceListQuery request, CancellationToken cancellationToken)
            => _repository.GetDefaultAsync();
    }
}
