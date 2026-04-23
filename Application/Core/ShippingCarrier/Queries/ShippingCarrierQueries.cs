using Application.DTOs.ShippingCarrier;
using MediatR;

namespace Application.Core.ShippingCarrier.Queries
{
    public record GetAllShippingCarriersQuery(int? CompanyId, bool IncludeInactive = false) : IRequest<List<ShippingCarrierDto>>;
    public record GetShippingCarrierByIdQuery(int Id) : IRequest<ShippingCarrierDto?>;
}
