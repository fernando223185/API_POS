using Application.Abstractions.Config;
using Application.Core.Company.Queries;
using Application.DTOs.Company;
using MediatR;

namespace Application.Core.Company.QueryHandlers
{
    /// <summary>
    /// Handler para obtener empresa por ID
    /// </summary>
    public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, CompanyResponseDto>
    {
        private readonly ICompanyRepository _companyRepository;

        public GetCompanyByIdQueryHandler(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<CompanyResponseDto> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
        {
            var company = await _companyRepository.GetByIdAsync(request.CompanyId);

            if (company == null)
            {
                throw new KeyNotFoundException($"Empresa con ID {request.CompanyId} no encontrada");
            }

            return new CompanyResponseDto
            {
                Id = company.Id,
                Code = company.Code,
                LegalName = company.LegalName,
                TradeName = company.TradeName,
                TaxId = company.TaxId,
                SatTaxRegime = company.SatTaxRegime,
                FiscalZipCode = company.FiscalZipCode,
                FiscalAddress = company.FiscalAddress,
                Phone = company.Phone,
                Email = company.Email,
                Website = company.Website,
                InvoiceSeries = company.InvoiceSeries,
                InvoiceStartingFolio = company.InvoiceStartingFolio,
                InvoiceCurrentFolio = company.InvoiceCurrentFolio,
                DefaultCurrency = company.DefaultCurrency,
                LogoUrl = company.LogoUrl,
                Slogan = company.Slogan,
                IsActive = company.IsActive,
                IsMainCompany = company.IsMainCompany,
                Notes = company.Notes,
                CreatedAt = company.CreatedAt,
                UpdatedAt = company.UpdatedAt,
                CreatedByUserName = company.CreatedBy?.Name,
                UpdatedByUserName = company.UpdatedBy?.Name
            };
        }
    }

    /// <summary>
    /// Handler para obtener todas las empresas activas
    /// </summary>
    public class GetAllActiveCompaniesQueryHandler : IRequestHandler<GetAllActiveCompaniesQuery, List<CompanyResponseDto>>
    {
        private readonly ICompanyRepository _companyRepository;

        public GetAllActiveCompaniesQueryHandler(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<List<CompanyResponseDto>> Handle(GetAllActiveCompaniesQuery request, CancellationToken cancellationToken)
        {
            var companies = await _companyRepository.GetAllActiveAsync();

            return companies.Select(c => new CompanyResponseDto
            {
                Id = c.Id,
                Code = c.Code,
                LegalName = c.LegalName,
                TradeName = c.TradeName,
                TaxId = c.TaxId,
                SatTaxRegime = c.SatTaxRegime,
                FiscalZipCode = c.FiscalZipCode,
                FiscalAddress = c.FiscalAddress,
                Phone = c.Phone,
                Email = c.Email,
                Website = c.Website,
                DefaultCurrency = c.DefaultCurrency,
                LogoUrl = c.LogoUrl,
                IsActive = c.IsActive,
                IsMainCompany = c.IsMainCompany,
                CreatedAt = c.CreatedAt
            }).ToList();
        }
    }

    /// <summary>
    /// Handler para obtener empresas paginadas
    /// </summary>
    public class GetCompaniesPagedQueryHandler : IRequestHandler<GetCompaniesPagedQuery, CompanyListResponseDto>
    {
        private readonly ICompanyRepository _companyRepository;

        public GetCompaniesPagedQueryHandler(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<CompanyListResponseDto> Handle(GetCompaniesPagedQuery request, CancellationToken cancellationToken)
        {
            var (companies, totalRecords) = await _companyRepository.GetPagedAsync(
                request.Page,
                request.PageSize,
                request.IsActive,
                request.SearchTerm
            );

            var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

            var companiesDto = companies.Select(c => new CompanyResponseDto
            {
                Id = c.Id,
                Code = c.Code,
                LegalName = c.LegalName,
                TradeName = c.TradeName,
                TaxId = c.TaxId,
                SatTaxRegime = c.SatTaxRegime,
                FiscalZipCode = c.FiscalZipCode,
                FiscalAddress = c.FiscalAddress,
                Phone = c.Phone,
                Email = c.Email,
                Website = c.Website,
                InvoiceSeries = c.InvoiceSeries,
                InvoiceCurrentFolio = c.InvoiceCurrentFolio,
                DefaultCurrency = c.DefaultCurrency,
                LogoUrl = c.LogoUrl,
                Slogan = c.Slogan,
                IsActive = c.IsActive,
                IsMainCompany = c.IsMainCompany,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                CreatedByUserName = c.CreatedBy?.Name,
                UpdatedByUserName = c.UpdatedBy?.Name
            }).ToList();

            return new CompanyListResponseDto
            {
                Message = "Empresas obtenidas exitosamente",
                Error = 0,
                Data = companiesDto,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }
    }

    /// <summary>
    /// Handler para obtener la empresa principal
    /// </summary>
    public class GetMainCompanyQueryHandler : IRequestHandler<GetMainCompanyQuery, CompanyResponseDto?>
    {
        private readonly ICompanyRepository _companyRepository;

        public GetMainCompanyQueryHandler(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<CompanyResponseDto?> Handle(GetMainCompanyQuery request, CancellationToken cancellationToken)
        {
            var company = await _companyRepository.GetMainCompanyAsync();

            if (company == null)
            {
                return null;
            }

            return new CompanyResponseDto
            {
                Id = company.Id,
                Code = company.Code,
                LegalName = company.LegalName,
                TradeName = company.TradeName,
                TaxId = company.TaxId,
                SatTaxRegime = company.SatTaxRegime,
                FiscalZipCode = company.FiscalZipCode,
                FiscalAddress = company.FiscalAddress,
                Phone = company.Phone,
                Email = company.Email,
                Website = company.Website,
                InvoiceSeries = company.InvoiceSeries,
                InvoiceStartingFolio = company.InvoiceStartingFolio,
                InvoiceCurrentFolio = company.InvoiceCurrentFolio,
                DefaultCurrency = company.DefaultCurrency,
                LogoUrl = company.LogoUrl,
                Slogan = company.Slogan,
                IsActive = company.IsActive,
                IsMainCompany = company.IsMainCompany,
                Notes = company.Notes,
                CreatedAt = company.CreatedAt,
                UpdatedAt = company.UpdatedAt
            };
        }
    }
}
