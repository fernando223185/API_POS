using Application.Abstractions.Messaging;
using Domain.Entities;

namespace Application.Core.Product.Commands
{
    public class CreateProductCommand : ICommand<Products>
    {
        public string name { get; set; }
        public string description { get; set; }
        public string code { get; set; }
        public string barcode { get; set; }
        public decimal price { get; set; }
    }
}
