using Application.Abstractions.Catalogue;
using Application.Core.Product.Queries;
using Domain.DTOs;
using MediatR;

namespace Application.Core.Product.QueryHandlers
{
    public class GetProductByPageQueryHandler : IRequestHandler<GetProductByPageQuery, PaginatedDto>
    {
        private readonly IProductRepository _repository;

        public GetProductByPageQueryHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<PaginatedDto> Handle(GetProductByPageQuery request, CancellationToken cancellationToken)
        {
            // Convertir de GetProductByPageQuery a ProductPageQuery
            var repoQuery = new ProductPageQuery
            {
                Size = 10, // Tamaño fijo por defecto
                Nro = request.Page, // Usar Page como número de página
                search = request.search
            };

            var products = await _repository.GetByPageAsync(repoQuery);
            var productsList = products.ToList();

            return new PaginatedDto
            {
                page = request.Page,
                totalPages = (int)Math.Ceiling((double)productsList.Count / 10), // Calcular páginas
                sizePage = 10,
                data = productsList
            };
        }
    }
}
