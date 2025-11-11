using Application.Abstractions.CRM;
using Application.Core.CRM.Queries;
using Domain.DTOs;
using MediatR;

namespace Application.Core.CRM.QueryHandlers
{
    public class GetCustomerByPageQueryHandler : IRequestHandler<GetCustomerByPageQuery, PaginatedDto>
    {
        private readonly ICustomerRepository _repository;

        public GetCustomerByPageQueryHandler(ICustomerRepository repository)
        {
            _repository = repository;
        }

        public async Task<PaginatedDto> Handle(GetCustomerByPageQuery request, CancellationToken cancellationToken)
        {
            var customers = await _repository.GetByPageAsync(request);
            var customersList = customers.ToList();

            return new PaginatedDto
            {
                page = request.Page,
                totalPages = (int)Math.Ceiling((double)customersList.Count / 10), // Calcular páginas
                sizePage = 10,
                data = customersList
            };
        }
    }
}
