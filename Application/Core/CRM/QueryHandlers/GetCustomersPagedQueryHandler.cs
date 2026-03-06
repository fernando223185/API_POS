using Application.Abstractions.CRM;
using Application.Core.CRM.Queries;
using Application.DTOs.Customer;
using MediatR;

namespace Application.Core.CRM.QueryHandlers
{
    public class GetCustomersPagedQueryHandler : IRequestHandler<GetCustomersPagedQuery, PagedCustomersResponseDto>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetCustomersPagedQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<PagedCustomersResponseDto> Handle(GetCustomersPagedQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Obtener datos paginados del repositorio
                var (customers, totalCount) = await _customerRepository.GetPagedWithCountAsync(
                    page: request.Page,
                    pageSize: request.PageSize,
                    searchTerm: request.SearchTerm,
                    sortBy: request.SortBy,
                    sortDirection: request.SortDirection,
                    isActive: request.IsActive,
                    statusId: request.StatusId,
                    priceListId: request.PriceListId
                );

                // Mapear a DTOs optimizados para la tabla
                var customerTableDtos = customers.Select(c => new CustomerTableDto
                {
                    Id = c.ID,
                    Code = c.Code,
                    Name = c.Name,
                    LastName = c.LastName,
                    Phone = c.Phone,
                    Email = c.Email,
                    CompanyName = c.CompanyName,
                    TaxId = c.TaxId,
                    SatTaxRegime = c.SatTaxRegime,
                    CreatedAt = c.CreatedAt ?? c.CreatedAtOriginal,
                    IsActive = c.IsActive,
                    PriceListId = c.PriceListId,
                    PriceListName = c.PriceList?.Name,
                    DiscountPercentage = c.DiscountPercentage,
                    CreditLimit = c.CreditLimit,
                    Address = c.Address,
                    ZipCode = c.ZipCode,
                    PaymentTermsDays = c.PaymentTermsDays,
                    UpdatedAt = c.UpdatedAt,
                    CreatedByName = c.CreatedBy?.Name,
                    StatusId = c.StatusId
                }).ToList();

                // Calcular información de paginación
                var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
                
                var paginationInfo = new PaginationInfoDto
                {
                    CurrentPage = request.Page,
                    PageSize = request.PageSize,
                    TotalItems = totalCount,
                    TotalPages = totalPages
                };

                // Información de filtros aplicados
                var filterInfo = new FilterInfoDto
                {
                    SearchTerm = request.SearchTerm,
                    SortBy = request.SortBy,
                    SortDirection = request.SortDirection,
                    IsActive = request.IsActive,
                    StatusId = request.StatusId,
                    PriceListId = request.PriceListId,
                    ActiveFiltersCount = CountActiveFilters(request)
                };

                return new PagedCustomersResponseDto
                {
                    Customers = customerTableDtos,
                    Pagination = paginationInfo,
                    Filters = filterInfo
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCustomersPagedQueryHandler: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private int CountActiveFilters(GetCustomersPagedQuery request)
        {
            var count = 0;
            
            if (!string.IsNullOrWhiteSpace(request.SearchTerm)) count++;
            if (request.IsActive.HasValue) count++;
            if (request.StatusId.HasValue) count++;
            if (request.PriceListId.HasValue) count++;
            
            return count;
        }
    }
}