-- ============================================
-- SCRIPT DE LIMPIEZA PARA AWS
-- Elimina tablas obsoletas y migraciones incorrectas
-- Deja solo el sistema unificado correcto
-- ============================================

USE ERP;
GO

PRINT '?? Iniciando limpieza de base de datos...';
PRINT '==========================================================';
PRINT '';

-- ============================================
-- 1. ELIMINAR TABLAS OBSOLETAS DEL SISTEMA ANTIGUO
-- ============================================

PRINT '???  Paso 1: Eliminando tablas del sistema antiguo...';

-- Eliminar RolePermissions (sistema antiguo)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RolePermissions]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[RolePermissions];
    PRINT '   ? Tabla RolePermissions (antigua) eliminada';
END

-- Eliminar Permissions (sistema antiguo)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Permissions]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[Permissions];
    PRINT '   ? Tabla Permissions (antigua) eliminada';
END

-- Eliminar MenuItems si existe
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MenuItems]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[MenuItems];
    PRINT '   ? Tabla MenuItems eliminada';
END

PRINT '';

-- ============================================
-- 2. ELIMINAR TABLAS CON NOMBRES INCORRECTOS
-- ============================================

PRINT '???  Paso 2: Eliminando tablas con nombres incorrectos...';

-- Eliminar AppSubmodules
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppSubmodules]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[AppSubmodules];
    PRINT '   ? Tabla AppSubmodules eliminada';
END

-- Eliminar AppModules
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppModules]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[AppModules];
    PRINT '   ? Tabla AppModules eliminada';
END

PRINT '';

-- ============================================
-- 3. ELIMINAR MIGRACIONES OBSOLETAS DEL HISTORIAL
-- ============================================

PRINT '?? Paso 3: Limpiando historial de migraciones...';

-- Eliminar migraciones obsoletas que creaban tablas antiguas
DELETE FROM [__EFMigrationsHistory] 
WHERE [MigrationId] IN (
    '20251013220056_AddPermissionSystem',           -- Creaba Modules, Permissions, RolePermissions antiguo
    '20251013223059_AddModulePathIconAndMenuItems', -- Modificaba sistema antiguo
    '20251013223701_RevertModulePathIconAndMenuItems', -- Revertía cambios
    '20260303230356_AddAppModulesAndSubmodules'     -- Creaba AppModules y AppSubmodules
);

PRINT '   ? Migraciones obsoletas eliminadas del historial';

PRINT '';

-- ============================================
-- 4. VERIFICAR TABLAS CORRECTAS
-- ============================================

PRINT '?? Paso 4: Verificando tablas correctas...';
PRINT '';

DECLARE @HasModules BIT = 0;
DECLARE @HasSubmodules BIT = 0;
DECLARE @HasRoleModulePermissions BIT = 0;
DECLARE @HasUserModulePermissions BIT = 0;

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Modules]') AND type in (N'U'))
    SET @HasModules = 1;

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Submodules]') AND type in (N'U'))
    SET @HasSubmodules = 1;

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RoleModulePermissions]') AND type in (N'U'))
    SET @HasRoleModulePermissions = 1;

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserModulePermissions]') AND type in (N'U'))
    SET @HasUserModulePermissions = 1;

PRINT '? Tablas del sistema unificado:';
PRINT '   ?? Modules: ' + CASE WHEN @HasModules = 1 THEN '? Existe' ELSE '? Falta' END;
PRINT '   ?? Submodules: ' + CASE WHEN @HasSubmodules = 1 THEN '? Existe' ELSE '? Falta' END;
PRINT '   ?? RoleModulePermissions: ' + CASE WHEN @HasRoleModulePermissions = 1 THEN '? Existe' ELSE '? Falta' END;
PRINT '   ?? UserModulePermissions: ' + CASE WHEN @HasUserModulePermissions = 1 THEN '? Existe' ELSE '? Falta' END;

PRINT '';

-- ============================================
-- 5. VERIFICAR TABLAS OBSOLETAS ELIMINADAS
-- ============================================

PRINT '?? Paso 5: Verificando que tablas obsoletas fueron eliminadas...';
PRINT '';

DECLARE @HasOldPermissions BIT = 0;
DECLARE @HasOldRolePermissions BIT = 0;
DECLARE @HasAppModules BIT = 0;
DECLARE @HasAppSubmodules BIT = 0;

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Permissions]') AND type in (N'U'))
    SET @HasOldPermissions = 1;

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RolePermissions]') AND type in (N'U'))
    SET @HasOldRolePermissions = 1;

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppModules]') AND type in (N'U'))
    SET @HasAppModules = 1;

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppSubmodules]') AND type in (N'U'))
    SET @HasAppSubmodules = 1;

PRINT '???  Tablas obsoletas:';
PRINT '   ? Permissions (antigua): ' + CASE WHEN @HasOldPermissions = 0 THEN '? Eliminada' ELSE '??  Aún existe' END;
PRINT '   ? RolePermissions (antigua): ' + CASE WHEN @HasOldRolePermissions = 0 THEN '? Eliminada' ELSE '??  Aún existe' END;
PRINT '   ? AppModules: ' + CASE WHEN @HasAppModules = 0 THEN '? Eliminada' ELSE '??  Aún existe' END;
PRINT '   ? AppSubmodules: ' + CASE WHEN @HasAppSubmodules = 0 THEN '? Eliminada' ELSE '??  Aún existe' END;

PRINT '';

-- ============================================
-- 6. RESUMEN FINAL
-- ============================================

IF @HasModules = 1 AND @HasSubmodules = 1 AND @HasRoleModulePermissions = 1 AND @HasUserModulePermissions = 1
   AND @HasOldPermissions = 0 AND @HasOldRolePermissions = 0 AND @HasAppModules = 0 AND @HasAppSubmodules = 0
BEGIN
    PRINT '==========================================================';
    PRINT '? LIMPIEZA COMPLETADA EXITOSAMENTE';
    PRINT '==========================================================';
    PRINT '';
    PRINT '? Sistema unificado listo:';
    PRINT '   - Modules ?';
    PRINT '   - Submodules ?';
    PRINT '   - RoleModulePermissions ?';
    PRINT '   - UserModulePermissions ?';
    PRINT '';
    PRINT '? Tablas obsoletas eliminadas:';
    PRINT '   - Permissions (antigua) ???';
    PRINT '   - RolePermissions (antigua) ???';
    PRINT '   - AppModules ???';
    PRINT '   - AppSubmodules ???';
    PRINT '';
    PRINT '?? Siguiente paso:';
    PRINT '   Ejecutar: dotnet ef database update';
    PRINT '   Para sincronizar el modelo de EF Core con la BD limpia';
    PRINT '';
END
ELSE
BEGIN
    PRINT '==========================================================';
    PRINT '??  ADVERTENCIA: REVISAR ESTADO DE LA BASE DE DATOS';
    PRINT '==========================================================';
    PRINT '';
    
    IF @HasModules = 0 OR @HasSubmodules = 0 OR @HasRoleModulePermissions = 0 OR @HasUserModulePermissions = 0
    BEGIN
        PRINT '??  Faltan tablas del sistema unificado';
        PRINT '   Ejecuta: CreateModulesAndRoleModulePermissions_Local.sql';
        PRINT '';
    END
    
    IF @HasOldPermissions = 1 OR @HasOldRolePermissions = 1 OR @HasAppModules = 1 OR @HasAppSubmodules = 1
    BEGIN
        PRINT '??  Aún existen tablas obsoletas';
        PRINT '   Revisa los mensajes anteriores para ver cuáles';
        PRINT '';
    END
END

GO
