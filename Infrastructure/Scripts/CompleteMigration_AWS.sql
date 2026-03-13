-- ============================================
-- SCRIPT COMPLETO PARA AWS
-- Limpia base de datos y crea estructura correcta
-- ============================================

USE ERP;
GO

PRINT '?? INICIO DEL PROCESO DE MIGRACIÓN COMPLETA';
PRINT '==========================================================';
PRINT '';

-- ============================================
-- PASO 1: ELIMINAR TABLAS OBSOLETAS
-- ============================================

PRINT '???  PASO 1: Eliminando tablas obsoletas...';
PRINT '';

-- Eliminar RolePermissions (sistema antiguo)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RolePermissions]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[RolePermissions];
    PRINT '   ? RolePermissions (antigua) eliminada';
END

-- Eliminar Permissions (sistema antiguo)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Permissions]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[Permissions];
    PRINT '   ? Permissions (antigua) eliminada';
END

-- Eliminar MenuItems
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MenuItems]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[MenuItems];
    PRINT '   ? MenuItems eliminada';
END

-- Eliminar AppSubmodules
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppSubmodules]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[AppSubmodules];
    PRINT '   ? AppSubmodules eliminada';
END

-- Eliminar AppModules
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppModules]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[AppModules];
    PRINT '   ? AppModules eliminada';
END

PRINT '';

-- ============================================
-- PASO 2: LIMPIAR HISTORIAL DE MIGRACIONES OBSOLETAS
-- ============================================

PRINT '?? PASO 2: Limpiando historial de migraciones...';
PRINT '';

DELETE FROM [__EFMigrationsHistory] 
WHERE [MigrationId] IN (
    '20251013220056_AddPermissionSystem',
    '20251013223059_AddModulePathIconAndMenuItems',
    '20251013223701_RevertModulePathIconAndMenuItems',
    '20260303230356_AddAppModulesAndSubmodules'
);

PRINT '   ? Migraciones obsoletas eliminadas';
PRINT '';

-- ============================================
-- PASO 3: CREAR TABLA Modules
-- ============================================

PRINT '?? PASO 3: Creando tabla Modules...';
PRINT '';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Modules]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Modules] (
        [Id] INT NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [Path] NVARCHAR(200) NOT NULL,
        [Icon] NVARCHAR(50) NOT NULL,
        [Order] INT NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [PK_Modules] PRIMARY KEY ([Id])
    );
    
    CREATE INDEX [IX_Modules_Order] ON [dbo].[Modules]([Order]);
    CREATE INDEX [IX_Modules_IsActive] ON [dbo].[Modules]([IsActive]);
    
    PRINT '   ? Tabla Modules creada';
END
ELSE
BEGIN
    PRINT '   ??  Tabla Modules ya existe';
END

PRINT '';

-- ============================================
-- PASO 4: CREAR TABLA Submodules
-- ============================================

PRINT '?? PASO 4: Creando tabla Submodules...';
PRINT '';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Submodules]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Submodules] (
        [Id] INT NOT NULL,
        [ModuleId] INT NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [Path] NVARCHAR(200) NOT NULL,
        [Icon] NVARCHAR(50) NOT NULL,
        [Order] INT NOT NULL,
        [Color] NVARCHAR(100) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [PK_Submodules] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Submodules_Modules_ModuleId] FOREIGN KEY ([ModuleId]) 
            REFERENCES [dbo].[Modules]([Id]) ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_Submodules_ModuleId] ON [dbo].[Submodules]([ModuleId]);
    CREATE INDEX [IX_Submodules_Order] ON [dbo].[Submodules]([Order]);
    CREATE INDEX [IX_Submodules_IsActive] ON [dbo].[Submodules]([IsActive]);
    
    PRINT '   ? Tabla Submodules creada';
END
ELSE
BEGIN
    PRINT '   ??  Tabla Submodules ya existe';
END

PRINT '';

-- ============================================
-- PASO 5: CREAR TABLA RoleModulePermissions
-- ============================================

PRINT '?? PASO 5: Creando tabla RoleModulePermissions...';
PRINT '';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RoleModulePermissions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RoleModulePermissions] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [RoleId] INT NOT NULL,
        [ModuleId] INT NOT NULL,
        [SubmoduleId] INT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Path] NVARCHAR(200) NOT NULL,
        [Icon] NVARCHAR(50) NOT NULL,
        [Order] INT NOT NULL,
        [HasAccess] BIT NOT NULL DEFAULT 0,
        [CanView] BIT NOT NULL DEFAULT 0,
        [CanCreate] BIT NOT NULL DEFAULT 0,
        [CanEdit] BIT NOT NULL DEFAULT 0,
        [CanDelete] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedByUserId] INT NULL,
        CONSTRAINT [PK_RoleModulePermissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RoleModulePermissions_Roles_RoleId] FOREIGN KEY ([RoleId]) 
            REFERENCES [dbo].[Roles]([Id]) ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_RoleModulePermissions_RoleId] ON [dbo].[RoleModulePermissions]([RoleId]);
    CREATE INDEX [IX_RoleModulePermissions_ModuleId] ON [dbo].[RoleModulePermissions]([ModuleId]);
    CREATE UNIQUE INDEX [IX_RoleModulePermissions_RoleId_ModuleId_SubmoduleId] 
        ON [dbo].[RoleModulePermissions]([RoleId], [ModuleId], [SubmoduleId]) 
        WHERE [SubmoduleId] IS NOT NULL;
    
    PRINT '   ? Tabla RoleModulePermissions creada';
