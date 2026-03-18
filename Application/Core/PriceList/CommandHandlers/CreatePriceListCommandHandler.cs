using Application.Abstractions.Catalogue;
using Application.Core.PriceList.Commands;
using MediatR;

namespace Application.Core.PriceList.CommandHandlers
{
    public class CreatePriceListCommandHandler : IRequestHandler<CreatePriceListCommand, Domain.Entities.PriceList>
    {
        private readonly IPriceListRepository _repository;

        public CreatePriceListCommandHandler(IPriceListRepository repository)
        {
            _repository = repository;
        }

        public async Task<Domain.Entities.PriceList> Handle(CreatePriceListCommand request, CancellationToken cancellationToken)
        {
            var priceList = new Domain.Entities.PriceList
            {
                Name = request.Name,
                Description = request.Description,
                Code = request.Code,
                DefaultDiscountPercentage = request.DefaultDiscountPercentage,
                IsDefault = request.IsDefault,
                IsActive = true,
                ValidFrom = request.ValidFrom,
                ValidTo = request.ValidTo,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _repository.CreateAsync(priceList);
            return result;
        }
    }
}
