using Application.Abstractions.Catalogue;
using Application.Core.Product.Commands;
using Application.DTOs.Product;
using Domain.Entities;
using MediatR;

namespace Application.Core.Product.CommandHandlers
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductResponseDto>
    {
        private readonly IProductRepository _productRepository;

        public CreateProductCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductResponseDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            // Validar que el código sea único
            var existingProduct = await _productRepository.GetByPageAsync(new Application.Abstractions.Catalogue.ProductPageQuery
            {
                search = request.ProductData.Code,
                Size = 1,
                Nro = 1
            });

            if (existingProduct.Any(p => p.code == request.ProductData.Code))
            {
                throw new InvalidOperationException($"Ya existe un producto con el código: {request.ProductData.Code}");
            }

            // Crear la entidad Product
            var product = new Products
            {
                name = request.ProductData.Name,
                description = request.ProductData.Description,
                code = request.ProductData.Code,
                barcode = request.ProductData.Barcode,
                Brand = request.ProductData.Brand,
                Model = request.ProductData.Model,
                CategoryId = request.ProductData.CategoryId,
                SubcategoryId = request.ProductData.SubcategoryId,
                SatCode = request.ProductData.SatCode,
                SatUnit = request.ProductData.SatUnit,
                BaseCost = request.ProductData.BaseCost,
                price = request.ProductData.Price,
                TaxRate = request.ProductData.TaxRate,
                MinimumStock = request.ProductData.MinimumStock,
                MaximumStock = request.ProductData.MaximumStock,
                ReorderPoint = request.ProductData.ReorderPoint,
                Unit = request.ProductData.Unit,
                Weight = request.ProductData.Weight,
                Length = request.ProductData.Length,
                Width = request.ProductData.Width,
                Height = request.ProductData.Height,
                Color = request.ProductData.Color,
                Size = request.ProductData.Size,
                IsActive = request.ProductData.IsActive,
                IsService = request.ProductData.IsService,
                AllowFractionalQuantities = request.ProductData.AllowFractionalQuantities,
                TrackSerial = request.ProductData.TrackSerial,
                TrackExpiry = request.ProductData.TrackExpiry,
                PrimarySupplierId = request.ProductData.PrimarySupplierId,
                SupplierCode = request.ProductData.SupplierCode,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = request.CreatedByUserId
            };

            // Guardar en la base de datos
            var createdProduct = await _productRepository.CreateAsync(product);

            // Mapear a DTO de respuesta
            var response = new ProductResponseDto
            {
                ID = createdProduct.ID, // ✅ CORREGIDO: ID en lugar de Id
                Name = createdProduct.name,
                Description = createdProduct.description,
                Code = createdProduct.code,
                Barcode = createdProduct.barcode,
                Brand = createdProduct.Brand,
                Model = createdProduct.Model,
                CategoryId = createdProduct.CategoryId,
                SubcategoryId = createdProduct.SubcategoryId,
                SatCode = createdProduct.SatCode ?? "01010101",
                SatUnit = createdProduct.SatUnit ?? "PZA",
                SatTaxType = createdProduct.SatTaxType ?? "Tasa",
                CustomsCode = createdProduct.CustomsCode,
                CountryOfOrigin = createdProduct.CountryOfOrigin ?? "México",
                BaseCost = createdProduct.BaseCost,
                Price = createdProduct.price,
                TaxRate = createdProduct.TaxRate,
                PriceWithTax = createdProduct.price * (1 + createdProduct.TaxRate), // ✅ CORREGIDO: Cálculo directo
                MinimumStock = createdProduct.MinimumStock,
                MaximumStock = createdProduct.MaximumStock,
                ReorderPoint = createdProduct.ReorderPoint,
                Unit = createdProduct.Unit,
                Weight = createdProduct.Weight,
                Length = createdProduct.Length,
                Width = createdProduct.Width,
                Height = createdProduct.Height,
                Color = createdProduct.Color,
                Size = createdProduct.Size,

                // ✅ NUEVOS CAMPOS AVANZADOS
                Warranty = createdProduct.Warranty,
                WarrantyType = createdProduct.WarrantyType,
                Location = createdProduct.Location,
                Aisle = createdProduct.Aisle,
                Shelf = createdProduct.Shelf,
                Bin = createdProduct.Bin,
                Tags = createdProduct.Tags,
                Season = createdProduct.Season,
                TargetGender = createdProduct.TargetGender,
                AgeGroup = createdProduct.AgeGroup,
                MaxQuantityPerSale = createdProduct.MaxQuantityPerSale,
                MinQuantityPerSale = createdProduct.MinQuantityPerSale,
                SalesNotes = createdProduct.SalesNotes,
                IsDiscountAllowed = createdProduct.IsDiscountAllowed,
                MaxDiscountPercentage = createdProduct.MaxDiscountPercentage,
                SEOTitle = createdProduct.SEOTitle,
                SEODescription = createdProduct.SEODescription,
                SEOKeywords = createdProduct.SEOKeywords,
                IsWebVisible = createdProduct.IsWebVisible,
                IsFeatured = createdProduct.IsFeatured,
                LaunchDate = createdProduct.LaunchDate,
                DiscontinuedDate = createdProduct.DiscontinuedDate,
                TechnicalSpecs = createdProduct.TechnicalSpecs,
                ManufacturerPartNumber = createdProduct.ManufacturerPartNumber,
                UPC = createdProduct.UPC,
                EAN = createdProduct.EAN,
                ISBN = createdProduct.ISBN,
                IsFragile = createdProduct.IsFragile,
                RequiresSpecialHandling = createdProduct.RequiresSpecialHandling,
                ShippingClass = createdProduct.ShippingClass,
                PackageLength = createdProduct.PackageLength,
                PackageWidth = createdProduct.PackageWidth,
                PackageHeight = createdProduct.PackageHeight,
                PackageWeight = createdProduct.PackageWeight,
                QualityGrade = createdProduct.QualityGrade,
                LastQualityCheck = createdProduct.LastQualityCheck,
                DefectRate = createdProduct.DefectRate,
                ReturnRate = createdProduct.ReturnRate,
                ABCClassification = createdProduct.ABCClassification,
                VelocityCode = createdProduct.VelocityCode,
                ProfitMarginPercentage = createdProduct.ProfitMarginPercentage,
                LastSaleDate = createdProduct.LastSaleDate,
                TotalSalesQuantity = createdProduct.TotalSalesQuantity,
                InternalNotes = createdProduct.InternalNotes,
                CustomerNotes = createdProduct.CustomerNotes,
                MaintenanceInstructions = createdProduct.MaintenanceInstructions,
                SafetyWarnings = createdProduct.SafetyWarnings,

                IsActive = createdProduct.IsActive,
                IsService = createdProduct.IsService,
                AllowFractionalQuantities = createdProduct.AllowFractionalQuantities,
                TrackSerial = createdProduct.TrackSerial,
                TrackExpiry = createdProduct.TrackExpiry,
                PrimarySupplierId = createdProduct.PrimarySupplierId,
                SupplierCode = createdProduct.SupplierCode,
                CreatedAt = createdProduct.CreatedAt,
                UpdatedAt = createdProduct.UpdatedAt,
                CreatedByName = createdProduct.CreatedBy?.Name,
                UpdatedByName = createdProduct.UpdatedBy?.Name
            };

            return response;
        }
    }
}
