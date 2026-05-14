using MediatR;

namespace Application.Core.PriceList.Commands
{
    public class CreatePriceListCommand : IRequest<Domain.Entities.PriceList>
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal DefaultDiscountPercentage { get; set; } = 0;
        public bool IsDefault { get; set; } = false;
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }
}
