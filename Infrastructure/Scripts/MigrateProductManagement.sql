-- Script SQL para migrar estructura de productos completa
-- Ejecutar este script para actualizar la base de datos

-- 1. Crear tabla de categor�as de productos
CREATE TABLE ProductCategories (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(100) NOT NULL,
    Description nvarchar(500) NULL,
    Code nvarchar(50) NOT NULL,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE()
);

-- 2. Crear tabla de subcategor�as de productos
CREATE TABLE ProductSubcategories (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(100) NOT NULL,
    Description nvarchar(500) NULL,
    Code nvarchar(50) NOT NULL,
    CategoryId int NOT NULL,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (CategoryId) REFERENCES ProductCategories(Id)
);

-- 3. Crear tabla de listas de precios
CREATE TABLE PriceLists (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(100) NOT NULL,
    Description nvarchar(500) NULL,
    Code nvarchar(50) NOT NULL,
    DefaultDiscountPercentage decimal(5,4) NOT NULL DEFAULT 0,
    IsDefault bit NOT NULL DEFAULT 0,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    ValidFrom datetime2 NULL,
    ValidTo datetime2 NULL
);

-- 4. Crear tabla de proveedores
CREATE TABLE Suppliers (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(200) NOT NULL,
    Code nvarchar(50) NOT NULL,
    TaxId nvarchar(20) NULL,
    ContactPerson nvarchar(200) NULL,
    Email nvarchar(100) NULL,
    Phone nvarchar(20) NULL,
    Address nvarchar(500) NULL,
    City nvarchar(100) NULL,
    State nvarchar(50) NULL,
    ZipCode nvarchar(10) NULL,
    Country nvarchar(50) NULL DEFAULT 'M�xico',
    PaymentTermsDays int NOT NULL DEFAULT 30,
    CreditLimit decimal(18,2) NOT NULL DEFAULT 0,
    DefaultDiscountPercentage decimal(5,4) NOT NULL DEFAULT 0,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt datetime2 NULL
);

-- 5. Actualizar tabla de productos (agregar campos nuevos)
ALTER TABLE Products ADD Brand nvarchar(100) NULL;
ALTER TABLE Products ADD Model nvarchar(100) NULL;
ALTER TABLE Products ADD CategoryId int NULL;
ALTER TABLE Products ADD SubcategoryId int NULL;
ALTER TABLE Products ADD SatCode nvarchar(10) NULL DEFAULT '01010101';
ALTER TABLE Products ADD SatUnit nvarchar(10) NULL DEFAULT 'PZA';
ALTER TABLE Products ADD BaseCost decimal(18,4) NOT NULL DEFAULT 0;
ALTER TABLE Products ADD TaxRate decimal(5,4) NOT NULL DEFAULT 0.16;
ALTER TABLE Products ADD MinimumStock decimal(18,4) NOT NULL DEFAULT 0;
ALTER TABLE Products ADD MaximumStock decimal(18,4) NOT NULL DEFAULT 0;
ALTER TABLE Products ADD ReorderPoint decimal(18,4) NOT NULL DEFAULT 0;
ALTER TABLE Products ADD Unit nvarchar(20) NOT NULL DEFAULT 'PZA';
ALTER TABLE Products ADD Weight decimal(10,4) NULL;
ALTER TABLE Products ADD Length decimal(10,4) NULL;
ALTER TABLE Products ADD Width decimal(10,4) NULL;
ALTER TABLE Products ADD Height decimal(10,4) NULL;
ALTER TABLE Products ADD Color nvarchar(50) NULL;
ALTER TABLE Products ADD Size nvarchar(50) NULL;
ALTER TABLE Products ADD IsActive bit NOT NULL DEFAULT 1;
ALTER TABLE Products ADD IsService bit NOT NULL DEFAULT 0;
ALTER TABLE Products ADD AllowFractionalQuantities bit NOT NULL DEFAULT 0;
ALTER TABLE Products ADD TrackSerial bit NOT NULL DEFAULT 0;
ALTER TABLE Products ADD TrackExpiry bit NOT NULL DEFAULT 0;
ALTER TABLE Products ADD PrimarySupplierId int NULL;
ALTER TABLE Products ADD SupplierCode nvarchar(100) NULL;
ALTER TABLE Products ADD CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE();
ALTER TABLE Products ADD UpdatedAt datetime2 NULL;
ALTER TABLE Products ADD CreatedByUserId int NULL;
ALTER TABLE Products ADD UpdatedByUserId int NULL;

