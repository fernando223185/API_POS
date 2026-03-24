using Application.Abstractions.Catalogue;
using Application.Core.Product.Commands;
using Application.DTOs.Product;
using Domain.Entities;
using MediatR;

namespace Application.Core.Product.CommandHandlers
{
    /// <summary>
    /// Handler CQRS para actualizar un producto existente
    /// </summary>
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductResponseDto>
    {
        private readonly IProductRepository _productRepository;

        public UpdateProductCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductResponseDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            // ✅ 1. Verificar que el producto existe
            var existingProduct = await _productRepository.GetByIdAsync(request.ProductId);
            
            if (existingProduct == null)
            {
                throw new InvalidOperationException($"No se encontró el producto con ID: {request.ProductId}");
            }

            // ✅ 2. Verificar que el código sea único (si se cambió)
            if (existingProduct.code != request.ProductData.Code)
            {
                var codeCheck = await _productRepository.GetByPageAsync(new Application.Abstractions.Catalogue.ProductPageQuery
                {
                    search = request.ProductData.Code,
                    Size = 1,
                    Nro = 1
                });

                if (codeCheck.Any(p => p.code == request.ProductData.Code && p.ID != request.ProductId))
                {
                    throw new InvalidOperationException($"Ya existe otro producto con el código: {request.ProductData.Code}");
                }
            }

            // ✅ 3. Actualizar los campos del producto
            existingProduct.name = request.ProductData.Name;
            existingProduct.description = request.ProductData.Description;
            existingProduct.code = request.ProductData.Code;
            existingProduct.barcode = request.ProductData.Barcode;
            existingProduct.Brand = request.ProductData.Brand;
            existingProduct.Model = request.ProductData.Model;
            existingProduct.CategoryId = request.ProductData.CategoryId;
            existingProduct.SubcategoryId = request.ProductData.SubcategoryId;
            existingProduct.SatCode = request.ProductData.SatCode;
            existingProduct.SatUnit = request.ProductData.SatUnit;
            existingProduct.SatTaxType = request.ProductData.SatTaxType;
            existingProduct.CustomsCode = request.ProductData.CustomsCode;
            existingProduct.CountryOfOrigin = request.ProductData.CountryOfOrigin;
            existingProduct.BaseCost = request.ProductData.BaseCost;
            existingProduct.price = request.ProductData.Price;
            existingProduct.TaxRate = request.ProductData.TaxRate;
            existingProduct.MinimumStock = request.ProductData.MinimumStock;
            existingProduct.MaximumStock = request.ProductData.MaximumStock;
            existingProduct.ReorderPoint = request.ProductData.ReorderPoint;
            existingProduct.Unit = request.ProductData.Unit;
            existingProduct.Weight = request.ProductData.Weight;
            existingProduct.Length = request.ProductData.Length;
            existingProduct.Width = request.ProductData.Width;
            existingProduct.Height = request.ProductData.Height;
            existingProduct.Color = request.ProductData.Color;
            existingProduct.Size = request.ProductData.Size;

            // ✅ CAMPOS AVANZADOS
            existingProduct.Warranty = request.ProductData.Warranty;
            existingProduct.WarrantyType = request.ProductData.WarrantyType;
            existingProduct.Location = request.ProductData.Location;
            existingProduct.Aisle = request.ProductData.Aisle;
            existingProduct.Shelf = request.ProductData.Shelf;
            existingProduct.Bin = request.ProductData.Bin;
            existingProduct.Tags = request.ProductData.Tags;
            existingProduct.Season = request.ProductData.Season;
            existingProduct.TargetGender = request.ProductData.TargetGender;
            existingProduct.AgeGroup = request.ProductData.AgeGroup;
            existingProduct.MaxQuantityPerSale = request.ProductData.MaxQuantityPerSale;
            existingProduct.MinQuantityPerSale = request.ProductData.MinQuantityPerSale;
            existingProduct.SalesNotes = request.ProductData.SalesNotes;
            existingProduct.IsDiscountAllowed = request.ProductData.IsDiscountAllowed;
            existingProduct.MaxDiscountPercentage = request.ProductData.MaxDiscountPercentage;
            existingProduct.SEOTitle = request.ProductData.SEOTitle;
            existingProduct.SEODescription = request.ProductData.SEODescription;
            existingProduct.SEOKeywords = request.ProductData.SEOKeywords;
            existingProduct.IsWebVisible = request.ProductData.IsWebVisible;
            existingProduct.IsFeatured = request.ProductData.IsFeatured;
            existingProduct.LaunchDate = request.ProductData.LaunchDate;
            existingProduct.DiscontinuedDate = request.ProductData.DiscontinuedDate;
            existingProduct.TechnicalSpecs = request.ProductData.TechnicalSpecs;
            existingProduct.ManufacturerPartNumber = request.ProductData.ManufacturerPartNumber;
            existingProduct.UPC = request.ProductData.UPC;
            existingProduct.EAN = request.ProductData.EAN;
            existingProduct.ISBN = request.ProductData.ISBN;
            existingProduct.IsFragile = request.ProductData.IsFragile;
            existingProduct.RequiresSpecialHandling = request.ProductData.RequiresSpecialHandling;
            existingProduct.ShippingClass = request.ProductData.ShippingClass;
            existingProduct.PackageLength = request.ProductData.PackageLength;
            existingProduct.PackageWidth = request.ProductData.PackageWidth;
            existingProduct.PackageHeight = request.ProductData.PackageHeight;
            existingProduct.PackageWeight = request.ProductData.PackageWeight;
            existingProduct.QualityGrade = request.ProductData.QualityGrade;
            existingProduct.LastQualityCheck = request.ProductData.LastQualityCheck;
            existingProduct.DefectRate = request.ProductData.DefectRate;
            existingProduct.ReturnRate = request.ProductData.ReturnRate;
            existingProduct.ABCClassification = request.ProductData.ABCClassification;
            existingProduct.VelocityCode = request.ProductData.VelocityCode;
            existingProduct.ProfitMarginPercentage = request.ProductData.ProfitMarginPercentage;
            existingProduct.LastSaleDate = request.ProductData.LastSaleDate;
            existingProduct.TotalSalesQuantity = request.ProductData.TotalSalesQuantity;
            existingProduct.InternalNotes = request.ProductData.InternalNotes;
            existingProduct.CustomerNotes = request.ProductData.CustomerNotes;
            existingProduct.MaintenanceInstructions = request.ProductData.MaintenanceInstructions;
            existingProduct.SafetyWarnings = request.ProductData.SafetyWarnings;

            // ✅ CONFIGURACIÓN
            existingProduct.IsActive = request.ProductData.IsActive;
            existingProduct.IsService = request.ProductData.IsService;
            existingProduct.AllowFractionalQuantities = request.ProductData.AllowFractionalQuantities;
            existingProduct.TrackSerial = request.ProductData.TrackSerial;
            existingProduct.TrackExpiry = request.ProductData.TrackExpiry;
            existingProduct.PrimarySupplierId = request.ProductData.PrimarySupplierId;
            existingProduct.SupplierCode = request.ProductData.SupplierCode;

            // ✅ ACTUALIZAR METADATA
            existingProduct.UpdatedAt = DateTime.UtcNow;
            existingProduct.UpdatedByUserId = request.UpdatedByUserId;

            // ✅ 4. Guardar cambios en la base de datos
            var updatedProduct = await _productRepository.UpdateAsync(existingProduct);

            // ✅ 5. Mapear a DTO de respuesta
            var response = new ProductResponseDto
            {
                ID = updatedProduct.ID,
                Name = updatedProduct.name,
                Description = updatedProduct.description,
                Code = updatedProduct.code,
                Barcode = updatedProduct.barcode,
                Brand = updatedProduct.Brand,
                Model = updatedProduct.Model,
                CategoryId = updatedProduct.CategoryId,
                SubcategoryId = updatedProduct.SubcategoryId,
                SatCode = updatedProduct.SatCode ?? "01010101",
                SatUnit = updatedProduct.SatUnit ?? "PZA",
                SatTaxType = updatedProduct.SatTaxType ?? "Tasa",
                CustomsCode = updatedProduct.CustomsCode,
                CountryOfOrigin = updatedProduct.CountryOfOrigin ?? "México",
                BaseCost = updatedProduct.BaseCost,
                Price = updatedProduct.price,
                TaxRate = updatedProduct.TaxRate,
                PriceWithTax = updatedProduct.price * (1 + updatedProduct.TaxRate),
                MinimumStock = updatedProduct.MinimumStock,
                MaximumStock = updatedProduct.MaximumStock,
                ReorderPoint = updatedProduct.ReorderPoint,
                Unit = updatedProduct.Unit,
                Weight = updatedProduct.Weight,
                Length = updatedProduct.Length,
                Width = updatedProduct.Width,
                Height = updatedProduct.Height,
                Color = updatedProduct.Color,
                Size = updatedProduct.Size,

                // ✅ CAMPOS AVANZADOS
                Warranty = updatedProduct.Warranty,
                WarrantyType = updatedProduct.WarrantyType,
                Location = updatedProduct.Location,
                Aisle = updatedProduct.Aisle,
                Shelf = updatedProduct.Shelf,
                Bin = updatedProduct.Bin,
                Tags = updatedProduct.Tags,
                Season = updatedProduct.Season,
                TargetGender = updatedProduct.TargetGender,
                AgeGroup = updatedProduct.AgeGroup,
                MaxQuantityPerSale = updatedProduct.MaxQuantityPerSale,
                MinQuantityPerSale = updatedProduct.MinQuantityPerSale,
                SalesNotes = updatedProduct.SalesNotes,
                IsDiscountAllowed = updatedProduct.IsDiscountAllowed,
                MaxDiscountPercentage = updatedProduct.MaxDiscountPercentage,
                SEOTitle = updatedProduct.SEOTitle,
                SEODescription = updatedProduct.SEODescription,
                SEOKeywords = updatedProduct.SEOKeywords,
                IsWebVisible = updatedProduct.IsWebVisible,
                IsFeatured = updatedProduct.IsFeatured,
                LaunchDate = updatedProduct.LaunchDate,
                DiscontinuedDate = updatedProduct.DiscontinuedDate,
                TechnicalSpecs = updatedProduct.TechnicalSpecs,
                ManufacturerPartNumber = updatedProduct.ManufacturerPartNumber,
                UPC = updatedProduct.UPC,
                EAN = updatedProduct.EAN,
                ISBN = updatedProduct.ISBN,
                IsFragile = updatedProduct.IsFragile,
                RequiresSpecialHandling = updatedProduct.RequiresSpecialHandling,
                ShippingClass = updatedProduct.ShippingClass,
                PackageLength = updatedProduct.PackageLength,
                PackageWidth = updatedProduct.PackageWidth,
                PackageHeight = updatedProduct.PackageHeight,
                PackageWeight = updatedProduct.PackageWeight,
                QualityGrade = updatedProduct.QualityGrade,
                LastQualityCheck = updatedProduct.LastQualityCheck,
                DefectRate = updatedProduct.DefectRate,
                ReturnRate = updatedProduct.ReturnRate,
                ABCClassification = updatedProduct.ABCClassification,
                VelocityCode = updatedProduct.VelocityCode,
                ProfitMarginPercentage = updatedProduct.ProfitMarginPercentage,
                LastSaleDate = updatedProduct.LastSaleDate,
                TotalSalesQuantity = updatedProduct.TotalSalesQuantity,
                InternalNotes = updatedProduct.InternalNotes,
                CustomerNotes = updatedProduct.CustomerNotes,
                MaintenanceInstructions = updatedProduct.MaintenanceInstructions,
                SafetyWarnings = updatedProduct.SafetyWarnings,

                // ✅ CONFIGURACIÓN
                IsActive = updatedProduct.IsActive,
                IsService = updatedProduct.IsService,
                AllowFractionalQuantities = updatedProduct.AllowFractionalQuantities,
                TrackSerial = updatedProduct.TrackSerial,
                TrackExpiry = updatedProduct.TrackExpiry,
                PrimarySupplierId = updatedProduct.PrimarySupplierId,
                SupplierCode = updatedProduct.SupplierCode,
                
                // ✅ METADATA
                CreatedAt = updatedProduct.CreatedAt,
                UpdatedAt = updatedProduct.UpdatedAt,
                CreatedByName = updatedProduct.CreatedBy?.Name,
                UpdatedByName = updatedProduct.UpdatedBy?.Name
            };

            return response;
        }
    }
}

