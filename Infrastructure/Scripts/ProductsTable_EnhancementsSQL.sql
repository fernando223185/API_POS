-- =============================================
-- CAMPOS ADICIONALES RECOMENDADOS PARA PRODUCTOS ERP
-- Basado en tu estructura existente
-- =============================================

-- 1. INFORMACIÓN COMERCIAL AVANZADA
ALTER TABLE Products ADD Warranty INT NULL; -- Meses de garantía
ALTER TABLE Products ADD WarrantyType NVARCHAR(50) NULL; -- "Fabricante", "Tienda", "Extendida"
ALTER TABLE Products ADD Location NVARCHAR(100) NULL; -- Ubicación física en almacén
ALTER TABLE Products ADD Aisle NVARCHAR(20) NULL; -- Pasillo
ALTER TABLE Products ADD Shelf NVARCHAR(20) NULL; -- Estante
ALTER TABLE Products ADD Bin NVARCHAR(20) NULL; -- Contenedor

-- 2. CLASIFICACIÓN AVANZADA
ALTER TABLE Products ADD Tags NVARCHAR(500) NULL; -- Tags separados por comas "oferta,nuevo,temporada"
ALTER TABLE Products ADD Season NVARCHAR(50) NULL; -- "Primavera", "Verano", "Otoño", "Invierno", "Todo el año"
ALTER TABLE Products ADD TargetGender NVARCHAR(20) NULL; -- "Masculino", "Femenino", "Unisex", "Infantil"
ALTER TABLE Products ADD AgeGroup NVARCHAR(50) NULL; -- "Adulto", "Infantil", "Juvenil", "Senior"

-- 3. INFORMACIÓN DE VENTAS
ALTER TABLE Products ADD MaxQuantityPerSale DECIMAL(18,4) NULL; -- Cantidad máxima por venta
ALTER TABLE Products ADD  MinQuantityPerSale DECIMAL(18,4) NULL DEFAULT 1; -- Cantidad mínima por venta
ALTER TABLE Products ADD  SalesNotes NVARCHAR(500) NULL; -- Notas especiales de venta
ALTER TABLE Products ADD  IsDiscountAllowed BIT NOT NULL DEFAULT 1; -- Permitir descuentos
ALTER TABLE Products ADD  MaxDiscountPercentage DECIMAL(5,2) NULL; -- Descuento máximo permitido

-- 4. INFORMACIÓN FISCAL AVANZADA
ALTER TABLE Products ADD  SatTaxType NVARCHAR(20) NULL DEFAULT 'Tasa'; -- "Tasa", "Cuota", "Exento"
ALTER TABLE Products ADD  CustomsCode NVARCHAR(20) NULL; -- Código aduanero para importación
ALTER TABLE Products ADD  CountryOfOrigin NVARCHAR(50) NULL DEFAULT 'México'; -- País de origen

-- 5. E-COMMERCE Y MARKETING
ALTER TABLE Products ADD  SEOTitle NVARCHAR(200) NULL; -- Título SEO
ALTER TABLE Products ADD  SEODescription NVARCHAR(500) NULL; -- Descripción SEO
ALTER TABLE Products ADD  SEOKeywords NVARCHAR(300) NULL; -- Palabras clave SEO
ALTER TABLE Products ADD  IsWebVisible BIT NOT NULL DEFAULT 1; -- Visible en web
ALTER TABLE Products ADD  IsFeatured BIT NOT NULL DEFAULT 0; -- Producto destacado
ALTER TABLE Products ADD  LaunchDate DATETIME2 NULL; -- Fecha de lanzamiento
ALTER TABLE Products ADD  DiscontinuedDate DATETIME2 NULL; -- Fecha de descontinuación

-- 6. INFORMACIÓN TÉCNICA
ALTER TABLE Products ADD  TechnicalSpecs NVARCHAR(2000) NULL; -- Especificaciones técnicas (JSON)
ALTER TABLE Products ADD  ManufacturerPartNumber NVARCHAR(100) NULL; -- Número de parte del fabricante
ALTER TABLE Products ADD  UPC NVARCHAR(50) NULL; -- Código UPC
ALTER TABLE Products ADD  EAN NVARCHAR(50) NULL; -- Código EAN
ALTER TABLE Products ADD  ISBN NVARCHAR(50) NULL; -- Para libros

