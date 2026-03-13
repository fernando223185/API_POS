using Application.Abstractions.Common;
using Application.Abstractions.Config;
using Application.Core.Company.Commands;
using Application.DTOs.Company;
using MediatR;

namespace Application.Core.Company.CommandHandlers
{
    /// <summary>
    /// Handler para crear empresa
    /// </summary>
    public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, CompanyResponseDto>
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly ICodeGeneratorService _codeGeneratorService;

        public CreateCompanyCommandHandler(
            ICompanyRepository companyRepository,
            ICodeGeneratorService codeGeneratorService)
        {
            _companyRepository = companyRepository;
            _codeGeneratorService = codeGeneratorService;
        }

        public async Task<CompanyResponseDto> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
        {
            // Validar que no exista el RFC
            if (await _companyRepository.ExistsByTaxIdAsync(request.CompanyData.TaxId))
            {
                throw new InvalidOperationException($"Ya existe una empresa con el RFC: {request.CompanyData.TaxId}");
            }

            // Generar código único
            var code = await _codeGeneratorService.GenerateNextCodeAsync("COMP", "Companies", "Code", 3);

            var company = new Domain.Entities.Company
            {
                Code = code,
                LegalName = request.CompanyData.LegalName,
                TradeName = request.CompanyData.TradeName,
                TaxId = request.CompanyData.TaxId.ToUpper(),
                SatTaxRegime = request.CompanyData.SatTaxRegime,
                FiscalZipCode = request.CompanyData.FiscalZipCode,
                FiscalAddress = request.CompanyData.FiscalAddress,
                Phone = request.CompanyData.Phone,
                Email = request.CompanyData.Email,
                Website = request.CompanyData.Website,
                InvoiceSeries = request.CompanyData.InvoiceSeries,
                InvoiceStartingFolio = request.CompanyData.InvoiceStartingFolio,
                InvoiceCurrentFolio = request.CompanyData.InvoiceStartingFolio,
                DefaultCurrency = request.CompanyData.DefaultCurrency,
                LogoUrl = request.CompanyData.LogoUrl,
                Slogan = request.CompanyData.Slogan,
                IsMainCompany = request.CompanyData.IsMainCompany,
                Notes = request.CompanyData.Notes,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = request.CreatedByUserId
            };

            var createdCompany = await _companyRepository.CreateAsync(company);

            // Recargar con relaciones
            createdCompany = await _companyRepository.GetByIdAsync(createdCompany.Id);

            return new CompanyResponseDto
            {
                Id = createdCompany!.Id,
                Code = createdCompany.Code,
                LegalName = createdCompany.LegalName,
                TradeName = createdCompany.TradeName,
                TaxId = createdCompany.TaxId,
                SatTaxRegime = createdCompany.SatTaxRegime,
                FiscalZipCode = createdCompany.FiscalZipCode,
                FiscalAddress = createdCompany.FiscalAddress,
                Phone = createdCompany.Phone,
                Email = createdCompany.Email,
                Website = createdCompany.Website,
                InvoiceSeries = createdCompany.InvoiceSeries,
                InvoiceStartingFolio = createdCompany.InvoiceStartingFolio,
                InvoiceCurrentFolio = createdCompany.InvoiceCurrentFolio,
                DefaultCurrency = createdCompany.DefaultCurrency,
                LogoUrl = createdCompany.LogoUrl,
                Slogan = createdCompany.Slogan,
                IsActive = createdCompany.IsActive,
                IsMainCompany = createdCompany.IsMainCompany,
                Notes = createdCompany.Notes,
                CreatedAt = createdCompany.CreatedAt,
                CreatedByUserName = createdCompany.CreatedBy?.Name
            };
        }
    }

    /// <summary>
    /// Handler para actualizar empresa
    /// </summary>
    public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, CompanyResponseDto>
    {
        private readonly ICompanyRepository _companyRepository;

        public UpdateCompanyCommandHandler(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<CompanyResponseDto> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
        {
            var company = await _companyRepository.GetByIdAsync(request.CompanyId);

            if (company == null)
            {
                throw new KeyNotFoundException($"Empresa con ID {request.CompanyId} no encontrada");
            }

            // Actualizar campos
            company.LegalName = request.CompanyData.LegalName;
            company.TradeName = request.CompanyData.TradeName;
            company.SatTaxRegime = request.CompanyData.SatTaxRegime;
            company.FiscalZipCode = request.CompanyData.FiscalZipCode;
            company.FiscalAddress = request.CompanyData.FiscalAddress;
            company.Phone = request.CompanyData.Phone;
            company.Email = request.CompanyData.Email;
            company.Website = request.CompanyData.Website;
            company.InvoiceSeries = request.CompanyData.InvoiceSeries;
            company.DefaultCurrency = request.CompanyData.DefaultCurrency;
            company.LogoUrl = request.CompanyData.LogoUrl;
            company.Slogan = request.CompanyData.Slogan;
            company.Notes = request.CompanyData.Notes;
            company.UpdatedAt = DateTime.UtcNow;
            company.UpdatedByUserId = request.UpdatedByUserId;

            await _companyRepository.UpdateAsync(company);

            // Recargar con relaciones
            company = await _companyRepository.GetByIdAsync(company.Id);

            return new CompanyResponseDto
            {
                Id = company!.Id,
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
    /// Handler para desactivar empresa
    /// </summary>
    public class DeactivateCompanyCommandHandler : IRequestHandler<DeactivateCompanyCommand, bool>
    {
        private readonly ICompanyRepository _companyRepository;

        public DeactivateCompanyCommandHandler(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<bool> Handle(DeactivateCompanyCommand request, CancellationToken cancellationToken)
        {
            var company = await _companyRepository.GetByIdAsync(request.CompanyId);

            if (company == null)
            {
                throw new KeyNotFoundException($"Empresa con ID {request.CompanyId} no encontrada");
            }

            if (!company.IsActive)
            {
                throw new InvalidOperationException("La empresa ya está inactiva");
            }

            // Validar que no sea la empresa principal
            if (company.IsMainCompany)
            {
                throw new InvalidOperationException("No se puede desactivar la empresa principal");
            }

            company.IsActive = false;
            company.UpdatedAt = DateTime.UtcNow;
            company.UpdatedByUserId = request.UpdatedByUserId;

            await _companyRepository.UpdateAsync(company);

            return true;
        }
    }

    /// <summary>
    /// Handler para reactivar empresa
    /// </summary>
    public class ReactivateCompanyCommandHandler : IRequestHandler<ReactivateCompanyCommand, bool>
    {
        private readonly ICompanyRepository _companyRepository;

        public ReactivateCompanyCommandHandler(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<bool> Handle(ReactivateCompanyCommand request, CancellationToken cancellationToken)
        {
            var company = await _companyRepository.GetByIdAsync(request.CompanyId);

            if (company == null)
            {
                throw new KeyNotFoundException($"Empresa con ID {request.CompanyId} no encontrada");
            }

            if (company.IsActive)
            {
                throw new InvalidOperationException("La empresa ya está activa");
            }

            company.IsActive = true;
            company.UpdatedAt = DateTime.UtcNow;
            company.UpdatedByUserId = request.UpdatedByUserId;

            await _companyRepository.UpdateAsync(company);

            return true;
        }
    }

    /// <summary>
    /// Handler para actualizar configuración fiscal
    /// </summary>
    public class UpdateCompanyFiscalConfigCommandHandler : IRequestHandler<UpdateCompanyFiscalConfigCommand, CompanyResponseDto>
    {
        private readonly ICompanyRepository _companyRepository;

        public UpdateCompanyFiscalConfigCommandHandler(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<CompanyResponseDto> Handle(UpdateCompanyFiscalConfigCommand request, CancellationToken cancellationToken)
        {
            var company = await _companyRepository.GetByIdAsync(request.CompanyId);

            if (company == null)
            {
                throw new KeyNotFoundException($"Empresa con ID {request.CompanyId} no encontrada");
            }

            // Actualizar solo configuración fiscal
            if (!string.IsNullOrWhiteSpace(request.FiscalConfig.SatCertificatePath))
            {
                company.SatCertificatePath = request.FiscalConfig.SatCertificatePath;
            }

            if (!string.IsNullOrWhiteSpace(request.FiscalConfig.SatKeyPath))
            {
                company.SatKeyPath = request.FiscalConfig.SatKeyPath;
            }

            if (!string.IsNullOrWhiteSpace(request.FiscalConfig.SatKeyPassword))
            {
                company.SatKeyPassword = request.FiscalConfig.SatKeyPassword;
            }

            if (request.FiscalConfig.InvoiceCurrentFolio.HasValue)
            {
                company.InvoiceCurrentFolio = request.FiscalConfig.InvoiceCurrentFolio.Value;
            }

            company.UpdatedAt = DateTime.UtcNow;
            company.UpdatedByUserId = request.UpdatedByUserId;

            await _companyRepository.UpdateAsync(company);

            // Recargar con relaciones
            company = await _companyRepository.GetByIdAsync(company.Id);

            return new CompanyResponseDto
            {
                Id = company!.Id,
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
}
