using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Product.Queries
{
    public class GetProductByIdQuery : IRequest<Products>
    {
        public int ID { get; set; }
    }
}
