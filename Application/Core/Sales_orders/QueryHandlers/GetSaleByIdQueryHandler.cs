using Application.Abstractions.Sales_Orders;
using Application.Core.Sales_orders.Queries;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Sales_orders.QueryHandlers
{
    public class GetSaleByIdQueryHandler : IRequestHandler<GetSaleByIdQuery, Sales>
    {
        private readonly ISalesRepository _repository;

        public GetSaleByIdQueryHandler(ISalesRepository repository)
        {
            _repository = repository;
        }

        public async Task<Sales> Handle(GetSaleByIdQuery query, CancellationToken cancellationToken)
        {
            return await _repository.GetByIdAsync(query.ID);
        }
    }
}
