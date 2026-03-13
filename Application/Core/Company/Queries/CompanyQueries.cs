using Application.DTOs.Company;
using MediatR;

namespace Application.Core.Company.Queries
{
    /// <summary>
    /// Query para obtener empresa por ID
    /// </summary>
    public class GetCompanyByIdQuery : IRequest<CompanyResponseDto>
    {
        public int CompanyId { get; set; }

        public GetCompanyByIdQuery(int companyId)
        {
            CompanyId = companyId;
        }
    }

    /// <summary>
    /// Query para obtener todas las empresas activas
    /// </summary>
    public class GetAllActiveCompaniesQuery : IRequest<List<CompanyResponseDto>>
    {
    }

    /// <summary>
    /// Query para obtener empresas paginadas
    /// </summary>
    public class GetCompaniesPagedQuery : IRequest<CompanyListResponseDto>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool? IsActive { get; set; }
        public string? SearchTerm { get; set; }

        public GetCompaniesPagedQuery(
            int page, 
            int pageSize, 
            bool? isActive = null, 
            string? searchTerm = null)
        {
            Page = page;
            PageSize = pageSize;
            IsActive = isActive;
            SearchTerm = searchTerm;
        }
    }

    /// <summary>
    /// Query para obtener la empresa principal
    /// </summary>
    public class GetMainCompanyQuery : IRequest<CompanyResponseDto?>
    {
    }
}
