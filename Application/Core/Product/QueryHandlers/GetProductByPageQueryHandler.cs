using Application.Abstractions.Catalogue;
using Application.Core.Product.Queries;
using Application.DTOs.Product;
using MediatR;

namespace Application.Core.Product.QueryHandlers
{
    public class GetProductByPageQueryHandler : IRequestHandler<GetProductByPageQuery, GetProductsPagedResponseDto>
    {
        private readonly IProductRepository _repository;

        public GetProductByPageQueryHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<GetProductsPagedResponseDto> Handle(GetProductByPageQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Convertir de GetProductByPageQuery a ProductPageQuery
                var repoQuery = new ProductPageQuery
                {
                    Size = request.PageSize,
                    Nro = request.Page,
                    search = request.Search,
                    CategoryId = request.CategoryId,
                    IsActive = request.IsActive,
                    SortBy = request.SortBy,
                    SortOrder = request.SortOrder
                };

                // Obtener productos con conteo total
                var (products, totalCount) = await _repository.GetPagedWithCountAsync(repoQuery);
                var productsList = products.ToList();

                // Mapear a ProductTableDto
                var mappedProducts = productsList.Select(p => new ProductTableDto
                {
                    ID = p.ID,
                    Code = p.code,
                    Name = p.name,
                    Brand = p.Brand,
                    Model = p.Model,
                    CategoryName = p.Category?.Name,
                    Price = p.price,
                    BaseCost = p.BaseCost,
                    MinimumStock = p.MinimumStock,
                    MaximumStock = p.MaximumStock,
                    Location = p.Location,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    ABCClassification = p.ABCClassification,
                    VelocityCode = p.VelocityCode,
                    LastSaleDate = p.LastSaleDate,
                    TotalSalesQuantity = p.TotalSalesQuantity
                }).ToList();

                // Obtener estadísticas
                var (totalProducts, activeProducts, inactiveProducts, totalValue, lowStockProducts) = 
                    await _repository.GetStatisticsAsync();

                // Obtener top categorías usando el repositorio
                var topCategories = await _repository.GetTopCategoriesAsync(5);

                return new GetProductsPagedResponseDto
                {
                    Message = "Productos obtenidos exitosamente",
                    Error = 0,
                    Data = mappedProducts,
                    Pagination = new PaginationMetadata
                    {
                        Page = request.Page,
                        PageSize = request.PageSize,
                        TotalItems = totalCount
                    },
                    Statistics = new ProductsStatistics
                    {
                        TotalProducts = totalProducts,
                        ActiveProducts = activeProducts,
                        InactiveProducts = inactiveProducts,
                        TotalValue = totalValue,
                        LowStockProducts = lowStockProducts,
                        OutOfStockProducts = await _repository.GetOutOfStockCountAsync(),
                        TopCategories = topCategories
                    }
                };
            }
            catch (Exception ex)
            {
                return new GetProductsPagedResponseDto
                {
                    Message = $"Error al obtener productos: {ex.Message}",
                    Error = 2,
                    Data = new List<ProductTableDto>(),
                    Pagination = new PaginationMetadata(),
                    Statistics = new ProductsStatistics()
                };
            }
        }
    }
}
