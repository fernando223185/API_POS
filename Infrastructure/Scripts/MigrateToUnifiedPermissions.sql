-- ============================================
-- MIGRACIÓN COMPLETA: Sistema Antiguo ? Sistema Nuevo
-- RolePermissions + Permissions ? RoleModulePermissions
-- ============================================

USE ERP;
GO

PRINT '?? INICIANDO MIGRACIÓN DE PERMISOS...';
PRINT '=====================================';
PRINT '';

-- ============================================
-- PASO 1: MAPEO DE PERMISOS ANTIGUOS A MÓDULOS/SUBMÓDULOS
-- ============================================

PRINT '?? Paso 1: Creando tabla de mapeo de permisos...';

-- Tabla temporal para mapear permisos antiguos a módulos/submódulos
IF OBJECT_ID('tempdb..#PermissionMapping') IS NOT NULL
    DROP TABLE #PermissionMapping;

CREATE TABLE #PermissionMapping (
    PermissionId INT,
    PermissionName NVARCHAR(100),
    PermissionResource NVARCHAR(100),
    ModuleId INT,
    SubmoduleId INT NULL,
    CanView BIT,
    CanCreate BIT,
    CanEdit BIT,
    CanDelete BIT
);

-- Mapeo de permisos Customer ? Módulo Clientes (5)
INSERT INTO #PermissionMapping VALUES
(1, 'Create', 'Customer', 5, 52, 0, 1, 0, 0),      -- Create ? Nuevo Cliente (CanCreate)
(2, 'Read', 'Customer', 5, 51, 1, 0, 0, 0),        -- Read ? Listado de Clientes (CanView)
(3, 'Update', 'Customer', 5, 51, 0, 0, 1, 0),      -- Update ? Listado de Clientes (CanEdit)
(4, 'Delete', 'Customer', 5, 51, 0, 0, 0, 1),      -- Delete ? Listado de Clientes (CanDelete)
(5, 'ViewList', 'Customer', 5, 51, 1, 0, 0, 0);    -- ViewList ? Listado de Clientes (CanView)

-- Mapeo de permisos Sale ? Módulo Ventas (2)
INSERT INTO #PermissionMapping VALUES
(15, 'Create', 'Sale', 2, 21, 0, 1, 0, 0),         -- Create ? Nueva Venta (CanCreate)
(16, 'Read', 'Sale', 2, 22, 1, 0, 0, 0),           -- Read ? Historial de Ventas (CanView)
(17, 'Update', 'Sale', 2, 22, 0, 0, 1, 0),         -- Update ? Historial de Ventas (CanEdit)
(18, 'Delete', 'Sale', 2, 22, 0, 0, 0, 1),         -- Delete ? Historial de Ventas (CanDelete)
(19, 'CreateSale', 'Sale', 2, 21, 1, 1, 0, 0),     -- CreateSale ? Nueva Venta (CanView + CanCreate)
(20, 'ViewHistory', 'Sale', 2, 22, 1, 0, 0, 0),    -- ViewHistory ? Historial de Ventas (CanView)
(21, 'ProcessRefund', 'Sale', 2, 24, 0, 1, 0, 0),  -- ProcessRefund ? Devoluciones (CanCreate)
(22, 'ProcessPayment', 'Sale', 2, 21, 0, 1, 0, 0); -- ProcessPayment ? Nueva Venta (CanCreate)

-- Mapeo de permisos Product ? Módulo Productos (3)
INSERT INTO #PermissionMapping VALUES
(23, 'Create', 'Product', 3, 32, 0, 1, 0, 0),      -- Create ? Nuevo Producto (CanCreate)
(24, 'Read', 'Product', 3, 31, 1, 0, 0, 0),        -- Read ? Catálogo de Productos (CanView)
(25, 'Update', 'Product', 3, 31, 0, 0, 1, 0),      -- Update ? Catálogo de Productos (CanEdit)
(26, 'Delete', 'Product', 3, 31, 0, 0, 0, 1),      -- Delete ? Catálogo de Productos (CanDelete)
(27, 'ViewCatalog', 'Product', 3, 31, 1, 0, 0, 0), -- ViewCatalog ? Catálogo de Productos (CanView)
(28, 'ManagePrices', 'Product', 3, 31, 0, 0, 1, 0),-- ManagePrices ? Catálogo de Productos (CanEdit)
(29, 'ImportProducts', 'Product', 3, 33, 0, 1, 0, 0), -- ImportProducts ? Importar Productos (CanCreate)
(30, 'ExportProducts', 'Product', 3, 31, 1, 0, 0, 0), -- ExportProducts ? Catálogo de Productos (CanView)
(31, 'ManageCategories', 'Product', 3, 34, 0, 0, 1, 0); -- ManageCategories ? Categorías (CanEdit)

-- Mapeo de permisos User ? Módulo Configuración (8) - Usuarios
INSERT INTO #PermissionMapping VALUES
(37, 'Create', 'User', 8, 81, 0, 1, 0, 0),         -- Create ? Usuarios (CanCreate)
(38, 'Read', 'User', 8, 81, 1, 0, 0, 0),           -- Read ? Usuarios (CanView)
(39, 'Update', 'User', 8, 81, 0, 0, 1, 0),         -- Update ? Usuarios (CanEdit)
(40, 'Delete', 'User', 8, 81, 0, 0, 0, 1);         -- Delete ? Usuarios (CanDelete)

