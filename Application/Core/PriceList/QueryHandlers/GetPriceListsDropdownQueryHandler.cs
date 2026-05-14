using Application.Abstractions.Catalogue;
using Application.Core.PriceList.Queries;
using MediatR;

namespace Application.Core.PriceList.QueryHandlers
{
    public class GetPriceListsDropdownQueryHandler : IRequestHandler<GetPriceListsDropdownQuery, List<Domain.Entities.PriceList>>
    {
        private readonly IPriceListRepository _repository;

        public GetPriceListsDropdownQueryHandler(IPriceListRepository repository)
        {
            _repository = repository;
        }

        public Task<List<Domain.Entities.PriceList>> Handle(GetPriceListsDropdownQuery request, CancellationToken cancellationToken)
            => _repository.GetAllAsync(isActive: true);
    }
}
