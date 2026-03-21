using Application.Abstractions.Billing;
using Application.Core.Billing.Queries;
using Application.DTOs.Billing;
using MediatR;

namespace Application.Core.Billing.QueryHandlers
{
    /// <summary>
    /// Devuelve el estado de los certificados SAT de una empresa.
    /// </summary>
    public class GetCompanyCertificateStatusQueryHandler
        : IRequestHandler<GetCompanyCertificateStatusQuery, CertificateStatusDto>
    {
        private readonly ICertificateService _certificateService;

        public GetCompanyCertificateStatusQueryHandler(ICertificateService certificateService)
        {
            _certificateService = certificateService;
        }

        public Task<CertificateStatusDto> Handle(
            GetCompanyCertificateStatusQuery request,
            CancellationToken cancellationToken)
            => _certificateService.GetStatusAsync(request.CompanyId);
    }
}
