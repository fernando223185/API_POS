using Application.Abstractions.Messaging;
using Application.DTOs.Product;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Core.Product.Queries
{
    public class GetProductByPageQuery : IRequest<ProductPagedResponseDto>
    {
        [Required]
        public int Page { get; set; } = 1;
        
        public int PageSize { get; set; } = 50;
        
        public string? Search { get; set; }
        
        public int? CategoryId { get; set; }
        
        public int? SubcategoryId { get; set; }
        
        public bool? IsActive { get; set; } = true;
        
        public string? SortBy { get; set; } = "name"; // name, price, createdAt, code
        
        public string? SortDirection { get; set; } = "asc"; // asc, desc

        // ✅ NUEVO: Opciones de inventario
        /// <summary>
        /// Incluir información de stock por bodega en la respuesta
        /// </summary>
        public bool IncludeWarehouseStock { get; set; } = false;

        /// <summary>
        /// Filtrar por bodega específica (requiere IncludeWarehouseStock = true)
        /// </summary>
        public int? WarehouseId { get; set; }

        /// <summary>
        /// Mostrar solo productos con stock disponible en la bodega seleccionada
        /// </summary>
        public bool? OnlyWithStock { get; set; }

        /// <summary>
        /// Mostrar solo productos bajo mínimo de stock
        /// </summary>
        public bool? OnlyBelowMinimum { get; set; }
    }
}
