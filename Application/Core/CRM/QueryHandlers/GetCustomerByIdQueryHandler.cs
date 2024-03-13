using Application.Abstractions.CRM;
using Application.Core.CRM.Queries;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.CRM.QueryHandlers
{
    public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, Customer>
    {
        private readonly ICustomerRepository _repository;

        public GetCustomerByIdQueryHandler(ICustomerRepository repository)
        {
            _repository = repository;
        }

        public async Task<Customer> Handle(GetCustomerByIdQuery query, CancellationToken cancellationToken)
        {
            return await _repository.GetByIdAsync(query.ID);
        }
    }
}
