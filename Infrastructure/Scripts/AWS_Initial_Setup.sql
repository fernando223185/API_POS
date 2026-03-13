-- ============================================
-- SCRIPT DE INICIALIZACIÓN PARA AWS
-- Ejecutar ANTES de la primera ejecución de la aplicación
-- ============================================

USE ERP;
GO

PRINT '?? INICIANDO CONFIGURACIÓN INICIAL DE BASE DE DATOS AWS';
PRINT '==========================================================';
PRINT '';

-- ============================================
-- PASO 1: CREAR TABLAS DEL SISTEMA UNIFICADO
-- ============================================

PRINT '?? PASO 1: Creando tablas del sistema de módulos...';
PRINT '';

-- Tabla Modules
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

-- Tabla Submodules
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

-- Tabla RoleModulePermissions
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
-- PASO 2: MARCAR MIGRACIONES PROBLEMÁTICAS COMO APLICADAS
-- ============================================

PRINT '?? PASO 2: Registrando migraciones en historial...';
PRINT '';

-- Registrar migración de módulos
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260310000000_AddModulesSubmodulesAndRoleModulePermissions')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260310000000_AddModulesSubmodulesAndRoleModulePermissions', N'8.0.0');
    PRINT '   ? Migración AddModulesSubmodulesAndRoleModulePermissions registrada';
END

-- Marcar AddCustomerERPFieldsClean como aplicada si las columnas ya existen
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Products]') AND name = 'AllowFractionalQuantities')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20251111193648_AddCustomerERPFieldsClean')
    BEGIN
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES (N'20251111193648_AddCustomerERPFieldsClean', N'7.0.20');
        PRINT '   ? Migración AddCustomerERPFieldsClean marcada como aplicada';
    END
END

PRINT '';

-- ============================================
-- PASO 3: INSERTAR DATOS INICIALES DE MÓDULOS
-- ============================================

PRINT '?? PASO 3: Insertando módulos del sistema...';
PRINT '';

