using Application.Abstractions.CRM;
using Application.Core.CRM.Queries;
using Domain.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return await _repository.GetByPageAsync(request);
        }
    }
}