-- 6. Agregar foreign keys a productos
ALTER TABLE Products ADD FOREIGN KEY (CategoryId) REFERENCES ProductCategories(Id);
ALTER TABLE Products ADD FOREIGN KEY (SubcategoryId) REFERENCES ProductSubcategories(Id);
ALTER TABLE Products ADD FOREIGN KEY (PrimarySupplierId) REFERENCES Suppliers(Id);
ALTER TABLE Products ADD FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id);
ALTER TABLE Products ADD FOREIGN KEY (UpdatedByUserId) REFERENCES Users(Id);

-- 7. Crear tabla de precios de productos
CREATE TABLE ProductPrices (
    Id int IDENTITY(1,1) PRIMARY KEY,
    ProductId int NOT NULL,
    PriceListId int NOT NULL,
    Price decimal(18,4) NOT NULL,
    DiscountPercentage decimal(5,4) NOT NULL DEFAULT 0,
    ValidFrom datetime2 NOT NULL DEFAULT GETUTCDATE(),
    ValidTo datetime2 NULL,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedByUserId int NOT NULL,
    FOREIGN KEY (ProductId) REFERENCES Products(ID),
    FOREIGN KEY (PriceListId) REFERENCES PriceLists(Id),
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
);

-- 8. Crear tabla de relaci�n productos-proveedores
CREATE TABLE ProductSuppliers (
    Id int IDENTITY(1,1) PRIMARY KEY,
    ProductId int NOT NULL,
    SupplierId int NOT NULL,
    SupplierProductCode nvarchar(100) NULL,
    Cost decimal(18,4) NOT NULL DEFAULT 0,
    MinimumOrderQuantity decimal(18,4) NOT NULL DEFAULT 1,
    LeadTimeDays int NOT NULL DEFAULT 7,
    IsPreferred bit NOT NULL DEFAULT 0,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (ProductId) REFERENCES Products(ID),
    FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id)
);

-- 9. Crear tabla de im�genes de productos
CREATE TABLE ProductImages (
    Id int IDENTITY(1,1) PRIMARY KEY,
    ProductId int NOT NULL,
    ImageUrl nvarchar(500) NOT NULL,
    ImageName nvarchar(200) NULL,
    AltText nvarchar(100) NULL,
    IsPrimary bit NOT NULL DEFAULT 0,
    DisplayOrder int NOT NULL DEFAULT 0,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UploadedByUserId int NOT NULL,
    FOREIGN KEY (ProductId) REFERENCES Products(ID),
    FOREIGN KEY (UploadedByUserId) REFERENCES Users(Id)
);

-- 10. Actualizar tabla de clientes para listas de precios
ALTER TABLE Customer ADD PriceListId int NULL;
ALTER TABLE Customer ADD TaxId nvarchar(20) NULL;
ALTER TABLE Customer ADD CompanyName nvarchar(200) NULL;
ALTER TABLE Customer ADD SatTaxRegime nvarchar(10) NULL;
ALTER TABLE Customer ADD SatCfdiUse nvarchar(10) NULL DEFAULT 'G03';
ALTER TABLE Customer ADD DiscountPercentage decimal(5,4) NOT NULL DEFAULT 0;
ALTER TABLE Customer ADD CreditLimit decimal(18,2) NOT NULL DEFAULT 0;
ALTER TABLE Customer ADD PaymentTermsDays int NOT NULL DEFAULT 0;
ALTER TABLE Customer ADD IsActive bit NOT NULL DEFAULT 1;
ALTER TABLE Customer ADD UpdatedAt datetime2 NULL;

-- Agregar foreign key
ALTER TABLE Customer ADD FOREIGN KEY (PriceListId) REFERENCES PriceLists(Id);

-- 11. Insertar datos b�sicos

-- Categor�as por defecto
INSERT INTO ProductCategories (Name, Code, Description) VALUES 
('General', 'GEN', 'Categor�a general para productos'),
('Electr�nicos', 'ELEC', 'Productos electr�nicos'),
('Ropa', 'ROPA', 'Prendas de vestir'),
('Hogar', 'HOGAR', 'Artículos para el hogar'),
('Alimentación', 'ALIM', 'Productos alimenticios');

-- Listas de precios por defecto
INSERT INTO PriceLists (Name, Code, Description, IsDefault) VALUES 
('Público General', 'PUB', 'Precios para público en general', 1),
('Mayoreo', 'MAY', 'Precios para ventas al mayoreo', 0),
('VIP', 'VIP', 'Precios especiales para clientes VIP', 0),
('Distribuidor', 'DIST', 'Precios para distribuidores', 0);

-- Proveedor ejemplo
INSERT INTO Suppliers (Name, Code, ContactPerson, Email, Phone) VALUES 
('Proveedor General', 'PROV001', 'Juan P�rez', 'contacto@proveedor.com', '555-1234');

PRINT 'Migraci�n de productos completada exitosamente';