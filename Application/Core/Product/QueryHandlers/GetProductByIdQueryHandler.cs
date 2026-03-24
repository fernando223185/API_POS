using System;
using System.Linq;
using Application.Abstractions.Catalogue;
using Application.Core.Product.Queries;
using Application.DTOs.Product;
using Domain.Entities;
using MediatR;

namespace Application.Core.Product.QueryHandlers
{
    /// <summary>
    /// Handler CQRS para obtener un producto por ID con toda su información incluyendo imagen primaria
    /// </summary>
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductResponseDto>
    {
        private readonly IProductRepository _repository;

        public GetProductByIdQueryHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<ProductResponseDto> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
        {
            var product = await _repository.GetByIdAsync(query.ID);

            if (product == null)
            {
                return null;
            }

            // Mapear la entidad completa a ProductResponseDto con TODOS los campos
            var response = new ProductResponseDto
            {
                ID = product.ID,
                Name = product.name,
                Description = product.description,
                Code = product.code,
                Barcode = product.barcode,
                Brand = product.Brand,
                Model = product.Model,
                
                // Clasificación
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                SubcategoryId = product.SubcategoryId,
                SubcategoryName = product.Subcategory?.Name,
                
                // Información fiscal
                SatCode = product.SatCode ?? "01010101",
                SatUnit = product.SatUnit ?? "PZA",
                SatTaxType = product.SatTaxType ?? "Tasa",
                CustomsCode = product.CustomsCode,
                CountryOfOrigin = product.CountryOfOrigin ?? "México",
                
                // Precios y costos
                BaseCost = product.BaseCost,
                Price = product.price,
                TaxRate = product.TaxRate,
                PriceWithTax = product.price * (1 + product.TaxRate),
                
                // Inventario
                MinimumStock = product.MinimumStock,
                MaximumStock = product.MaximumStock,
                ReorderPoint = product.ReorderPoint,
                Unit = product.Unit,
                
                // Características físicas
                Weight = product.Weight,
                Length = product.Length,
                Width = product.Width,
                Height = product.Height,
                Color = product.Color,
                Size = product.Size,
                
                // Información comercial avanzada
                Warranty = product.Warranty,
                WarrantyType = product.WarrantyType,
                Location = product.Location,
                Aisle = product.Aisle,
                Shelf = product.Shelf,
                Bin = product.Bin,
                
                // Clasificación avanzada
                Tags = product.Tags,
                Season = product.Season,
                TargetGender = product.TargetGender,
                AgeGroup = product.AgeGroup,
                
                // Información de ventas
                MaxQuantityPerSale = product.MaxQuantityPerSale,
                MinQuantityPerSale = product.MinQuantityPerSale,
                SalesNotes = product.SalesNotes,
                IsDiscountAllowed = product.IsDiscountAllowed,
                MaxDiscountPercentage = product.MaxDiscountPercentage,
                
                // E-commerce y marketing
                SEOTitle = product.SEOTitle,
                SEODescription = product.SEODescription,
                SEOKeywords = product.SEOKeywords,
                IsWebVisible = product.IsWebVisible,
                IsFeatured = product.IsFeatured,
                LaunchDate = product.LaunchDate,
                DiscontinuedDate = product.DiscontinuedDate,
                
                // Información técnica
                TechnicalSpecs = product.TechnicalSpecs,
                ManufacturerPartNumber = product.ManufacturerPartNumber,
                UPC = product.UPC,
                EAN = product.EAN,
                ISBN = product.ISBN,
                
                // Logística y envío
                IsFragile = product.IsFragile,
                RequiresSpecialHandling = product.RequiresSpecialHandling,
                ShippingClass = product.ShippingClass,
                PackageLength = product.PackageLength,
                PackageWidth = product.PackageWidth,
                PackageHeight = product.PackageHeight,
                PackageWeight = product.PackageWeight,
                
                // Control de calidad
                QualityGrade = product.QualityGrade,
                LastQualityCheck = product.LastQualityCheck,
                DefectRate = product.DefectRate,
                ReturnRate = product.ReturnRate,
                
                // Análisis y reportes
                ABCClassification = product.ABCClassification,
                VelocityCode = product.VelocityCode,
                ProfitMarginPercentage = product.ProfitMarginPercentage,
                LastSaleDate = product.LastSaleDate,
                TotalSalesQuantity = product.TotalSalesQuantity,
                
                // Información adicional
                InternalNotes = product.InternalNotes,
                CustomerNotes = product.CustomerNotes,
                MaintenanceInstructions = product.MaintenanceInstructions,
                SafetyWarnings = product.SafetyWarnings,
                
                // Configuración
                IsActive = product.IsActive,
                IsService = product.IsService,
                AllowFractionalQuantities = product.AllowFractionalQuantities,
                TrackSerial = product.TrackSerial,
                TrackExpiry = product.TrackExpiry,
                
                // Proveedores
                PrimarySupplierId = product.PrimarySupplierId,
                PrimarySupplierName = product.PrimarySupplier?.Name,
                SupplierCode = product.SupplierCode,
                
                // Metadata
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                CreatedByName = product.CreatedBy?.Name,
                UpdatedByName = product.UpdatedBy?.Name
            };

            // ✅ Obtener la imagen primaria del producto usando la navigation property
            var primaryImage = product.ProductImages?.FirstOrDefault(img => img.IsPrimary && img.IsActive);
            if (primaryImage != null)
            {
                response.ImageUrl = primaryImage.ImageUrl;
            }

            return response;
        }
    }
}
