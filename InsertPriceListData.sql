-- =============================================
-- Script: Insertar Datos Iniciales para PriceList
-- Descripción: Solo insertar las listas de precios básicas
-- =============================================

USE [ERP]
GO

-- Insertar listas de precios básicas
IF NOT EXISTS (SELECT 1 FROM [PriceLists] WHERE [Code] = 'MENUDEO')
BEGIN
    INSERT INTO [PriceLists] ([Name], [Description], [Code], [DefaultDiscountPercentage], [IsDefault], [IsActive], [CreatedAt])
    VALUES 
        ('Precio Menudeo', 'Lista de precios para venta al menudeo', 'MENUDEO', 0.0000, 1, 1, GETUTCDATE()),
        ('Precio Mayoreo', 'Lista de precios para venta al mayoreo con descuento', 'MAYOREO', 10.0000, 0, 1, GETUTCDATE()),
        ('Precio VIP', 'Lista de precios especiales para clientes VIP', 'VIP', 15.0000, 0, 1, GETUTCDATE()),
        ('Precio Distribuidor', 'Lista de precios para distribuidores', 'DISTRIBUIDOR', 20.0000, 0, 1, GETUTCDATE());
    
    PRINT '? Listas de precios insertadas exitosamente';
    
    -- Mostrar las listas insertadas
    SELECT [Id], [Name], [Code], [DefaultDiscountPercentage], [IsDefault], [IsActive]
    FROM [PriceLists]
    ORDER BY [Id];
END
ELSE
BEGIN
    PRINT '?? Las listas de precios ya existen';
    
    -- Mostrar las listas existentes
    SELECT [Id], [Name], [Code], [DefaultDiscountPercentage], [IsDefault], [IsActive]
    FROM [PriceLists]
    ORDER BY [Id];
END

GO