END
ELSE
BEGIN
    PRINT '   ??  Tabla RoleModulePermissions ya existe';
END

PRINT '';

-- ============================================
-- PASO 6: REGISTRAR MIGRACIÓN EN HISTORIAL
-- ============================================

PRINT '?? PASO 6: Registrando migración en historial...';
PRINT '';

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260310000000_AddModulesSubmodulesAndRoleModulePermissions')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260310000000_AddModulesSubmodulesAndRoleModulePermissions', N'8.0.0');
    
    PRINT '   ? Migración registrada';
END
ELSE
BEGIN
    PRINT '   ??  Migración ya registrada';
END

PRINT '';

-- ============================================
-- PASO 7: VERIFICACIÓN FINAL
-- ============================================

PRINT '?? PASO 7: Verificación final...';
PRINT '';

DECLARE @HasModules BIT = 0;
DECLARE @HasSubmodules BIT = 0;
DECLARE @HasRoleModulePermissions BIT = 0;
DECLARE @HasUserModulePermissions BIT = 0;
DECLARE @HasOldPermissions BIT = 0;
DECLARE @HasOldRolePermissions BIT = 0;
DECLARE @HasAppModules BIT = 0;

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Modules]') AND type in (N'U'))
    SET @HasModules = 1;
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Submodules]') AND type in (N'U'))
    SET @HasSubmodules = 1;
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RoleModulePermissions]') AND type in (N'U'))
    SET @HasRoleModulePermissions = 1;
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserModulePermissions]') AND type in (N'U'))
    SET @HasUserModulePermissions = 1;
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Permissions]') AND type in (N'U'))
    SET @HasOldPermissions = 1;
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RolePermissions]') AND type in (N'U'))
    SET @HasOldRolePermissions = 1;
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppModules]') AND type in (N'U'))
    SET @HasAppModules = 1;

PRINT '? Tablas correctas:';
PRINT '   ?? Modules: ' + CASE WHEN @HasModules = 1 THEN '?' ELSE '?' END;
PRINT '   ?? Submodules: ' + CASE WHEN @HasSubmodules = 1 THEN '?' ELSE '?' END;
PRINT '   ?? RoleModulePermissions: ' + CASE WHEN @HasRoleModulePermissions = 1 THEN '?' ELSE '?' END;
PRINT '   ?? UserModulePermissions: ' + CASE WHEN @HasUserModulePermissions = 1 THEN '?' ELSE '?' END;

PRINT '';
PRINT '???  Tablas obsoletas (deben estar eliminadas):';
PRINT '   ? Permissions: ' + CASE WHEN @HasOldPermissions = 0 THEN '? Eliminada' ELSE '??  Aún existe' END;
PRINT '   ? RolePermissions: ' + CASE WHEN @HasOldRolePermissions = 0 THEN '? Eliminada' ELSE '??  Aún existe' END;
PRINT '   ? AppModules: ' + CASE WHEN @HasAppModules = 0 THEN '? Eliminada' ELSE '??  Aún existe' END;

PRINT '';

-- ============================================
-- RESUMEN FINAL
-- ============================================

IF @HasModules = 1 AND @HasSubmodules = 1 AND @HasRoleModulePermissions = 1 AND @HasUserModulePermissions = 1
   AND @HasOldPermissions = 0 AND @HasOldRolePermissions = 0 AND @HasAppModules = 0
BEGIN
    PRINT '==========================================================';
    PRINT '? MIGRACIÓN COMPLETADA EXITOSAMENTE';
    PRINT '==========================================================';
    PRINT '';
    PRINT '?? Próximos pasos:';
    PRINT '   1. Ejecutar: SeedSystemModules_Portable.sql';
    PRINT '      (Para insertar módulos y submódulos)';
    PRINT '';
    PRINT '   2. Ejecutar: AssignFullPermissionsToAdmin_Portable.sql';
    PRINT '      (Para asignar permisos al Administrador)';
    PRINT '';
    PRINT '   3. Reiniciar la aplicación .NET';
    PRINT '';
END
ELSE
BEGIN
    PRINT '==========================================================';
    PRINT '??  ADVERTENCIA: REVISAR ESTADO';
    PRINT '==========================================================';
    PRINT '';
    IF @HasModules = 0 OR @HasSubmodules = 0 OR @HasRoleModulePermissions = 0
    BEGIN
        PRINT '? Faltan tablas necesarias';
    END
    IF @HasOldPermissions = 1 OR @HasOldRolePermissions = 1 OR @HasAppModules = 1
    BEGIN
        PRINT '? Aún existen tablas obsoletas';
    END
    PRINT '';
END

GO
