using Application.Abstractions.Catalogue;
using Application.Core.Product.Queries;
using Application.DTOs.Product;
using MediatR;

namespace Application.Core.Product.QueryHandlers
{
    public class GetProductByPageQueryHandler : IRequestHandler<GetProductByPageQuery, ProductPagedResponseDto>
    {
        private readonly IProductRepository _repository;
        private readonly IProductImageRepository _imageRepository;

        public GetProductByPageQueryHandler(
            IProductRepository repository,
            IProductImageRepository imageRepository)
        {
            _repository = repository;
            _imageRepository = imageRepository;
        }

        public async Task<ProductPagedResponseDto> Handle(GetProductByPageQuery request, CancellationToken cancellationToken)
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
                    SortOrder = request.SortDirection,
                    // ✅ NUEVO: Parámetros de inventario
                    IncludeWarehouseStock = request.IncludeWarehouseStock,
                    WarehouseId = request.WarehouseId,
                    OnlyWithStock = request.OnlyWithStock,
                    OnlyBelowMinimum = request.OnlyBelowMinimum
                };

                // Obtener productos con conteo total
                var (products, totalCount) = await _repository.GetPagedWithCountAsync(repoQuery);
                var productsList = products.ToList();

                // Obtener IDs de productos para buscar imágenes
                var productIds = productsList.Select(p => p.ID).ToList();

                // Obtener imágenes principales de todos los productos
                var primaryImages = new Dictionary<int, Domain.Entities.ProductImage?>();
                foreach (var productId in productIds)
                {
                    var image = await _imageRepository.GetPrimaryImageAsync(productId);
                    primaryImages[productId] = image;
                }

                // ✅ NUEVO: Obtener stock por bodega si se solicita
                Dictionary<int, List<ProductWarehouseStockDto>>? productsStock = null;
                if (request.IncludeWarehouseStock)
                {
                    productsStock = new Dictionary<int, List<ProductWarehouseStockDto>>();
                    foreach (var productId in productIds)
                    {
                        var stock = await _repository.GetProductStockByWarehouseAsync(productId, request.WarehouseId);
                        
                        // ✅ NUEVO: Mapear stock, incluyendo productos sin stock
                        productsStock[productId] = await _repository.GetWarehouseStockOrZeroAsync(productId, request.WarehouseId, stock);
                    }
                }

                // Mapear a ProductResponseDto
                var mappedProducts = productsList.Select(p => new ProductResponseDto
                {
                    ID = p.ID,
                    Code = p.code,
                    Name = p.name,
                    Description = p.description,
                    Brand = p.Brand,
                    Model = p.Model,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name,
                    SubcategoryId = p.SubcategoryId,
                    SubcategoryName = p.Subcategory?.Name,
                    Price = p.price,
                    BaseCost = p.BaseCost,
                    TaxRate = p.TaxRate,
                    MinimumStock = p.MinimumStock,
                    MaximumStock = p.MaximumStock,
                    ReorderPoint = p.ReorderPoint,
                    Location = p.Location,
                    IsActive = p.IsActive,
                    IsService = p.IsService,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    SatCode = p.SatCode ?? "01010101",
                    SatUnit = p.SatUnit,
                    SatTaxType = p.SatTaxType,
                    Barcode = p.barcode,
                    Unit = p.Unit,
                    
                    // Imagen
                    ImageUrl = primaryImages.ContainsKey(p.ID) && primaryImages[p.ID] != null 
                        ? primaryImages[p.ID]!.ImageUrl 
                        : null,
                    
                    // ✅ NUEVO: Stock por bodega
                    WarehouseStock = request.IncludeWarehouseStock && productsStock != null && productsStock.ContainsKey(p.ID)
                        ? productsStock[p.ID]
                        : null
                }).ToList();

                // Obtener estadísticas
                var (totalProducts, activeProducts, inactiveProducts, totalValue, lowStockProducts) = 
                    await _repository.GetStatisticsAsync();

                // Obtener top categorías
                var topCategories = await _repository.GetTopCategoriesAsync(5);

                return new ProductPagedResponseDto
                {
                    Message = "Productos obtenidos exitosamente",
                    Error = 0,
                    Data = mappedProducts,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalRecords = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                    Statistics = new ProductStatisticsDto
                    {
                        TotalProducts = totalProducts,
                        ActiveProducts = activeProducts,
                        InactiveProducts = inactiveProducts,
                        TotalValue = totalValue,
                        LowStockProducts = lowStockProducts,
                        OutOfStockProducts = await _repository.GetOutOfStockCountAsync(),
                        TopCategories = topCategories.Select(c => new CategoryStatsDto
                        {
                            CategoryName = c.CategoryName,
                            ProductCount = c.ProductCount,
                            TotalValue = c.TotalValue
                        }).ToList()
                    }
                };
            }
            catch (Exception ex)
            {
                return new ProductPagedResponseDto
                {
                    Message = $"Error al obtener productos: {ex.Message}",
                    Error = 2,
                    Data = new List<ProductResponseDto>(),
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalRecords = 0,
                    TotalPages = 0
                };
            }
        }
    }
}
