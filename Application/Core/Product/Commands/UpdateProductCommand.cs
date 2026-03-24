
using Application.Abstractions.Messaging;
using Application.DTOs.Product;
using MediatR;

namespace Application.Core.Product.Commands
{
    /// <summary>
    /// Comando CQRS para actualizar un producto existente
    /// </summary>
    public class UpdateProductCommand : IRequest<ProductResponseDto>
    {
        public int ProductId { get; set; }
        public UpdateProductRequestDto ProductData { get; set; }
        public int UpdatedByUserId { get; set; }

        public UpdateProductCommand(int productId, UpdateProductRequestDto productData, int updatedByUserId)
        {
            ProductId = productId;
            ProductData = productData;
            UpdatedByUserId = updatedByUserId;
        }
    }
}
