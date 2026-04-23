using Application.DTOs.ShippingCarrier;
using MediatR;

namespace Application.Core.ShippingCarrier.Commands
{
    public record CreateShippingCarrierCommand(CreateShippingCarrierDto Data, int UserId) : IRequest<ShippingCarrierDto>;
    public record UpdateShippingCarrierCommand(int Id, UpdateShippingCarrierDto Data) : IRequest<ShippingCarrierDto>;
    public record DeleteShippingCarrierCommand(int Id) : IRequest<bool>;
}
