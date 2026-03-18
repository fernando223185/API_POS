using Application.Abstractions.Messaging;

namespace Application.Core.PriceList.Commands
{
    public class UpdatePriceListCommand : ICommand<Domain.Entities.PriceList>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal DefaultDiscountPercentage { get; set; }
        public bool IsDefault { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }
}
