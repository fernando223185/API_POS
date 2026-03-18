using Application.Abstractions.Messaging;

namespace Application.Core.PriceList.Commands
{
    public class DeletePriceListCommand : ICommand<bool>
    {
        public int Id { get; set; }
    }
}
