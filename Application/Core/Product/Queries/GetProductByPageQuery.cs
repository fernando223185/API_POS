using Application.DTOs.Product;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Core.Product.Queries
{
    public class GetProductByPageQuery : IRequest<GetProductsPagedResponseDto>
    {
        [Required]
        public int Page { get; set; } = 1;
        
        public int PageSize { get; set; } = 50;
        
        public string? Search { get; set; }
        
        public int? CategoryId { get; set; }
        
        public bool? IsActive { get; set; } = true;
        
        public string? SortBy { get; set; } = "name"; // name, price, createdAt, code
        
        public string? SortOrder { get; set; } = "asc"; // asc, desc
    }
}
