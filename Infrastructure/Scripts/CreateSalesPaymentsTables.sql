-- ========================================
-- SCRIPT: Crear tablas del sistema de ventas con cobranza
-- Fecha: 2026-03-11
-- Descripción: Crea las tablas Sales, SaleDetails, SalePayments
--              y actualiza InventoryMovements para soportar ventas
-- ========================================

BEGIN TRANSACTION;

PRINT '?? Iniciando creación de tablas del sistema de ventas...';

-- ========================================
-- TABLA: Sales (Ventas)
-- ========================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Sales' AND TABLE_SCHEMA = 'dbo')
BEGIN
    PRINT '?? Creando tabla Sales...';
    
    CREATE TABLE [dbo].[Sales] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Code] NVARCHAR(20) NOT NULL,
        [SaleDate] DATETIME2 NOT NULL,
        
        -- Cliente
        [CustomerId] INT NULL,
        [CustomerName] NVARCHAR(200) NULL,
        
        -- Ubicación y usuario
        [WarehouseId] INT NOT NULL,
        [UserId] INT NOT NULL,
        [PriceListId] INT NULL,
        
        -- Montos
        [SubTotal] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [DiscountAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [DiscountPercentage] DECIMAL(6,4) NOT NULL DEFAULT 0,
        [TaxAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Total] DECIMAL(18,2) NOT NULL DEFAULT 0,
        
        -- Pago
        [AmountPaid] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [ChangeAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [IsPaid] BIT NOT NULL DEFAULT 0,
        
        -- Estado
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Draft',
        [IsPostedToInventory] BIT NOT NULL DEFAULT 0,
        [PostedToInventoryDate] DATETIME2 NULL,
        
        -- Facturación
        [RequiresInvoice] BIT NOT NULL DEFAULT 0,
        [InvoiceId] INT NULL,
        [InvoiceUuid] NVARCHAR(50) NULL,
        
        -- Metadatos
        [Notes] NVARCHAR(1000) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT CURRENT_TIMESTAMP,
        [CreatedByUserId] INT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [CancelledAt] DATETIME2 NULL,
        [CancelledByUserId] INT NULL,
        [CancellationReason] NVARCHAR(500) NULL,
        
        CONSTRAINT [PK_Sales] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Sales_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [Customer]([ID]) ON DELETE SET NULL,
        CONSTRAINT [FK_Sales_Warehouse] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses]([Id]),
        CONSTRAINT [FK_Sales_User] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]),
        CONSTRAINT [FK_Sales_PriceList] FOREIGN KEY ([PriceListId]) REFERENCES [PriceLists]([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Sales_CreatedBy] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users]([Id])
    );
    
    -- Índices
    CREATE UNIQUE INDEX [IX_Sales_Code] ON [Sales]([Code]);
    CREATE INDEX [IX_Sales_SaleDate] ON [Sales]([SaleDate]);
    CREATE INDEX [IX_Sales_CustomerId] ON [Sales]([CustomerId]);
    CREATE INDEX [IX_Sales_WarehouseId] ON [Sales]([WarehouseId]);
    CREATE INDEX [IX_Sales_UserId] ON [Sales]([UserId]);
    CREATE INDEX [IX_Sales_Status] ON [Sales]([Status]);
    CREATE INDEX [IX_Sales_IsPostedToInventory] ON [Sales]([IsPostedToInventory]);
    
    PRINT '? Tabla Sales creada exitosamente';
END
ELSE
BEGIN
    PRINT '??  Tabla Sales ya existe, omitiendo...';
END

-- ========================================
-- TABLA: SaleDetails (Detalles de venta)
-- ========================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SaleDetails' AND TABLE_SCHEMA = 'dbo')
BEGIN
    PRINT '?? Creando tabla SaleDetails...';
    
    CREATE TABLE [dbo].[SaleDetails] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [SaleId] INT NOT NULL,
        [ProductId] INT NOT NULL,
        
        -- Información denormalizada
        [ProductCode] NVARCHAR(50) NOT NULL,
        [ProductName] NVARCHAR(200) NOT NULL,
        
        -- Cantidades y precios
        [Quantity] DECIMAL(18,4) NOT NULL,
        [UnitPrice] DECIMAL(18,4) NOT NULL,
        [DiscountPercentage] DECIMAL(6,4) NOT NULL DEFAULT 0,
        [DiscountAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [TaxPercentage] DECIMAL(6,4) NOT NULL DEFAULT 0,
        [TaxAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        
        -- Totales
        [SubTotal] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Total] DECIMAL(18,2) NOT NULL DEFAULT 0,
        
        -- Costos
        [UnitCost] DECIMAL(18,4) NULL,
        [TotalCost] DECIMAL(18,2) NULL,
        
        -- Información adicional
        [Notes] NVARCHAR(500) NULL,
        [SerialNumber] NVARCHAR(100) NULL,
        [LotNumber] NVARCHAR(100) NULL,
        
        CONSTRAINT [PK_SaleDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SaleDetails_Sale] FOREIGN KEY ([SaleId]) REFERENCES [Sales]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SaleDetails_Product] FOREIGN KEY ([ProductId]) REFERENCES [Products]([ID])
    );
    
    -- Índices
    CREATE INDEX [IX_SaleDetails_SaleId] ON [SaleDetails]([SaleId]);
    CREATE INDEX [IX_SaleDetails_ProductId] ON [SaleDetails]([ProductId]);
    
    PRINT '? Tabla SaleDetails creada exitosamente';
