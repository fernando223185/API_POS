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
            var priceList = await _repository.GetByIdAsync(request.Id)
                ?? throw new KeyNotFoundException($"Lista de precios con ID {request.Id} no encontrada");

            var newName = request.Name.Trim();
            var newCode = request.Code.Trim().ToUpper();

            if (newCode != priceList.Code && await _repository.CodeExistsAsync(newCode, priceList.Id))
                throw new InvalidOperationException($"Ya existe otra lista de precios con el código '{newCode}'");

            if (request.ValidFrom.HasValue && request.ValidTo.HasValue && request.ValidFrom.Value > request.ValidTo.Value)
                throw new InvalidOperationException("La fecha 'ValidFrom' no puede ser posterior a 'ValidTo'");

            priceList.Name = newName;
            priceList.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            priceList.Code = newCode;
            priceList.DefaultDiscountPercentage = request.DefaultDiscountPercentage;
            priceList.IsDefault = request.IsDefault;
            priceList.IsActive = request.IsActive;
            priceList.ValidFrom = request.ValidFrom;
            priceList.ValidTo = request.ValidTo;

            var updated = await _repository.UpdateAsync(priceList);

            // Si quedó marcada como default, asegurar que sea la única
            if (updated.IsDefault)
                await _repository.ClearDefaultFlagExceptAsync(updated.Id);

            return updated;
        }
    }
}
