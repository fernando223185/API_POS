using MediatR;

namespace Application.Core.PriceList.Commands
{
    public class DeletePriceListCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}