-- Mapeo de permisos Inventory ? Módulo Inventario (4)
INSERT INTO #PermissionMapping VALUES
(41, 'ViewStock', 'Inventory', 4, 41, 1, 0, 0, 0), -- ViewStock ? Stock Actual (CanView)
(42, 'ViewKardex', 'Inventory', 4, 42, 1, 0, 0, 0),-- ViewKardex ? Kardex (CanView)
(43, 'AdjustStock', 'Inventory', 4, 44, 0, 1, 0, 0), -- AdjustStock ? Movimientos (CanCreate)
(44, 'TransferStock', 'Inventory', 4, 44, 0, 1, 0, 0); -- TransferStock ? Movimientos (CanCreate)

-- Mapeo de permisos Billing ? Módulo CFDI (6)
INSERT INTO #PermissionMapping VALUES
(52, 'ViewInvoices', 'Billing', 6, 62, 1, 0, 0, 0),    -- ViewInvoices ? Facturas Emitidas (CanView)
(53, 'CreateInvoice', 'Billing', 6, 61, 0, 1, 0, 0),   -- CreateInvoice ? Nueva Factura (CanCreate)
(54, 'ViewPending', 'Billing', 6, 63, 1, 0, 0, 0),     -- ViewPending ? Facturas Pendientes (CanView)
(55, 'ProcessStamping', 'Billing', 6, 63, 0, 1, 0, 0), -- ProcessStamping ? Facturas Pendientes (CanCreate)
(56, 'ViewStamped', 'Billing', 6, 64, 1, 0, 0, 0);     -- ViewStamped ? Facturas Timbradas (CanView)

-- Mapeo de permisos Configuration ? Módulo Configuración (8)
INSERT INTO #PermissionMapping VALUES
(63, 'ManageGeneral', 'Configuration', 8, NULL, 1, 0, 0, 0),    -- ManageGeneral ? Módulo Configuración (CanView)
(64, 'ManageUsers', 'Configuration', 8, 81, 1, 1, 1, 1),        -- ManageUsers ? Usuarios (Todos)
(65, 'ManagePermissions', 'Configuration', 8, 83, 1, 1, 1, 1),  -- ManagePermissions ? Permisos (Todos)
(66, 'ManageCompany', 'Configuration', 8, 84, 1, 1, 1, 0),      -- ManageCompany ? Datos de Empresa (View, Create, Edit)
(67, 'ManageBranches', 'Configuration', 8, 85, 1, 1, 1, 1);     -- ManageBranches ? Sucursales (Todos)

-- Mapeo de permisos Reports ? Módulo Reportes (7)
INSERT INTO #PermissionMapping VALUES
(68, 'ViewSalesReport', 'Reports', 7, 71, 1, 0, 0, 0),      -- ViewSalesReport ? Reporte de Ventas (CanView)
(69, 'ViewInventoryReport', 'Reports', 7, 72, 1, 0, 0, 0),  -- ViewInventoryReport ? Reporte de Inventario (CanView)
(70, 'ViewProductReport', 'Reports', 7, 73, 1, 0, 0, 0),    -- ViewProductReport ? Reporte de Productos (CanView)
(71, 'ViewCustomerReport', 'Reports', 7, 74, 1, 0, 0, 0),   -- ViewCustomerReport ? Reporte de Clientes (CanView)
(72, 'ExportReports', 'Reports', 7, NULL, 1, 0, 0, 0);      -- ExportReports ? Módulo Reportes (CanView)

PRINT '? Tabla de mapeo creada con ' + CAST(@@ROWCOUNT AS VARCHAR) + ' registros';
PRINT '';

-- ============================================
-- PASO 2: MIGRAR PERMISOS DE ROLES
-- ============================================

PRINT '?? Paso 2: Migrando permisos de roles...';

-- Insertar permisos migrados en RoleModulePermissions
INSERT INTO RoleModulePermissions (
    RoleId,
    ModuleId,
    Name,
    Path,
    Icon,
    [Order],
    HasAccess,
    SubmoduleId,
    CanView,
    CanCreate,
    CanEdit,
    CanDelete,
    CreatedAt,
    CreatedByUserId
)
SELECT DISTINCT
    rp.RoleId,
    pm.ModuleId,
    m.Name,
    m.Path,
    m.Icon,
    m.[Order],
    1 AS HasAccess,
    pm.SubmoduleId,
    MAX(CAST(pm.CanView AS INT)) AS CanView,
    MAX(CAST(pm.CanCreate AS INT)) AS CanCreate,
    MAX(CAST(pm.CanEdit AS INT)) AS CanEdit,
    MAX(CAST(pm.CanDelete AS INT)) AS CanDelete,
    GETUTCDATE(),
    NULL
