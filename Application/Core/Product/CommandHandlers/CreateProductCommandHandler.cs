using Application.Abstractions.Catalogue;
using Application.Core.Product.Commands;
using Domain.Entities;
using MediatR;

namespace Application.Core.Product.CommandHandlers
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Products>
    {
        private readonly IProductRepository _repository;

        public CreateProductCommandHandler(IProductRepository repository) 
        {
            _repository = repository;
        }
        public async Task<Products> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Products
            {
                name = request.name,
                description = request.description,
                code = request.code,
                barcode = request.barcode,
                price = request.price
            };
            return await _repository.CreateAsync(product);
        }

    }
}
