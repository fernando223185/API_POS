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
            var priceList = await _repository.GetByIdAsync(request.Id);
            
            if (priceList == null)
            {
                throw new KeyNotFoundException($"PriceList with ID {request.Id} not found");
            }

            // Eliminación lógica mediante bit IsActive
            return await _repository.DeleteAsync(request.Id);
        }
    }
}
