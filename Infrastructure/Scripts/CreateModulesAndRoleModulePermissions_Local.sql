-- ============================================
-- SCRIPT PARA LOCALHOST
-- Crear tablas Modules, Submodules y RoleModulePermissions
-- Eliminar tablas obsoletas AppModules y AppSubmodules
-- ============================================

USE ERP;
GO

PRINT '?? Iniciando creación de tablas del sistema de módulos...';
PRINT '==========================================================';
PRINT '';

-- ============================================
-- 1. ELIMINAR TABLAS OBSOLETAS SI EXISTEN
-- ============================================

PRINT '???  Paso 1: Eliminando tablas obsoletas...';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppSubmodules]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[AppSubmodules];
    PRINT '   ? Tabla AppSubmodules eliminada';
END
ELSE
BEGIN
    PRINT '   ??  Tabla AppSubmodules no existe (OK)';
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppModules]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[AppModules];
    PRINT '   ? Tabla AppModules eliminada';
END
ELSE
BEGIN
    PRINT '   ??  Tabla AppModules no existe (OK)';
END

PRINT '';

-- ============================================
-- 2. CREAR TABLA Modules
-- ============================================

PRINT '?? Paso 2: Creando tabla Modules...';

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
    
    PRINT '   ? Tabla Modules creada exitosamente';
END
ELSE
BEGIN
    PRINT '   ??  Tabla Modules ya existe';
END

PRINT '';

-- ============================================
-- 3. CREAR TABLA Submodules
-- ============================================

PRINT '?? Paso 3: Creando tabla Submodules...';

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
    
    PRINT '   ? Tabla Submodules creada exitosamente';
END
ELSE
BEGIN
    PRINT '   ??  Tabla Submodules ya existe';
END

PRINT '';

-- ============================================
-- 4. CREAR TABLA RoleModulePermissions
-- ============================================

PRINT '?? Paso 4: Creando tabla RoleModulePermissions...';

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
    
    PRINT '   ? Tabla RoleModulePermissions creada exitosamente';
END
ELSE
BEGIN
    PRINT '   ??  Tabla RoleModulePermissions ya existe';
END

PRINT '';

-- ============================================
-- 5. INSERTAR REGISTRO EN __EFMigrationsHistory
-- ============================================

PRINT '?? Paso 5: Registrando migración en historial...';

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260310000000_AddModulesSubmodulesAndRoleModulePermissions')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260310000000_AddModulesSubmodulesAndRoleModulePermissions', N'8.0.0');
    
    PRINT '   ? Migración registrada en historial';
END
ELSE
BEGIN
    PRINT '   ??  Migración ya registrada';
END

PRINT '';

-- ============================================
-- 6. VERIFICAR ESTRUCTURA
-- ============================================

PRINT '?? Paso 6: Verificando estructura creada...';
PRINT '';

DECLARE @ModulesExists BIT = 0;
DECLARE @SubmodulesExists BIT = 0;
DECLARE @RoleModulePermissionsExists BIT = 0;

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Modules]') AND type in (N'U'))
    SET @ModulesExists = 1;

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Submodules]') AND type in (N'U'))
    SET @SubmodulesExists = 1;

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RoleModulePermissions]') AND type in (N'U'))
    SET @RoleModulePermissionsExists = 1;

PRINT '   ?? Modules: ' + CASE WHEN @ModulesExists = 1 THEN '? Existe' ELSE '? No existe' END;
PRINT '   ?? Submodules: ' + CASE WHEN @SubmodulesExists = 1 THEN '? Existe' ELSE '? No existe' END;
PRINT '   ?? RoleModulePermissions: ' + CASE WHEN @RoleModulePermissionsExists = 1 THEN '? Existe' ELSE '? No existe' END;

PRINT '';

-- ============================================
-- 7. RESUMEN FINAL
-- ============================================

IF @ModulesExists = 1 AND @SubmodulesExists = 1 AND @RoleModulePermissionsExists = 1
BEGIN
    PRINT '==========================================================';
    PRINT '? SCRIPT EJECUTADO EXITOSAMENTE';
    PRINT '==========================================================';
    PRINT '';
    PRINT '? Todas las tablas fueron creadas correctamente:';
    PRINT '   - Modules';
    PRINT '   - Submodules';
    PRINT '   - RoleModulePermissions';
    PRINT '';
    PRINT '?? Próximos pasos:';
    PRINT '   1. Ejecutar script de seed de módulos: SeedSystemModules_Portable.sql';
    PRINT '   2. Asignar permisos al administrador: AssignFullPermissionsToAdmin_Portable.sql';
    PRINT '   3. Reiniciar la aplicación para que reconozca las nuevas tablas';
    PRINT '';
END
ELSE
BEGIN
    PRINT '==========================================================';
    PRINT '??  ADVERTENCIA: ALGUNAS TABLAS NO FUERON CREADAS';
    PRINT '==========================================================';
    PRINT '';
    PRINT 'Verifica los errores anteriores e intenta ejecutar nuevamente.';
    PRINT '';
END

GO
