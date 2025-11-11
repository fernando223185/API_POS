using Application.Abstractions.Messaging;
using Application.DTOs.Product;
using MediatR;

namespace Application.Core.Product.Commands
{
    public class CreateProductCommand : IRequest<ProductResponseDto>
    {
        public CreateProductRequestDto ProductData { get; set; }
        public int CreatedByUserId { get; set; }

        public CreateProductCommand(CreateProductRequestDto productData, int createdByUserId)
        {
            ProductData = productData;
            CreatedByUserId = createdByUserId;
        }
    }
}
