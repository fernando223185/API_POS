-- ? SCRIPT DE REVERSIÓN - Volver a nombres originales
-- Ejecutar DESDE tu instancia de AWS/Linux conectado a SQL Server

USE ERP;
GO

PRINT '?? REVIRTIENDO cambios incorrectos...';
GO

-- ============================================
-- PASO 1: VERIFICAR ESTADO ACTUAL
-- ============================================
PRINT '?? Verificando tablas actuales...';

SELECT name AS TableName
FROM sys.tables 
WHERE name LIKE '%Module%'
ORDER BY name;
GO

-- ============================================
-- PASO 2: REVERTIR NOMBRES A LOS ORIGINALES
-- ============================================
PRINT '?? Revirtiendo a nombres originales: Modules y Submodules';

-- Revertir SystemModules ? Modules (si existe)
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SystemModules')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Modules')
    BEGIN
        EXEC sp_rename 'SystemModules', 'Modules';
        PRINT '? Revertido SystemModules ? Modules';
    END
    ELSE
    BEGIN
        PRINT '?? Modules ya existe, eliminando SystemModules duplicado';
        DROP TABLE SystemModules;
    END
END
ELSE
BEGIN
    PRINT '? SystemModules no existe, no se requiere reversión';
END

-- Revertir SystemSubmodules ? Submodules (si existe)
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SystemSubmodules')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Submodules')
    BEGIN
        EXEC sp_rename 'SystemSubmodules', 'Submodules';
        PRINT '? Revertido SystemSubmodules ? Submodules';
    END
    ELSE
    BEGIN
        PRINT '?? Submodules ya existe, eliminando SystemSubmodules duplicado';
        DROP TABLE SystemSubmodules;
    END
END
ELSE
BEGIN
    PRINT '? SystemSubmodules no existe, no se requiere reversión';
END
GO

-- ============================================
-- PASO 3: ELIMINAR MIGRACIONES INCORRECTAS DEL HISTORIAL
-- ============================================
PRINT '??? Eliminando migraciones problemáticas del historial...';

DELETE FROM __EFMigrationsHistory 
WHERE MigrationId IN (
    '20260303234231_RenameToModulesAndSubmodules',
    '20260304190142_RemoveOldPermissionsSystem'
);

PRINT '? Migraciones problemáticas eliminadas';
GO

-- ============================================
-- PASO 4: VERIFICACIÓN FINAL
-- ============================================
PRINT '';
PRINT '?? VERIFICACIÓN FINAL:';
PRINT '==========================================';

PRINT '';
PRINT '? Tablas actuales:';
SELECT name AS TableName
FROM sys.tables 
WHERE name LIKE '%Module%'
ORDER BY name;

PRINT '';
PRINT '? Migraciones aplicadas:';
SELECT MigrationId 
FROM __EFMigrationsHistory
WHERE MigrationId LIKE '%Module%'
ORDER BY MigrationId;

PRINT '';
PRINT '?? ¡REVERSIÓN COMPLETADA!';
PRINT '';
PRINT 'TABLAS CORRECTAS:';
PRINT '- Modules (ORIGINAL)';
PRINT '- Submodules (ORIGINAL)';
PRINT '- UserModulePermissions';
PRINT '- RoleModulePermissions';
PRINT '';
PRINT 'SIGUIENTE PASO:';
PRINT '1. Eliminar migraciones problemáticas en Visual Studio';
PRINT '2. Crear nueva migración limpia';
PRINT '3. Republicar en AWS';
PRINT '';
GO
