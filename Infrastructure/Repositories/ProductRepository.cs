using Application.Abstractions.Catalogue;
using Application.DTOs.Product;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly POSDbContext _dbcontext;

        public ProductRepository(POSDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        // ? MÉTODO AUXILIAR PARA CONVERTIR Product a Products
        private Products ConvertToProducts(Product product)
        {
            var products = new Products();
            
            // Copiar todas las propiedades
            foreach (var prop in typeof(Product).GetProperties())
            {
                if (prop.CanWrite)
                {
                    var value = prop.GetValue(product);
                    prop.SetValue(products, value);
                }
            }
            
            return products;
        }

        public async Task<Products> CreateAsync(Products product)
        {
            await _dbcontext.Products.AddAsync(product);
            await _dbcontext.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteAsync(int productID)
        {
            var existingProduct = await _dbcontext.Products.FirstOrDefaultAsync(c => c.ID == productID);
            if (existingProduct != null)
            {
                _dbcontext.Products.Remove(existingProduct);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        // ? VERSIÓN MEJORADA CON CONTEO TOTAL Y MEJOR FILTRADO
        public async Task<IEnumerable<Products>> GetByPageAsync(ProductPageQuery query)
        {
            var pageSize = query.Size ?? 10;
            var pageNumber = query.Nro ?? 1;

            IQueryable<Product> queryable = _dbcontext.Products
                .Include(p => p.Category)
                .Include(p => p.Subcategory)
                .Include(p => p.PrimarySupplier)
                .AsQueryable();

            // Filtros
            if (!string.IsNullOrWhiteSpace(query.search))
            {
                queryable = queryable.Where(p => 
                    p.name.Contains(query.search) ||
                    p.code.Contains(query.search) ||
                    (p.description != null && p.description.Contains(query.search)) ||
                    (p.barcode != null && p.barcode.Contains(query.search)) ||
                    (p.Brand != null && p.Brand.Contains(query.search))
                );
            }

            var results = await queryable
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ? CORREGIDO: Convertir usando método auxiliar
            return results.Select(ConvertToProducts);
        }

        // ? NUEVO MÉTODO PARA OBTENER CONTEO TOTAL
        public async Task<int> GetTotalCountAsync(ProductPageQuery query)
        {
            IQueryable<Product> queryable = _dbcontext.Products.AsQueryable();

            // Aplicar mismos filtros que en GetByPageAsync
            if (!string.IsNullOrWhiteSpace(query.search))
            {
                queryable = queryable.Where(p => 
                    p.name.Contains(query.search) ||
                    p.code.Contains(query.search) ||
                    (p.description != null && p.description.Contains(query.search)) ||
                    (p.barcode != null && p.barcode.Contains(query.search)) ||
                    (p.Brand != null && p.Brand.Contains(query.search))
                );
            }

            if (query.CategoryId.HasValue)
            {
                queryable = queryable.Where(p => p.CategoryId == query.CategoryId.Value);
            }

            if (query.IsActive.HasValue)
            {
                queryable = queryable.Where(p => p.IsActive == query.IsActive.Value);
            }

            return await queryable.CountAsync();
        }

        // ?? NUEVO MÉTODO PARA OBTENER PRODUCTOS PAGINADOS CON CONTEO
        public async Task<(IEnumerable<Products> Products, int TotalCount)> GetPagedWithCountAsync(ProductPageQuery query)
        {
            var pageSize = query.Size ?? 10;
            var pageNumber = query.Nro ?? 1;

            IQueryable<Product> queryable = _dbcontext.Products
                .Include(p => p.Category)
                .Include(p => p.Subcategory)
                .Include(p => p.PrimarySupplier)
                .Include(p => p.CreatedBy)
                .AsQueryable();

            // Filtros
            if (!string.IsNullOrWhiteSpace(query.search))
            {
                queryable = queryable.Where(p => 
                    p.name.Contains(query.search) ||
                    p.code.Contains(query.search) ||
                    (p.description != null && p.description.Contains(query.search)) ||
                    (p.barcode != null && p.barcode.Contains(query.search)) ||
                    (p.Brand != null && p.Brand.Contains(query.search))
                );
            }

            if (query.CategoryId.HasValue)
            {
                queryable = queryable.Where(p => p.CategoryId == query.CategoryId.Value);
            }

            if (query.IsActive.HasValue)
            {
                queryable = queryable.Where(p => p.IsActive == query.IsActive.Value);
            }

            // ? NUEVO: Filtros de inventario
            // NOTA: warehouseId NO filtra productos, solo afecta qué stock se muestra
            
            if (query.OnlyWithStock.HasValue && query.OnlyWithStock.Value)
            {
                if (query.WarehouseId.HasValue)
                {
                    // Stock en un almacén específico
                    var productIdsWithStock = await _dbcontext.ProductStock
                        .Where(ps => ps.WarehouseId == query.WarehouseId.Value && ps.Quantity > 0)
                        .Select(ps => ps.ProductId)
                        .ToListAsync();

                    queryable = queryable.Where(p => productIdsWithStock.Contains(p.ID));
                }
                else
                {
                    // Stock total en cualquier almacén
                    var productIdsWithStock = await _dbcontext.ProductStock
                        .Where(ps => ps.Quantity > 0)
                        .Select(ps => ps.ProductId)
                        .Distinct()
                        .ToListAsync();

                    queryable = queryable.Where(p => productIdsWithStock.Contains(p.ID));
                }
            }

            if (query.OnlyBelowMinimum.HasValue && query.OnlyBelowMinimum.Value)
            {
                if (query.WarehouseId.HasValue)
                {
                    var productIdsBelowMin = await _dbcontext.ProductStock
                        .Where(ps => ps.WarehouseId == query.WarehouseId.Value && 
                                    ps.Quantity < (ps.MinimumStock ?? 0))
                        .Select(ps => ps.ProductId)
                        .ToListAsync();

                    queryable = queryable.Where(p => productIdsBelowMin.Contains(p.ID));
                }
                else
                {
                    var productIdsBelowMin = await _dbcontext.ProductStock
                        .Where(ps => ps.Quantity < (ps.MinimumStock ?? 0))
                        .Select(ps => ps.ProductId)
                        .Distinct()
                        .ToListAsync();

                    queryable = queryable.Where(p => productIdsBelowMin.Contains(p.ID));
                }
            }

            // Obtener conteo total antes de paginación
            var totalCount = await queryable.CountAsync();

            // Aplicar ordenamiento
            queryable = query.SortBy?.ToLower() switch
            {
                "price" => query.SortOrder?.ToLower() == "desc" 
                    ? queryable.OrderByDescending(p => p.price)
                    : queryable.OrderBy(p => p.price),
                "createdat" => query.SortOrder?.ToLower() == "desc"
                    ? queryable.OrderByDescending(p => p.CreatedAt)
                    : queryable.OrderBy(p => p.CreatedAt),
                "code" => query.SortOrder?.ToLower() == "desc"
                    ? queryable.OrderByDescending(p => p.code)
                    : queryable.OrderBy(p => p.code),
                _ => query.SortOrder?.ToLower() == "desc"
                    ? queryable.OrderByDescending(p => p.name)
                    : queryable.OrderBy(p => p.name)
            };

            // Aplicar paginación
            var results = await queryable
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ?? CORREGIDO: Convertir usando método auxiliar
            var products = results.Select(ConvertToProducts);

            return (products, totalCount);
        }

        // ? MÉTODO PARA ESTADÍSTICAS DE PRODUCTOS
        public async Task<(int Total, int Active, int Inactive, decimal TotalValue, int LowStock)> GetStatisticsAsync()
        {
            var total = await _dbcontext.Products.CountAsync();
            var active = await _dbcontext.Products.CountAsync(p => p.IsActive);
            var inactive = total - active;
            
            // ? CORREGIDO: Evitar división por cero y valores nulos
            var totalValue = await _dbcontext.Products
                .Where(p => p.price > 0 && p.MinimumStock > 0 && p.MaximumStock > 0)
                .SumAsync(p => p.price * ((p.MinimumStock + p.MaximumStock) / 2));
                
            var lowStock = await _dbcontext.Products
                .CountAsync(p => p.MinimumStock > 0 && p.MaximumStock > 0 && 
                           ((p.MinimumStock + p.MaximumStock) / 2) <= p.MinimumStock);

            return (total, active, inactive, totalValue, lowStock);
        }

        // ? MÉTODO PARA TOP CATEGORÍAS
        public async Task<List<CategoryStats>> GetTopCategoriesAsync(int count = 5)
        {
            return await _dbcontext.ProductCategories
                .Where(c => c.IsActive)
                .Select(c => new CategoryStats
                {
                    CategoryName = c.Name,
                    ProductCount = c.Products.Count(p => p.IsActive),
                    TotalValue = c.Products.Where(p => p.IsActive).Sum(p => p.price)
                })
                .OrderByDescending(c => c.ProductCount)
                .Take(count)
                .ToListAsync();
        }

        // ? MÉTODO PARA PRODUCTOS SIN STOCK
        public async Task<int> GetOutOfStockCountAsync()
        {
            return await _dbcontext.Products.CountAsync(p => p.MaximumStock == 0);
        }

        // ? NUEVO: Obtener stock por bodega para un producto
        public async Task<List<ProductStock>> GetProductStockByWarehouseAsync(int productId, int? warehouseId = null)
        {
            IQueryable<ProductStock> query = _dbcontext.ProductStock
                .Include(ps => ps.Warehouse)
                    .ThenInclude(w => w.Branch)
                .Where(ps => ps.ProductId == productId);

            if (warehouseId.HasValue)
            {
                query = query.Where(ps => ps.WarehouseId == warehouseId.Value);
            }

            return await query
                .OrderBy(ps => ps.Warehouse.Name)
                .ToListAsync();
        }

        // ? NUEVO: Obtener total de stock de un producto en todas las bodegas
        public async Task<decimal> GetTotalStockByProductAsync(int productId)
        {
            var total = await _dbcontext.ProductStock
                .Where(ps => ps.ProductId == productId && ps.Warehouse.IsActive)
                .SumAsync(ps => ps.Quantity);

            return total;
        }

        // ? NUEVO: Obtener stock por bodega o crear entradas con 0 si no existe
        public async Task<List<ProductWarehouseStockDto>> GetWarehouseStockOrZeroAsync(
            int productId, 
            int? warehouseId, 
            List<ProductStock> existingStock)
        {
            var result = new List<ProductWarehouseStockDto>();

            if (existingStock.Any())
            {
                // Hay stock, mapear normalmente
                result = existingStock.Select(s => new ProductWarehouseStockDto
                {
                    WarehouseId = s.WarehouseId,
                    WarehouseCode = s.Warehouse.Code,
                    WarehouseName = s.Warehouse.Name,
                    Quantity = s.Quantity,
                    MinimumStock = s.MinimumStock,
                    MaximumStock = s.MaximumStock,
                    LastMovementDate = s.LastMovementDate,
                    BranchName = s.Warehouse.Branch?.Name,
                    IsActive = s.Warehouse.IsActive
                }).ToList();
            }
            else if (warehouseId.HasValue)
            {
                // No hay stock pero se especificó un almacén, mostrar 0 para ese almacén
                var warehouse = await _dbcontext.Warehouses
                    .Include(w => w.Branch)
                    .FirstOrDefaultAsync(w => w.Id == warehouseId.Value);

                if (warehouse != null)
                {
                    result.Add(new ProductWarehouseStockDto
                    {
                        WarehouseId = warehouse.Id,
                        WarehouseCode = warehouse.Code,
                        WarehouseName = warehouse.Name,
                        Quantity = 0,
                        MinimumStock = null,
                        MaximumStock = null,
                        LastMovementDate = null,
                        BranchName = warehouse.Branch?.Name,
                        IsActive = warehouse.IsActive
                    });
                }
            }
            else
            {
                // No hay stock y no se especificó almacén, mostrar 0 para todos los almacenes activos
                var allWarehouses = await _dbcontext.Warehouses
                    .Include(w => w.Branch)
                    .Where(w => w.IsActive)
                    .OrderBy(w => w.Name)
                    .ToListAsync();

                result = allWarehouses.Select(w => new ProductWarehouseStockDto
                {
                    WarehouseId = w.Id,
                    WarehouseCode = w.Code,
                    WarehouseName = w.Name,
                    Quantity = 0,
                    MinimumStock = null,
                    MaximumStock = null,
                    LastMovementDate = null,
                    BranchName = w.Branch?.Name,
                    IsActive = w.IsActive
                }).ToList();
            }

            return result;
        }

        public async Task<Products?> GetByIdAsync(int productID)
        {
            var product = await _dbcontext.Products
                .Include(p => p.Category)
                .Include(p => p.Subcategory)
                .Include(p => p.PrimarySupplier)
                .Include(p => p.CreatedBy)
                .Include(p => p.UpdatedBy)
                .FirstOrDefaultAsync(c => c.ID == productID);
                
            return product != null ? ConvertToProducts(product) : null;
        }

        public async Task<Products?> UpdateAsync(Products product)
        {
            var existingProduct = await _dbcontext.Products.FirstOrDefaultAsync(c => c.ID == product.ID);
            if (existingProduct != null)
            {
                existingProduct.name = product.name;
                existingProduct.description = product.description;
                existingProduct.code = product.code;
                existingProduct.barcode = product.barcode;
                existingProduct.price = product.price;
                existingProduct.Brand = product.Brand;
                existingProduct.Model = product.Model;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.SubcategoryId = product.SubcategoryId;
                existingProduct.SatCode = product.SatCode;
                existingProduct.SatUnit = product.SatUnit;
                existingProduct.BaseCost = product.BaseCost;
                existingProduct.TaxRate = product.TaxRate;
                existingProduct.MinimumStock = product.MinimumStock;
                existingProduct.MaximumStock = product.MaximumStock;
                existingProduct.ReorderPoint = product.ReorderPoint;
                existingProduct.Unit = product.Unit;
                existingProduct.Weight = product.Weight;
                existingProduct.Length = product.Length;
                existingProduct.Width = product.Width;
                existingProduct.Height = product.Height;
                existingProduct.Color = product.Color;
                existingProduct.Size = product.Size;
                existingProduct.IsActive = product.IsActive;
                existingProduct.IsService = product.IsService;
                existingProduct.AllowFractionalQuantities = product.AllowFractionalQuantities;
                existingProduct.TrackSerial = product.TrackSerial;
                existingProduct.TrackExpiry = product.TrackExpiry;
                existingProduct.PrimarySupplierId = product.PrimarySupplierId;
                existingProduct.SupplierCode = product.SupplierCode;
                existingProduct.UpdatedAt = DateTime.UtcNow;
                existingProduct.UpdatedByUserId = product.UpdatedByUserId;

                await _dbcontext.SaveChangesAsync();
                return product;
            }
            return null;
        }
    }
}