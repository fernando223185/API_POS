using Application.Abstractions.Sales_Orders;
using Application.Core.Sales_orders.Commands;
using Domain.Entities;
using MediatR;


namespace Application.Core.Sales_orders.CommandHandlers
{
    public class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, Sales>
    {
        private readonly ISalesRepository _repository;
        public CreateSaleCommandHandler(ISalesRepository repository) 
        {
            _repository = repository;
        }

        private string GetTimestamp()
        {
            var datePart = DateTime.Now.ToString("yyMMddHHmm");
            var randomPart = new Random().Next(100, 999);
            return "V" + datePart + randomPart;
        }

        public async Task<Sales> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
        {
            var MovID = GetTimestamp();
            var sale = new Sales
            {
                Company_ID = request.Company_ID,
                Mov = request.Mov,
                MovID = MovID,
                FechaEmision = DateTime.Now,
                Moneda = request.Moneda,
                User = request.User,
                Status = request.Status,
                Customer = request.Customer

            };
            return await _repository.CreateAsync(sale);

        }
    }
}
