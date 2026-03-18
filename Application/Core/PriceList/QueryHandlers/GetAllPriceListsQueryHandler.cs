using Application.Abstractions.Catalogue;
using Application.Core.PriceList.Queries;
using MediatR;

namespace Application.Core.PriceList.QueryHandlers
{
    public class GetAllPriceListsQueryHandler : IRequestHandler<GetAllPriceListsQuery, List<Domain.Entities.PriceList>>
    {
        private readonly IPriceListRepository _repository;

        public GetAllPriceListsQueryHandler(IPriceListRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Domain.Entities.PriceList>> Handle(GetAllPriceListsQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetAllAsync(request.IsActive);
        }
    }
}
