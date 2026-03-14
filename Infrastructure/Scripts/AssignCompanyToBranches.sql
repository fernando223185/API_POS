-- ================================================
-- ACTUALIZAR SUCURSALES: ASIGNAR EMPRESA
-- ================================================
-- Este script asigna la empresa principal a todas las sucursales
-- que no tienen CompanyId asignado
--
-- Ejecutar ANTES de aplicar la nueva lógica que requiere CompanyId

USE [ERP]; -- ?? CAMBIA POR EL NOMBRE DE TU BASE DE DATOS
GO

PRINT '========================================';
PRINT '?? ASIGNANDO EMPRESA A SUCURSALES';
PRINT '========================================';
PRINT '';

-- ========================================
-- 1. VERIFICAR EMPRESA PRINCIPAL
-- ========================================
DECLARE @MainCompanyId INT;

SELECT @MainCompanyId = Id
FROM Companies
WHERE IsMainCompany = 1
  AND IsActive = 1;

IF @MainCompanyId IS NULL
BEGIN
    PRINT '??  No se encontró empresa principal activa';
    PRINT '?? Buscando la primera empresa activa...';
    
    SELECT TOP 1 @MainCompanyId = Id
    FROM Companies
    WHERE IsActive = 1
    ORDER BY Id;
    
    IF @MainCompanyId IS NULL
    BEGIN
        PRINT '? ERROR: No hay empresas activas en el sistema';
        PRINT '??  Crea primero una empresa antes de asignar sucursales';
        RETURN;
    END
END

PRINT '? Empresa encontrada (ID: ' + CAST(@MainCompanyId AS VARCHAR) + ')';

DECLARE @CompanyName NVARCHAR(300);
SELECT @CompanyName = LegalName FROM Companies WHERE Id = @MainCompanyId;
PRINT '   Nombre: ' + @CompanyName;
PRINT '';

-- ========================================
-- 2. VERIFICAR SUCURSALES SIN EMPRESA
-- ========================================
DECLARE @BranchesWithoutCompany INT;

SELECT @BranchesWithoutCompany = COUNT(*)
FROM Branches
WHERE CompanyId IS NULL;

PRINT '?? ESTADO ACTUAL:';
PRINT '   Sucursales sin empresa: ' + CAST(@BranchesWithoutCompany AS VARCHAR);

IF @BranchesWithoutCompany = 0
BEGIN
    PRINT '';
    PRINT '? Todas las sucursales ya tienen empresa asignada';
    PRINT '========================================';
    RETURN;
END

PRINT '   Empresa a asignar: ' + @CompanyName;
PRINT '';

-- ========================================
-- 3. MOSTRAR SUCURSALES QUE SE ACTUALIZARÁN
-- ========================================
PRINT '?? SUCURSALES QUE SE ACTUALIZARÁN:';
PRINT '-----------------------------------';

SELECT 
    Code AS Código,
    Name AS Nombre,
    City AS Ciudad,
    CASE WHEN IsActive = 1 THEN 'Activa' ELSE 'Inactiva' END AS Estado
FROM Branches
WHERE CompanyId IS NULL;

PRINT '';

-- ========================================
-- 4. ACTUALIZAR SUCURSALES
-- ========================================
PRINT '?? Actualizando sucursales...';

UPDATE Branches
SET 
    CompanyId = @MainCompanyId,
    UpdatedAt = GETUTCDATE()
WHERE CompanyId IS NULL;

DECLARE @RowsUpdated INT = @@ROWCOUNT;

PRINT '? ' + CAST(@RowsUpdated AS VARCHAR) + ' sucursales actualizadas';
PRINT '';

-- ========================================
-- 5. VERIFICAR RESULTADO
-- ========================================
PRINT '?? VERIFICACIÓN FINAL:';
PRINT '-----------------------------------';

SELECT 
    b.Code AS Código,
    b.Name AS Nombre,
    c.LegalName AS Empresa,
    b.City AS Ciudad,
    CASE WHEN b.IsActive = 1 THEN '? Activa' ELSE '? Inactiva' END AS Estado
FROM Branches b
INNER JOIN Companies c ON c.Id = b.CompanyId
ORDER BY b.Code;

PRINT '';

-- ========================================
-- 6. RESUMEN FINAL
-- ========================================
DECLARE @TotalBranches INT;
DECLARE @BranchesWithCompany INT;

SELECT @TotalBranches = COUNT(*) FROM Branches;
SELECT @BranchesWithCompany = COUNT(*) FROM Branches WHERE CompanyId IS NOT NULL;

PRINT '========================================';
PRINT '? ACTUALIZACIÓN COMPLETADA';
PRINT '========================================';
PRINT '   Total sucursales: ' + CAST(@TotalBranches AS VARCHAR);
PRINT '   Con empresa: ' + CAST(@BranchesWithCompany AS VARCHAR);
PRINT '   Sin empresa: ' + CAST(@TotalBranches - @BranchesWithCompany AS VARCHAR);
PRINT '';

IF @TotalBranches = @BranchesWithCompany
BEGIN
    PRINT '? Todas las sucursales tienen empresa asignada';
END
ELSE
BEGIN
    PRINT '??  Aún hay ' + CAST(@TotalBranches - @BranchesWithCompany AS VARCHAR) + ' sucursales sin empresa';
END

PRINT '========================================';
GO
