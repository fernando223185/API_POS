using Application.DTOs.Company;
using MediatR;

namespace Application.Core.Company.Commands
{
    /// <summary>
    /// Comando para crear una nueva empresa
    /// </summary>
    public class CreateCompanyCommand : IRequest<CompanyResponseDto>
    {
        public CreateCompanyDto CompanyData { get; set; }
        public int CreatedByUserId { get; set; }

        public CreateCompanyCommand(CreateCompanyDto companyData, int createdByUserId)
        {
            CompanyData = companyData;
            CreatedByUserId = createdByUserId;
        }
    }

    /// <summary>
    /// Comando para actualizar una empresa existente
    /// </summary>
    public class UpdateCompanyCommand : IRequest<CompanyResponseDto>
    {
        public int CompanyId { get; set; }
        public UpdateCompanyDto CompanyData { get; set; }
        public int UpdatedByUserId { get; set; }

        public UpdateCompanyCommand(int companyId, UpdateCompanyDto companyData, int updatedByUserId)
        {
            CompanyId = companyId;
            CompanyData = companyData;
            UpdatedByUserId = updatedByUserId;
        }
    }

    /// <summary>
    /// Comando para dar de baja lógica una empresa
    /// </summary>
    public class DeactivateCompanyCommand : IRequest<bool>
    {
        public int CompanyId { get; set; }
        public int UpdatedByUserId { get; set; }

        public DeactivateCompanyCommand(int companyId, int updatedByUserId)
        {
            CompanyId = companyId;
            UpdatedByUserId = updatedByUserId;
        }
    }

    /// <summary>
    /// Comando para reactivar una empresa
    /// </summary>
    public class ReactivateCompanyCommand : IRequest<bool>
    {
        public int CompanyId { get; set; }
        public int UpdatedByUserId { get; set; }

        public ReactivateCompanyCommand(int companyId, int updatedByUserId)
        {
            CompanyId = companyId;
            UpdatedByUserId = updatedByUserId;
        }
    }

    /// <summary>
    /// Comando para actualizar configuración fiscal
    /// </summary>
    public class UpdateCompanyFiscalConfigCommand : IRequest<CompanyResponseDto>
    {
        public int CompanyId { get; set; }
        public UpdateCompanyFiscalConfigDto FiscalConfig { get; set; }
        public int UpdatedByUserId { get; set; }

        public UpdateCompanyFiscalConfigCommand(
            int companyId, 
            UpdateCompanyFiscalConfigDto fiscalConfig, 
            int updatedByUserId)
        {
            CompanyId = companyId;
            FiscalConfig = fiscalConfig;
            UpdatedByUserId = updatedByUserId;
        }
    }
}