END
ELSE
BEGIN
    PRINT '??  Tabla SaleDetails ya existe, omitiendo...';
END

-- ========================================
-- TABLA: SalePayments (Pagos de venta)
-- ========================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SalePayments' AND TABLE_SCHEMA = 'dbo')
BEGIN
    PRINT '?? Creando tabla SalePayments...';
    
    CREATE TABLE [dbo].[SalePayments] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [SaleId] INT NOT NULL,
        
        -- Información básica del pago
        [PaymentMethod] NVARCHAR(50) NOT NULL,
        [Amount] DECIMAL(18,2) NOT NULL,
        [PaymentDate] DATETIME2 NOT NULL DEFAULT CURRENT_TIMESTAMP,
        
        -- Datos de tarjeta/terminal
        [CardNumber] NVARCHAR(4) NULL,
        [CardType] NVARCHAR(20) NULL,
        [AuthorizationCode] NVARCHAR(50) NULL,
        [TransactionReference] NVARCHAR(100) NULL,
        [TerminalId] NVARCHAR(50) NULL,
        [BankName] NVARCHAR(100) NULL,
        
        -- Transferencias
        [TransferReference] NVARCHAR(100) NULL,
        
        -- Cheques
        [CheckNumber] NVARCHAR(50) NULL,
        [CheckBank] NVARCHAR(100) NULL,
        
        -- Estado
        [Status] NVARCHAR(20) NOT NULL DEFAULT 'Completed',
        [Notes] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT CURRENT_TIMESTAMP,
        
        CONSTRAINT [PK_SalePayments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SalePayments_Sale] FOREIGN KEY ([SaleId]) REFERENCES [Sales]([Id]) ON DELETE CASCADE
    );
    
    -- Índices
    CREATE INDEX [IX_SalePayments_SaleId] ON [SalePayments]([SaleId]);
    CREATE INDEX [IX_SalePayments_PaymentMethod] ON [SalePayments]([PaymentMethod]);
    CREATE INDEX [IX_SalePayments_PaymentDate] ON [SalePayments]([PaymentDate]);
    CREATE INDEX [IX_SalePayments_Status] ON [SalePayments]([Status]);
    
    PRINT '? Tabla SalePayments creada exitosamente';
END
ELSE
BEGIN
    PRINT '??  Tabla SalePayments ya existe, omitiendo...';
END

-- ========================================
-- ACTUALIZAR TABLA: InventoryMovements
-- Agregar campo SaleId para vincular con ventas
-- ========================================
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'InventoryMovements' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'InventoryMovements' 
                   AND COLUMN_NAME = 'SaleId')
    BEGIN
        PRINT '?? Agregando campo SaleId a InventoryMovements...';
        
        ALTER TABLE [InventoryMovements]
        ADD [SaleId] INT NULL;
        
        ALTER TABLE [InventoryMovements]
        ADD CONSTRAINT [FK_InventoryMovements_Sale] 
        FOREIGN KEY ([SaleId]) REFERENCES [Sales]([Id]) ON DELETE SET NULL;
        
        CREATE INDEX [IX_InventoryMovements_SaleId] ON [InventoryMovements]([SaleId]);
        
        PRINT '? Campo SaleId agregado a InventoryMovements';
    END
    ELSE
    BEGIN
        PRINT '??  Campo SaleId ya existe en InventoryMovements, omitiendo...';
    END
END
ELSE
BEGIN
    PRINT '? ERROR: Tabla InventoryMovements no existe';
    ROLLBACK TRANSACTION;
    RETURN;
END

-- ========================================
-- COMMIT
-- ========================================
COMMIT TRANSACTION;

PRINT '';
PRINT '?? ====================================';
PRINT '? Sistema de ventas creado exitosamente';
PRINT '====================================';
PRINT '';
PRINT '?? Resumen:';
PRINT '   ? Tabla Sales creada';
PRINT '   ? Tabla SaleDetails creada';
PRINT '   ? Tabla SalePayments creada';
PRINT '   ? Campo SaleId agregado a InventoryMovements';
PRINT '';
PRINT '?? El sistema de ventas está listo para usarse';
PRINT '';