FROM RolePermissions rp
INNER JOIN #PermissionMapping pm ON pm.PermissionId = rp.PermissionId
INNER JOIN SystemModules m ON m.Id = pm.ModuleId
WHERE NOT EXISTS (
    -- Evitar duplicados
    SELECT 1
    FROM RoleModulePermissions rmp
    WHERE rmp.RoleId = rp.RoleId
      AND rmp.ModuleId = pm.ModuleId
      AND (rmp.SubmoduleId = pm.SubmoduleId OR (rmp.SubmoduleId IS NULL AND pm.SubmoduleId IS NULL))
)
GROUP BY 
    rp.RoleId,
    pm.ModuleId,
    pm.SubmoduleId,
    m.Name,
    m.Path,
    m.Icon,
    m.[Order];

DECLARE @MigratedCount INT = @@ROWCOUNT;
PRINT '? Permisos migrados: ' + CAST(@MigratedCount AS VARCHAR) + ' registros';
PRINT '';

-- ============================================
-- PASO 3: VERIFICAR MIGRACIÓN
-- ============================================

PRINT '?? Paso 3: Verificando migración...';
PRINT '';

-- Resumen por rol
SELECT 
    r.Id AS RoleId,
    r.Name AS RoleName,
    COUNT(DISTINCT rmp.ModuleId) AS TotalModulos,
    COUNT(rmp.Id) AS TotalPermisos,
    SUM(CASE WHEN rmp.CanView = 1 THEN 1 ELSE 0 END) AS PermisosVer,
    SUM(CASE WHEN rmp.CanCreate = 1 THEN 1 ELSE 0 END) AS PermisosCrear,
    SUM(CASE WHEN rmp.CanEdit = 1 THEN 1 ELSE 0 END) AS PermisosEditar,
    SUM(CASE WHEN rmp.CanDelete = 1 THEN 1 ELSE 0 END) AS PermisosEliminar
FROM Roles r
LEFT JOIN RoleModulePermissions rmp ON rmp.RoleId = r.Id
WHERE r.IsActive = 1
GROUP BY r.Id, r.Name
ORDER BY r.Id;

PRINT '';

-- ============================================
-- PASO 4: RESPALDO DE TABLAS ANTIGUAS
-- ============================================

PRINT '?? Paso 4: Creando respaldo de tablas antiguas...';

-- Respaldar RolePermissions
IF OBJECT_ID('RolePermissions_Backup') IS NOT NULL
    DROP TABLE RolePermissions_Backup;

SELECT * 
INTO RolePermissions_Backup
FROM RolePermissions;

PRINT '? Respaldo de RolePermissions creado: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' registros';

-- Respaldar Permissions
IF OBJECT_ID('Permissions_Backup') IS NOT NULL
    DROP TABLE Permissions_Backup;

SELECT * 
INTO Permissions_Backup
FROM Permissions;

PRINT '? Respaldo de Permissions creado: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' registros';
PRINT '';

-- ============================================
-- PASO 5: ELIMINAR FOREIGN KEYS Y TABLAS ANTIGUAS
-- ============================================

PRINT '???  Paso 5: Eliminando foreign keys y tablas antiguas...';

-- Eliminar foreign keys de RolePermissions
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RolePermissions_Roles_RoleId')
BEGIN
    ALTER TABLE RolePermissions DROP CONSTRAINT FK_RolePermissions_Roles_RoleId;
    PRINT '? Foreign key FK_RolePermissions_Roles_RoleId eliminada';
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RolePermissions_Permissions_PermissionId')
BEGIN
    ALTER TABLE RolePermissions DROP CONSTRAINT FK_RolePermissions_Permissions_PermissionId;
    PRINT '? Foreign key FK_RolePermissions_Permissions_PermissionId eliminada';
END

-- Eliminar tabla RolePermissions
IF OBJECT_ID('RolePermissions') IS NOT NULL
BEGIN
    DROP TABLE RolePermissions;
    PRINT '? Tabla RolePermissions eliminada';
END

-- Eliminar tabla Permissions
IF OBJECT_ID('Permissions') IS NOT NULL
BEGIN
    DROP TABLE Permissions;
    PRINT '? Tabla Permissions eliminada';
END

PRINT '';

-- ============================================
-- PASO 6: LIMPIAR TABLA TEMPORAL
-- ============================================

DROP TABLE #PermissionMapping;

PRINT '';
PRINT '? MIGRACIÓN COMPLETADA EXITOSAMENTE';
PRINT '====================================';
PRINT '';
PRINT '?? RESUMEN:';
PRINT '  - Permisos migrados a RoleModulePermissions: ' + CAST(@MigratedCount AS VARCHAR);
PRINT '  - Tablas eliminadas: RolePermissions, Permissions';
PRINT '  - Respaldos creados: RolePermissions_Backup, Permissions_Backup';
PRINT '';
PRINT '??  IMPORTANTE:';
PRINT '  - Los respaldos están en: RolePermissions_Backup, Permissions_Backup';
PRINT '  - Actualiza el código de .NET para usar solo RoleModulePermissions';
PRINT '  - Ejecuta la migración de EF Core para sincronizar el modelo';
PRINT '';
GO
