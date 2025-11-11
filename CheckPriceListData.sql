-- =============================================
-- Script: Verificar datos en tabla PriceLists
-- =============================================

USE [ERP]
GO

-- Ver todas las listas de precios disponibles
SELECT 
    [Id],
    [Name],
    [Code], 
    [DefaultDiscountPercentage],
    [IsDefault],
    [IsActive],
    [CreatedAt]
FROM [PriceLists] 
ORDER BY [Id];

-- Ver si existe la Foreign Key
SELECT 
    FK.name AS FK_Name,
    FK_Col.name AS FK_Column,
    PK_Tab.name AS PK_Table,
    PK_Col.name AS PK_Column
FROM sys.foreign_keys FK
    INNER JOIN sys.foreign_key_columns FK_Cols ON FK_Cols.constraint_object_id = FK.object_id
    INNER JOIN sys.columns FK_Col ON FK_Col.object_id = FK_Cols.parent_object_id AND FK_Col.column_id = FK_Cols.parent_column_id
    INNER JOIN sys.columns PK_Col ON PK_Col.object_id = FK_Cols.referenced_object_id AND PK_Col.column_id = FK_Cols.referenced_column_id
    INNER JOIN sys.tables PK_Tab ON PK_Tab.object_id = FK_Cols.referenced_object_id
WHERE FK.name = 'FK_Customer_PriceLists_PriceListId';

GO