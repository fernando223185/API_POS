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
            var name = request.Name.Trim();
            var code = request.Code.Trim().ToUpper();

            if (await _repository.CodeExistsAsync(code))
                throw new InvalidOperationException($"Ya existe una lista de precios con el código '{code}'");

            if (request.ValidFrom.HasValue && request.ValidTo.HasValue && request.ValidFrom.Value > request.ValidTo.Value)
                throw new InvalidOperationException("La fecha 'ValidFrom' no puede ser posterior a 'ValidTo'");

            var priceList = new Domain.Entities.PriceList
            {
                Name = name,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                Code = code,
                DefaultDiscountPercentage = request.DefaultDiscountPercentage,
                IsDefault = request.IsDefault,
                IsActive = true,
                ValidFrom = request.ValidFrom,
                ValidTo = request.ValidTo,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repository.CreateAsync(priceList);

            // Si esta nueva lista se marca como default, asegurar que sea la única
            if (created.IsDefault)
                await _repository.ClearDefaultFlagExceptAsync(created.Id);

            return created;
        }
    }
}
