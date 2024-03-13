using Domain.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.CRM.Queries
{
    public class GetCustomerByPageQuery : IRequest<PaginatedDto>
    {
        [Required]
        public int Page { get; set; }
        public string? search { get; set; }

    }
}
