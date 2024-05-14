using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Sales_orders.Queries
{
    public class GetSaleByIdQuery : IRequest<Sales>
    {
        public int ID { get; set; } 
    }
}
