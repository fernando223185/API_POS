using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.CRM.Queries
{
    public class GetCustomerByIdQuery : IRequest<Customer>
    {
        public int ID { get; set; }
    }
}
