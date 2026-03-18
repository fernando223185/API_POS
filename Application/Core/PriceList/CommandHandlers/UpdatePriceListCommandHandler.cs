using Application.Abstractions.Catalogue;
using Application.Core.PriceList.Commands;
using MediatR;

namespace Application.Core.PriceList.CommandHandlers
{
    public class UpdatePriceListCommandHandler : IRequestHandler<UpdatePriceListCommand, Domain.Entities.PriceList>
    {
        private readonly IPriceListRepository _repository;

        public UpdatePriceListCommandHandler(IPriceListRepository repository)
        {
            _repository = repository;
        }

        public async Task<Domain.Entities.PriceList> Handle(UpdatePriceListCommand request, CancellationToken cancellationToken)
        {
            var priceList = await _repository.GetByIdAsync(request.Id);
            
            if (priceList == null)
            {
                throw new KeyNotFoundException($"PriceList with ID {request.Id} not found");
            }

            priceList.Name = request.Name;
            priceList.Description = request.Description;
            priceList.Code = request.Code;
            priceList.DefaultDiscountPercentage = request.DefaultDiscountPercentage;
            priceList.IsDefault = request.IsDefault;
            priceList.ValidFrom = request.ValidFrom;
            priceList.ValidTo = request.ValidTo;

            var result = await _repository.UpdateAsync(priceList);
            return result;
        }
    }
}
