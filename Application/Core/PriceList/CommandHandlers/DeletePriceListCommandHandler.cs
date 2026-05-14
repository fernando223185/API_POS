using Application.Abstractions.Catalogue;
using Application.Core.PriceList.Commands;
using MediatR;

namespace Application.Core.PriceList.CommandHandlers
{
    public class DeletePriceListCommandHandler : IRequestHandler<DeletePriceListCommand, bool>
    {
        private readonly IPriceListRepository _repository;

        public DeletePriceListCommandHandler(IPriceListRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(DeletePriceListCommand request, CancellationToken cancellationToken)
        {
            var priceList = await _repository.GetByIdAsync(request.Id)
                ?? throw new KeyNotFoundException($"Lista de precios con ID {request.Id} no encontrada");

            if (priceList.IsDefault)
                throw new InvalidOperationException(
                    $"No se puede desactivar la lista '{priceList.Name}' porque está marcada como predeterminada. " +
                    "Marca otra lista como predeterminada primero.");

            if (await _repository.HasActiveDependenciesAsync(priceList.Id))
                throw new InvalidOperationException(
                    $"No se puede desactivar la lista '{priceList.Name}' porque tiene clientes, ventas, cotizaciones " +
                    "o precios de productos asociados. Reasigna esas dependencias antes de desactivarla.");

            return await _repository.SetActiveAsync(priceList.Id, false);
        }
    }
}
