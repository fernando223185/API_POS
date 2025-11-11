-- =============================================
-- Script: Crear Sistema Completo de Product Management ERP
-- Descripción: PriceLists, ProductCategories, Suppliers y ProductPrices
-- =============================================

USE [ERP]
GO

-- =============================================
-- 1. CREAR TABLA PRICELISTS (Listas de Precios)
-- =============================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PriceLists')
BEGIN
    CREATE TABLE [dbo].[PriceLists](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](100) NOT NULL,
        [Description] [nvarchar](500) NULL,
        [Code] [nvarchar](50) NOT NULL,
        [DefaultDiscountPercentage] [decimal](6,4) NOT NULL DEFAULT 0,
        [IsDefault] [bit] NOT NULL DEFAULT 0,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        [ValidFrom] [datetime2](7) NULL,
        [ValidTo] [datetime2](7) NULL,
        CONSTRAINT [PK_PriceLists] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UK_PriceLists_Code] UNIQUE ([Code])
    );
    
    CREATE NONCLUSTERED INDEX [IX_PriceLists_IsActive] ON [dbo].[PriceLists] ([IsActive]);
    CREATE NONCLUSTERED INDEX [IX_PriceLists_IsDefault] ON [dbo].[PriceLists] ([IsDefault]);
    
    PRINT '? Tabla PriceLists creada exitosamente';
END
ELSE
    PRINT '?? Tabla PriceLists ya existe';
GO

-- =============================================
-- 2. CREAR TABLA PRODUCTCATEGORIES (Categorías)
-- =============================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ProductCategories')
BEGIN
    CREATE TABLE [dbo].[ProductCategories](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](100) NOT NULL,
        [Description] [nvarchar](500) NULL,
        [Code] [nvarchar](20) NOT NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [DisplayOrder] [int] NOT NULL DEFAULT 0,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_ProductCategories] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UK_ProductCategories_Code] UNIQUE ([Code])
    );
  
    CREATE NONCLUSTERED INDEX [IX_ProductCategories_IsActive] ON [dbo].[ProductCategories] ([IsActive]);
    
    PRINT '? Tabla ProductCategories creada exitosamente';
END
ELSE
    PRINT '?? Tabla ProductCategories ya existe';
GO

-- =============================================
-- 3. CREAR TABLA PRODUCTSUBCATEGORIES (Subcategorías)
-- =============================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ProductSubcategories')
BEGIN
    CREATE TABLE [dbo].[ProductSubcategories](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](100) NOT NULL,
        [Description] [nvarchar](500) NULL,
        [Code] [nvarchar](20) NOT NULL,
        [CategoryId] [int] NOT NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [DisplayOrder] [int] NOT NULL DEFAULT 0,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_ProductSubcategories] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UK_ProductSubcategories_Code] UNIQUE ([Code]),
        CONSTRAINT [FK_ProductSubcategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [ProductCategories]([Id])
    );
    
    CREATE NONCLUSTERED INDEX [IX_ProductSubcategories_CategoryId] ON [dbo].[ProductSubcategories] ([CategoryId]);
    CREATE NONCLUSTERED INDEX [IX_ProductSubcategories_IsActive] ON [dbo].[ProductSubcategories] ([IsActive]);
    
    PRINT '? Tabla ProductSubcategories creada exitosamente';
END
ELSE
    PRINT '?? Tabla ProductSubcategories ya existe';
GO

-- =============================================
-- 4. CREAR TABLA SUPPLIERS (Proveedores)
-- =============================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Suppliers')
BEGIN
    CREATE TABLE [dbo].[Suppliers](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Code] [nvarchar](50) NOT NULL,
        [Name] [nvarchar](200) NOT NULL,
        [ContactPerson] [nvarchar](200) NULL,
        [Email] [nvarchar](100) NULL,
        [Phone] [nvarchar](20) NULL,
        [Address] [nvarchar](500) NULL,
        [City] [nvarchar](100) NULL,
        [State] [nvarchar](50) NULL,
        [Country] [nvarchar](50) NULL,
        [ZipCode] [nvarchar](10) NULL,
        [TaxId] [nvarchar](20) NULL,
        [CreditLimit] [decimal](18,2) NOT NULL DEFAULT 0,
        [PaymentTermsDays] [int] NOT NULL DEFAULT 0,
        [DefaultDiscountPercentage] [decimal](6,4) NOT NULL DEFAULT 0,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] [datetime2](7) NULL,
        CONSTRAINT [PK_Suppliers] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UK_Suppliers_Code] UNIQUE ([Code])
    );
    
    CREATE NONCLUSTERED INDEX [IX_Suppliers_IsActive] ON [dbo].[Suppliers] ([IsActive]);
    CREATE NONCLUSTERED INDEX [IX_Suppliers_TaxId] ON [dbo].[Suppliers] ([TaxId]);
    
    PRINT '? Tabla Suppliers creada exitosamente';
