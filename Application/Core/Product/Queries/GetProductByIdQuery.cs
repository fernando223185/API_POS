using Application.DTOs.Product;
using MediatR;

namespace Application.Core.Product.Queries
{
    /// <summary>
    /// Query CQRS para obtener un producto por ID con toda su información
    /// </summary>
    public class GetProductByIdQuery : IRequest<ProductResponseDto>
    {
        public int ID { get; set; }
    }
}
