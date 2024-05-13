using Domain.Entities;
using System.Data.SqlTypes;
using Application.Abstractions.Messaging;


namespace Application.Core.Sales_orders.Commands
{
    public class CreateSaleCommand : ICommand<Sales>
    {
        public int Company_ID { get; set; }
        public string Mov { get; set; }
        public string Moneda { get; set; }
        public string User { get; set; }
        public string Status { get; set; }
        public string Customer { get; set; }

    }
}