-- 7. LOGÍSTICA Y ENVÍO
ALTER TABLE Products ADD  IsFragile BIT NOT NULL DEFAULT 0; -- Producto frágil
ALTER TABLE Products ADD  RequiresSpecialHandling BIT NOT NULL DEFAULT 0; -- Manejo especial
ALTER TABLE Products ADD  ShippingClass NVARCHAR(50) NULL; -- "Normal", "Pesado", "Frágil", "Peligroso"
ALTER TABLE Products ADD  PackageLength DECIMAL(10,4) NULL; -- Largo del paquete
ALTER TABLE Products ADD  PackageWidth DECIMAL(10,4) NULL; -- Ancho del paquete  
ALTER TABLE Products ADD  PackageHeight DECIMAL(10,4) NULL; -- Alto del paquete
ALTER TABLE Products ADD  PackageWeight DECIMAL(10,4) NULL; -- Peso del paquete

-- 8. CONTROL DE CALIDAD
ALTER TABLE Products ADD  QualityGrade NVARCHAR(20) NULL; -- "A", "B", "C", "Premium", "Standard"
ALTER TABLE Products ADD  LastQualityCheck DATETIME2 NULL; -- Última revisión de calidad
ALTER TABLE Products ADD  DefectRate DECIMAL(5,4) NULL; -- Tasa de defectos
ALTER TABLE Products ADD  ReturnRate DECIMAL(5,4) NULL; -- Tasa de devoluciones

-- 9. ANÁLISIS Y REPORTES
ALTER TABLE Products ADD  ABCClassification NVARCHAR(5) NULL; -- Clasificación ABC (A, B, C)
ALTER TABLE Products ADD  VelocityCode NVARCHAR(20) NULL; -- "Rápido", "Medio", "Lento"
ALTER TABLE Products ADD  ProfitMarginPercentage DECIMAL(5,2) NULL; -- Margen de ganancia
ALTER TABLE Products ADD  LastSaleDate DATETIME2 NULL; -- Última fecha de venta
ALTER TABLE Products ADD  TotalSalesQuantity DECIMAL(18,4) NULL DEFAULT 0; -- Total vendido histórico

-- 10. INFORMACIÓN ADICIONAL
ALTER TABLE Products ADD  InternalNotes NVARCHAR(1000) NULL; -- Notas internas
ALTER TABLE Products ADD  CustomerNotes NVARCHAR(500) NULL; -- Notas visibles al cliente
ALTER TABLE Products ADD  MaintenanceInstructions NVARCHAR(1000) NULL; -- Instrucciones de mantenimiento
ALTER TABLE Products ADD  SafetyWarnings NVARCHAR(500) NULL; -- Advertencias de seguridad

-- =============================================
-- ÍNDICES RECOMENDADOS PARA PERFORMANCE
-- =============================================

-- Índices para búsquedas frecuentes
CREATE INDEX IX_Products_Brand_IsActive ON Products(Brand, IsActive);
CREATE INDEX IX_Products_Category_Subcategory ON Products(CategoryId, SubcategoryId);
CREATE INDEX IX_Products_Tags ON Products(Tags);
CREATE INDEX IX_Products_LastSaleDate ON Products(LastSaleDate DESC);
CREATE INDEX IX_Products_ABCClassification ON Products(ABCClassification);
CREATE INDEX IX_Products_Location ON Products(Location, Aisle, Shelf);

-- Índices compuestos para reportes
CREATE INDEX IX_Products_Analysis ON Products(IsActive, ABCClassification, VelocityCode);
CREATE INDEX IX_Products_Inventory ON Products(IsActive, MinimumStock, MaximumStock);
CREATE INDEX IX_Products_Pricing ON Products(IsActive, price, BaseCost);

-- =============================================
-- COMENTARIOS PARA DOCUMENTACIÓN
-- =============================================

-- Agregar comentarios a campos importantes
EXEC sp_addextendedproperty 
@name = N'MS_Description', @value = 'Clasificación ABC para análisis de inventario', 
@level0type = N'Schema', @level0name = 'dbo', 
@level1type = N'Table', @level1name = 'Products', 
@level2type = N'Column', @level2name = 'ABCClassification';

EXEC sp_addextendedproperty 
@name = N'MS_Description', @value = 'Código de velocidad de rotación: Rápido, Medio, Lento', 
@level0type = N'Schema', @level0name = 'dbo', 
@level1type = N'Table', @level1name = 'Products', 
@level2type = N'Column', @level2name = 'VelocityCode';