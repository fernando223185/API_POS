using Application.Abstractions.Catalogue;
using Application.Core.Product.Queries;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Product.QueryHandlers
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Products>
    {
        private readonly IProductRepository _repository;

        public GetProductByIdQueryHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<Products> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
        {
            return await _repository.GetByIdAsync(query.ID);
        }

    }
}