END
ELSE
    PRINT '?? Tabla Suppliers ya existe';
GO

-- =============================================
-- 5. CREAR TABLA PRODUCTPRICES (Precios por Lista)
-- =============================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ProductPrices')
BEGIN
    CREATE TABLE [dbo].[ProductPrices](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ProductId] [int] NOT NULL,
        [PriceListId] [int] NOT NULL,
        [Price] [decimal](18,4) NOT NULL,
        [DiscountPercentage] [decimal](6,4) NOT NULL DEFAULT 0,
        [ValidFrom] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        [ValidTo] [datetime2](7) NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        [CreatedByUserId] [int] NOT NULL,
        CONSTRAINT [PK_ProductPrices] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_ProductPrices_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products]([ID]),
        CONSTRAINT [FK_ProductPrices_PriceListId] FOREIGN KEY ([PriceListId]) REFERENCES [PriceLists]([Id]),
        CONSTRAINT [FK_ProductPrices_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users]([Id])
    );
    
    CREATE NONCLUSTERED INDEX [IX_ProductPrices_ProductId] ON [dbo].[ProductPrices] ([ProductId]);
    CREATE NONCLUSTERED INDEX [IX_ProductPrices_PriceListId] ON [dbo].[ProductPrices] ([PriceListId]);
    CREATE NONCLUSTERED INDEX [IX_ProductPrices_IsActive] ON [dbo].[ProductPrices] ([IsActive]);
    
    PRINT '? Tabla ProductPrices creada exitosamente';
END
ELSE  
    PRINT '?? Tabla ProductPrices ya existe';
GO

-- =============================================
-- 6. CREAR TABLA PRODUCTSUPPLIERS (Relación Producto-Proveedor)
-- =============================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ProductSuppliers')
BEGIN
    CREATE TABLE [dbo].[ProductSuppliers](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ProductId] [int] NOT NULL,
        [SupplierId] [int] NOT NULL,
        [SupplierProductCode] [nvarchar](100) NULL,
        [Cost] [decimal](18,4) NOT NULL DEFAULT 0,
        [LeadTimeDays] [int] NOT NULL DEFAULT 0,
        [MinOrderQuantity] [decimal](18,4) NOT NULL DEFAULT 1,
        [IsPrimary] [bit] NOT NULL DEFAULT 0,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_ProductSuppliers] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_ProductSuppliers_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products]([ID]),
        CONSTRAINT [FK_ProductSuppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers]([Id])
    );
    
    CREATE NONCLUSTERED INDEX [IX_ProductSuppliers_ProductId] ON [dbo].[ProductSuppliers] ([ProductId]);
    CREATE NONCLUSTERED INDEX [IX_ProductSuppliers_SupplierId] ON [dbo].[ProductSuppliers] ([SupplierId]);
    CREATE NONCLUSTERED INDEX [IX_ProductSuppliers_IsPrimary] ON [dbo].[ProductSuppliers] ([IsPrimary]);
    
    PRINT '? Tabla ProductSuppliers creada exitosamente';
END
ELSE
    PRINT '?? Tabla ProductSuppliers ya existe';
GO

-- =============================================
-- 7. CREAR TABLA PRODUCTIMAGES (Imágenes de Productos)
-- =============================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ProductImages')
BEGIN
    CREATE TABLE [dbo].[ProductImages](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ProductId] [int] NOT NULL,
        [ImageUrl] [nvarchar](500) NOT NULL,
        [ImagePath] [nvarchar](500) NULL,
        [AltText] [nvarchar](200) NULL,
        [IsPrimary] [bit] NOT NULL DEFAULT 0,
        [DisplayOrder] [int] NOT NULL DEFAULT 0,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_ProductImages] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_ProductImages_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products]([ID])
    );
    
    CREATE NONCLUSTERED INDEX [IX_ProductImages_ProductId] ON [dbo].[ProductImages] ([ProductId]);
    CREATE NONCLUSTERED INDEX [IX_ProductImages_IsPrimary] ON [dbo].[ProductImages] ([IsPrimary]);
    
    PRINT '? Tabla ProductImages creada exitosamente';
