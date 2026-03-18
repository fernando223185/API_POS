using Application.Abstractions.Catalogue;
using Application.Core.PriceList.Queries;
using MediatR;

namespace Application.Core.PriceList.QueryHandlers
{
    public class GetPriceListByIdQueryHandler : IRequestHandler<GetPriceListByIdQuery, Domain.Entities.PriceList?>
    {
        private readonly IPriceListRepository _repository;

        public GetPriceListByIdQueryHandler(IPriceListRepository repository)
        {
            _repository = repository;
        }

        public async Task<Domain.Entities.PriceList?> Handle(GetPriceListByIdQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetByIdAsync(request.Id);
        }
    }
}
