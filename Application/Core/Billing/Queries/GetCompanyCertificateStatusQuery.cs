using Application.DTOs.Billing;
using MediatR;

namespace Application.Core.Billing.Queries
{
    /// <summary>
    /// Consulta el estado de los certificados SAT de una empresa.
    /// </summary>
    public class GetCompanyCertificateStatusQuery : IRequest<CertificateStatusDto>
    {
        public int CompanyId { get; set; }

        public GetCompanyCertificateStatusQuery(int companyId) => CompanyId = companyId;
    }
}