END
ELSE
    PRINT '?? Tabla ProductImages ya existe';
GO

-- =============================================
-- 8. INSERTAR DATOS INICIALES
-- =============================================

-- Listas de Precios por defecto
IF NOT EXISTS (SELECT 1 FROM [PriceLists] WHERE [Code] = 'MENUDEO')
BEGIN
    INSERT INTO [PriceLists] ([Name], [Description], [Code], [DefaultDiscountPercentage], [IsDefault], [IsActive])
    VALUES 
        ('Precio Menudeo', 'Lista de precios para venta al menudeo', 'MENUDEO', 0.0000, 1, 1),
        ('Precio Mayoreo', 'Lista de precios para venta al mayoreo con descuento', 'MAYOREO', 10.0000, 0, 1),
        ('Precio VIP', 'Lista de precios especiales para clientes VIP', 'VIP', 15.0000, 0, 1),
        ('Precio Distribuidor', 'Lista de precios para distribuidores', 'DISTRIBUIDOR', 20.0000, 0, 1);
    
    PRINT '? Listas de precios insertadas exitosamente';
END
ELSE
    PRINT '?? Listas de precios ya existen';

-- Categorías de Productos por defecto
IF NOT EXISTS (SELECT 1 FROM [ProductCategories] WHERE [Code] = 'GENERAL')
BEGIN
    INSERT INTO [ProductCategories] ([Name], [Description], [Code], [DisplayOrder])
    VALUES 
        ('General', 'Productos generales', 'GENERAL', 1),
        ('Electrónicos', 'Productos electrónicos y tecnología', 'ELECTRONICOS', 2),
        ('Ropa y Accesorios', 'Vestimenta y accesorios', 'ROPA', 3),
        ('Hogar y Jardín', 'Productos para el hogar y jardín', 'HOGAR', 4),
        ('Alimentación', 'Productos alimenticios y bebidas', 'ALIMENTOS', 5),
        ('Salud y Belleza', 'Productos de salud y cuidado personal', 'SALUD', 6);
    
    PRINT '? Categorías de productos insertadas exitosamente';
END
ELSE
    PRINT '?? Categorías de productos ya existen';

-- Subcategorías por defecto
IF NOT EXISTS (SELECT 1 FROM [ProductSubcategories] WHERE [Code] = 'MISC')
BEGIN
    DECLARE @GeneralCategoryId INT = (SELECT TOP 1 [Id] FROM [ProductCategories] WHERE [Code] = 'GENERAL');
    DECLARE @ElectronicosCategoryId INT = (SELECT TOP 1 [Id] FROM [ProductCategories] WHERE [Code] = 'ELECTRONICOS');
    
    INSERT INTO [ProductSubcategories] ([Name], [Description], [Code], [CategoryId], [DisplayOrder])
    VALUES 
        ('Varios', 'Productos varios sin categoría específica', 'MISC', @GeneralCategoryId, 1),
        ('Computadoras', 'Laptops, desktops y accesorios', 'COMPUTADORAS', @ElectronicosCategoryId, 1),
        ('Teléfonos', 'Smartphones y accesorios', 'TELEFONOS', @ElectronicosCategoryId, 2);
        
    PRINT '? Subcategorías insertadas exitosamente';
END
ELSE
    PRINT '?? Subcategorías ya existen';

-- Proveedor por defecto
IF NOT EXISTS (SELECT 1 FROM [Suppliers] WHERE [Code] = 'PROV001')
BEGIN
    INSERT INTO [Suppliers] ([Code], [Name], [ContactPerson], [Email], [Phone], [Address], [CreditLimit], [PaymentTermsDays])
    VALUES 
        ('PROV001', 'Proveedor General S.A.', 'Juan Pérez', 'contacto@proveedor.com', '555-1234567', 'Av. Proveedores 123', 100000.00, 30),
        ('PROV002', 'Electrónicos del Norte', 'María González', 'ventas@electronicos.com', '555-7654321', 'Calle Tecnología 456', 250000.00, 15);
    
    PRINT '? Proveedores insertados exitosamente';
END
ELSE
    PRINT '?? Proveedores ya existen';

PRINT '?? Sistema de Product Management ERP creado exitosamente';
PRINT '?? Tablas creadas: PriceLists, ProductCategories, ProductSubcategories, Suppliers, ProductPrices, ProductSuppliers, ProductImages';
PRINT '?? Datos iniciales insertados: 4 listas de precios, 6 categorías, 3 subcategorías, 2 proveedores';

GO