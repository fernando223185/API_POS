-- ================================================================
-- AGREGAR CompanyId y BranchId a tabla Sales
-- Fecha: 2026-03-13
-- Descripción: Agrega campos desnormalizados para mejorar performance
-- ================================================================

USE [API_POS_DB];  -- CAMBIAR POR TU BASE DE DATOS SI ES DIFERENTE
GO

PRINT '?? Iniciando migración: Agregar CompanyId y BranchId a Sales';
GO

BEGIN TRANSACTION;

BEGIN TRY
    -- ========================================
    -- 1. VERIFICAR SI LAS COLUMNAS YA EXISTEN
    -- ========================================
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Sales') AND name = 'BranchId')
    BEGIN
        PRINT '? Agregando columna BranchId...';
        ALTER TABLE Sales ADD BranchId INT NULL;
    END
    ELSE
    BEGIN
        PRINT '??  Columna BranchId ya existe, saltando...';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Sales') AND name = 'CompanyId')
    BEGIN
        PRINT '? Agregando columna CompanyId...';
        ALTER TABLE Sales ADD CompanyId INT NULL;
    END
    ELSE
    BEGIN
        PRINT '??  Columna CompanyId ya existe, saltando...';
    END
    GO

    -- ========================================
    -- 2. POBLAR CON DATOS EXISTENTES
    -- ========================================
    PRINT '?? Poblando datos desde Warehouse ? Branch ? Company...';
    
    UPDATE s
    SET 
        s.BranchId = w.BranchId,
        s.CompanyId = b.CompanyId
    FROM Sales s
    INNER JOIN Warehouses w ON w.Id = s.WarehouseId
    INNER JOIN Branches b ON b.Id = w.BranchId
    WHERE s.BranchId IS NULL OR s.CompanyId IS NULL;
    GO

    -- ========================================
    -- 3. VERIFICAR DATOS POBLADOS
    -- ========================================
    DECLARE @TotalVentas INT;
    DECLARE @VentasConBranch INT;
    DECLARE @VentasConCompany INT;

    SELECT 
        @TotalVentas = COUNT(*),
        @VentasConBranch = COUNT(CASE WHEN BranchId IS NOT NULL THEN 1 END),
        @VentasConCompany = COUNT(CASE WHEN CompanyId IS NOT NULL THEN 1 END)
    FROM Sales;

    PRINT '?? Estadísticas:';
    PRINT '   Total de ventas: ' + CAST(@TotalVentas AS VARCHAR(10));
    PRINT '   Ventas con BranchId: ' + CAST(@VentasConBranch AS VARCHAR(10));
    PRINT '   Ventas con CompanyId: ' + CAST(@VentasConCompany AS VARCHAR(10));

    IF @TotalVentas > 0 AND (@VentasConBranch = 0 OR @VentasConCompany = 0)
    BEGIN
        PRINT '??  ADVERTENCIA: Hay ventas sin BranchId o CompanyId';
        PRINT '   Verifica que todas las ventas tengan un WarehouseId válido';
    END
    GO

    -- ========================================
    -- 4. CREAR ÍNDICES PARA PERFORMANCE
    -- ========================================
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Sales_BranchId' AND object_id = OBJECT_ID('Sales'))
    BEGIN
        PRINT '?? Creando índice IX_Sales_BranchId...';
        CREATE INDEX IX_Sales_BranchId ON Sales(BranchId);
    END
    ELSE
    BEGIN
        PRINT '??  Índice IX_Sales_BranchId ya existe';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Sales_CompanyId' AND object_id = OBJECT_ID('Sales'))
    BEGIN
        PRINT '?? Creando índice IX_Sales_CompanyId...';
        CREATE INDEX IX_Sales_CompanyId ON Sales(CompanyId);
    END
    ELSE
    BEGIN
        PRINT '??  Índice IX_Sales_CompanyId ya existe';
    END
    GO

    -- ========================================
    -- 5. AGREGAR FOREIGN KEYS
    -- ========================================
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Sales_Branches_BranchId')
    BEGIN
        PRINT '?? Creando Foreign Key FK_Sales_Branches_BranchId...';
        ALTER TABLE Sales
        ADD CONSTRAINT FK_Sales_Branches_BranchId 
        FOREIGN KEY (BranchId) REFERENCES Branches(Id);
    END
    ELSE
    BEGIN
        PRINT '??  Foreign Key FK_Sales_Branches_BranchId ya existe';
    END

    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Sales_Companies_CompanyId')
    BEGIN
        PRINT '?? Creando Foreign Key FK_Sales_Companies_CompanyId...';
        ALTER TABLE Sales
        ADD CONSTRAINT FK_Sales_Companies_CompanyId 
        FOREIGN KEY (CompanyId) REFERENCES Companies(Id);
    END
    ELSE
    BEGIN
        PRINT '??  Foreign Key FK_Sales_Companies_CompanyId ya existe';
    END
    GO

    -- ========================================
    -- 6. (OPCIONAL) HACER NOT NULL
    -- ========================================
    -- Descomentar si quieres hacer los campos NOT NULL después de validar
    -- que todos los registros tienen datos
    
    /*
    PRINT '?? Haciendo columnas NOT NULL...';
    
    -- Verificar que no hay NULLs
    IF NOT EXISTS (SELECT 1 FROM Sales WHERE BranchId IS NULL)
    BEGIN
        ALTER TABLE Sales ALTER COLUMN BranchId INT NOT NULL;
        PRINT '? BranchId ahora es NOT NULL';
    END
    ELSE
    BEGIN
        PRINT '??  No se puede hacer BranchId NOT NULL porque hay valores NULL';
    END

    IF NOT EXISTS (SELECT 1 FROM Sales WHERE CompanyId IS NULL)
    BEGIN
        ALTER TABLE Sales ALTER COLUMN CompanyId INT NOT NULL;
        PRINT '? CompanyId ahora es NOT NULL';
    END
    ELSE
    BEGIN
        PRINT '??  No se puede hacer CompanyId NOT NULL porque hay valores NULL';
    END
    GO
    */

    COMMIT TRANSACTION;
    PRINT '? ¡Migración completada exitosamente!';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    PRINT '? ERROR en la migración:';
    PRINT '   Mensaje: ' + ERROR_MESSAGE();
    PRINT '   Línea: ' + CAST(ERROR_LINE() AS VARCHAR(10));
    PRINT '   Procedimiento: ' + ISNULL(ERROR_PROCEDURE(), 'N/A');
    
    -- Re-lanzar el error
    THROW;
END CATCH
GO

-- ========================================
-- 7. VERIFICACIÓN FINAL
-- ========================================
PRINT '';
PRINT '?? REPORTE FINAL:';
PRINT '================';

SELECT 
    'Ventas por Empresa' as Tipo,
    c.LegalName as Empresa,
    COUNT(s.Id) as TotalVentas,
    SUM(s.Total) as ImporteTotal
FROM Sales s
LEFT JOIN Companies c ON c.Id = s.CompanyId
GROUP BY c.LegalName
ORDER BY ImporteTotal DESC;
GO

SELECT 
    'Ventas por Sucursal' as Tipo,
    b.Name as Sucursal,
    c.LegalName as Empresa,
    COUNT(s.Id) as TotalVentas,
    SUM(s.Total) as ImporteTotal
FROM Sales s
LEFT JOIN Branches b ON b.Id = s.BranchId
LEFT JOIN Companies c ON c.Id = s.CompanyId
GROUP BY b.Name, c.LegalName
ORDER BY ImporteTotal DESC;
GO

PRINT '';
PRINT '? Script completado exitosamente';
PRINT '?? Fecha: ' + CONVERT(VARCHAR(23), GETDATE(), 121);
GO