-- Insertar módulos solo si la tabla está vacía
IF NOT EXISTS (SELECT 1 FROM [Modules])
BEGIN
    -- Módulos principales
    INSERT INTO [Modules] ([Id], [Name], [Description], [Path], [Icon], [Order], [IsActive], [CreatedAt])
    VALUES
        (1, 'Dashboard', 'Panel de control y resumen ejecutivo', '/dashboard', 'fa-tachometer-alt', 1, 1, GETUTCDATE()),
        (2, 'Ventas', 'Gestión de ventas y cotizaciones', '/ventas', 'fa-shopping-cart', 2, 1, GETUTCDATE()),
        (3, 'Productos', 'Catálogo de productos y servicios', '/productos', 'fa-box', 3, 1, GETUTCDATE()),
        (4, 'Clientes', 'CRM y gestión de clientes', '/clientes', 'fa-users', 4, 1, GETUTCDATE()),
        (5, 'Facturación', 'Facturación electrónica y documentos fiscales', '/facturacion', 'fa-file-invoice-dollar', 5, 1, GETUTCDATE()),
        (6, 'Reportes', 'Reportes y análisis de datos', '/reportes', 'fa-chart-bar', 6, 1, GETUTCDATE()),
        (7, 'Administración', 'Configuración del sistema', '/administracion', 'fa-cog', 7, 1, GETUTCDATE()),
        (8, 'Usuarios', 'Gestión de usuarios y roles', '/usuarios', 'fa-user-shield', 8, 1, GETUTCDATE());
    
    PRINT '   ? 8 módulos insertados';
    
    -- Submódulos
    INSERT INTO [Submodules] ([Id], [ModuleId], [Name], [Description], [Path], [Icon], [Order], [Color], [IsActive], [CreatedAt])
    VALUES
        -- Dashboard (Módulo 1)
        (101, 1, 'Resumen General', 'Vista general del sistema', '/dashboard/overview', 'fa-chart-line', 1, '#3498db', 1, GETUTCDATE()),
        (102, 1, 'Métricas de Ventas', 'Indicadores de rendimiento de ventas', '/dashboard/sales-metrics', 'fa-dollar-sign', 2, '#2ecc71', 1, GETUTCDATE()),
        
        -- Ventas (Módulo 2)
        (201, 2, 'Nueva Venta', 'Punto de venta', '/ventas/nueva', 'fa-cash-register', 1, '#27ae60', 1, GETUTCDATE()),
        (202, 2, 'Historial de Ventas', 'Consultar ventas realizadas', '/ventas/historial', 'fa-history', 2, '#3498db', 1, GETUTCDATE()),
        (203, 2, 'Cotizaciones', 'Gestión de cotizaciones', '/ventas/cotizaciones', 'fa-file-alt', 3, '#f39c12', 1, GETUTCDATE()),
        (204, 2, 'Devoluciones', 'Gestión de devoluciones', '/ventas/devoluciones', 'fa-undo', 4, '#e74c3c', 1, GETUTCDATE()),
        
        -- Productos (Módulo 3)
        (301, 3, 'Catálogo', 'Ver y gestionar productos', '/productos/catalogo', 'fa-list', 1, '#9b59b6', 1, GETUTCDATE()),
        (302, 3, 'Nuevo Producto', 'Agregar nuevo producto', '/productos/nuevo', 'fa-plus-circle', 2, '#1abc9c', 1, GETUTCDATE()),
        (303, 3, 'Categorías', 'Gestión de categorías', '/productos/categorias', 'fa-tags', 3, '#34495e', 1, GETUTCDATE()),
        (304, 3, 'Inventario', 'Control de inventario', '/productos/inventario', 'fa-warehouse', 4, '#16a085', 1, GETUTCDATE()),
        (305, 3, 'Listas de Precios', 'Gestión de precios', '/productos/listas-precios', 'fa-money-bill', 5, '#2980b9', 1, GETUTCDATE()),
        
        -- Clientes (Módulo 4)
        (401, 4, 'Directorio', 'Lista de clientes', '/clientes/directorio', 'fa-address-book', 1, '#3498db', 1, GETUTCDATE()),
        (402, 4, 'Nuevo Cliente', 'Registrar cliente', '/clientes/nuevo', 'fa-user-plus', 2, '#2ecc71', 1, GETUTCDATE()),
        (403, 4, 'Historial de Compras', 'Ver historial de compras de clientes', '/clientes/historial', 'fa-shopping-bag', 3, '#9b59b6', 1, GETUTCDATE()),
        (404, 4, 'Estado de Cuenta', 'Cuentas por cobrar', '/clientes/estado-cuenta', 'fa-file-invoice', 4, '#e67e22', 1, GETUTCDATE()),
        
        -- Facturación (Módulo 5)
        (501, 5, 'Generar Factura', 'Crear factura electrónica', '/facturacion/generar', 'fa-file-invoice', 1, '#27ae60', 1, GETUTCDATE()),
        (502, 5, 'Facturas Emitidas', 'Consultar facturas', '/facturacion/emitidas', 'fa-receipt', 2, '#3498db', 1, GETUTCDATE()),
        (503, 5, 'Notas de Crédito', 'Gestión de notas de crédito', '/facturacion/notas-credito', 'fa-file-medical', 3, '#e74c3c', 1, GETUTCDATE()),
        (504, 5, 'Configuración SAT', 'Configuración de facturación SAT', '/facturacion/config-sat', 'fa-cogs', 4, '#95a5a6', 1, GETUTCDATE()),
        
        -- Reportes (Módulo 6)
        (601, 6, 'Ventas', 'Reporte de ventas', '/reportes/ventas', 'fa-chart-line', 1, '#3498db', 1, GETUTCDATE()),
        (602, 6, 'Productos Más Vendidos', 'Análisis de productos', '/reportes/productos-vendidos', 'fa-trophy', 2, '#f39c12', 1, GETUTCDATE()),
        (603, 6, 'Clientes', 'Reporte de clientes', '/reportes/clientes', 'fa-users', 3, '#9b59b6', 1, GETUTCDATE()),
        (604, 6, 'Inventario', 'Reporte de inventario', '/reportes/inventario', 'fa-boxes', 4, '#16a085', 1, GETUTCDATE()),
        (605, 6, 'Financiero', 'Reportes financieros', '/reportes/financiero', 'fa-money-check-alt', 5, '#27ae60', 1, GETUTCDATE()),
        
        -- Administración (Módulo 7)
        (701, 7, 'Empresa', 'Datos de la empresa', '/administracion/empresa', 'fa-building', 1, '#34495e', 1, GETUTCDATE()),
        (702, 7, 'Sucursales', 'Gestión de sucursales', '/administracion/sucursales', 'fa-store', 2, '#16a085', 1, GETUTCDATE()),
        (703, 7, 'Configuración General', 'Configuración del sistema', '/administracion/config', 'fa-sliders-h', 3, '#95a5a6', 1, GETUTCDATE()),
        (704, 7, 'Módulos del Sistema', 'Gestión de módulos y submódulos', '/administracion/modulos', 'fa-th-large', 4, '#3498db', 1, GETUTCDATE()),
        
        -- Usuarios (Módulo 8)
        (801, 8, 'Usuarios', 'Gestión de usuarios', '/usuarios/lista', 'fa-users-cog', 1, '#2c3e50', 1, GETUTCDATE()),
        (802, 8, 'Roles', 'Gestión de roles', '/usuarios/roles', 'fa-user-tag', 2, '#8e44ad', 1, GETUTCDATE()),
        (803, 8, 'Permisos', 'Asignación de permisos', '/usuarios/permisos', 'fa-key', 3, '#c0392b', 1, GETUTCDATE());
    
    PRINT '   ? 29 submódulos insertados';
