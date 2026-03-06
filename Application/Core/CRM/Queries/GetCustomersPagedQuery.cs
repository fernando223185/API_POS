using MediatR;
using Application.DTOs.Customer;

namespace Application.Core.CRM.Queries
{
    public class GetCustomersPagedQuery : IRequest<PagedCustomersResponseDto>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; } = "name";
        public string? SortDirection { get; set; } = "asc";
        public bool? IsActive { get; set; }
        public int? StatusId { get; set; }
        public int? PriceListId { get; set; }
    }
}