using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class PaginatedDto
    {
        public int page {  get; set; }
        public int totalPages { get; set; }
        public int sizePage { get; set; }
        public Object data { get; set; }
    }
}
