using Application.Abstractions.Catalogue;
using Application.Core.Product.Queries;
using Domain.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Product.QueryHandlers
{
    public class GetProductByPageQueryHandler : IRequestHandler<GetProductByPageQuery,PaginatedDto>
    {
        private readonly IProductRepository _repository;
        public GetProductByPageQueryHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<PaginatedDto> Handle(GetProductByPageQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetByPageAsync(request);
        }
    }
}