END
ELSE
BEGIN
    PRINT '   ??  Los módulos ya existen, omitiendo inserción';
END

PRINT '';

-- ============================================
-- PASO 4: ASIGNAR PERMISOS COMPLETOS AL ADMINISTRADOR
-- ============================================

PRINT '?? PASO 4: Asignando permisos al rol Administrador...';
PRINT '';

-- Limpiar permisos existentes del rol Administrador
DELETE FROM [RoleModulePermissions] WHERE [RoleId] = 1;

-- Insertar permisos para módulos principales
INSERT INTO [RoleModulePermissions] ([RoleId], [ModuleId], [SubmoduleId], [Name], [Path], [Icon], [Order], [HasAccess], [CanView], [CanCreate], [CanEdit], [CanDelete], [CreatedAt])
SELECT 
    1 AS RoleId,
    [Id] AS ModuleId,
    NULL AS SubmoduleId,
    [Name],
    [Path],
    [Icon],
    [Order],
    1 AS HasAccess,
    0 AS CanView,
    0 AS CanCreate,
    0 AS CanEdit,
    0 AS CanDelete,
    GETUTCDATE() AS CreatedAt
FROM [Modules]
WHERE [IsActive] = 1;

-- Insertar permisos para submódulos
INSERT INTO [RoleModulePermissions] ([RoleId], [ModuleId], [SubmoduleId], [Name], [Path], [Icon], [Order], [HasAccess], [CanView], [CanCreate], [CanEdit], [CanDelete], [CreatedAt])
SELECT 
    1 AS RoleId,
    s.[ModuleId],
    s.[Id] AS SubmoduleId,
    s.[Name],
    s.[Path],
    s.[Icon],
    s.[Order],
    1 AS HasAccess,
    1 AS CanView,
    1 AS CanCreate,
    1 AS CanEdit,
    1 AS CanDelete,
    GETUTCDATE() AS CreatedAt
FROM [Submodules] s
WHERE s.[IsActive] = 1;

DECLARE @TotalPermisos INT = (SELECT COUNT(*) FROM [RoleModulePermissions] WHERE [RoleId] = 1);
PRINT '   ? ' + CAST(@TotalPermisos AS NVARCHAR(10)) + ' permisos asignados al Administrador';

PRINT '';

-- ============================================
-- RESUMEN FINAL
-- ============================================

PRINT '==========================================================';
PRINT '? CONFIGURACIÓN INICIAL COMPLETADA';
PRINT '==========================================================';
PRINT '';
PRINT '? Sistema listo para ejecutar la aplicación .NET';
PRINT '';
PRINT '?? Resumen:';
PRINT '   - Tablas creadas: Modules, Submodules, RoleModulePermissions';
PRINT '   - Módulos: 8';
PRINT '   - Submódulos: 29';
PRINT '   - Permisos de Administrador: ' + CAST(@TotalPermisos AS NVARCHAR(10));
PRINT '';
PRINT '?? Siguiente paso:';
PRINT '   Ejecutar: dotnet Web.Api.dll';
PRINT '';

GO
