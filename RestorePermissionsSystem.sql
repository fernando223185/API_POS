-- =============================================
-- Script: Restaurar Sistema de Permisos Completo ERP
-- Descripción: Re-insertar todos los módulos, permisos y asignaciones de roles
-- =============================================

USE [ERP]
GO

-- =============================================
-- 1. LIMPIAR DATOS EXISTENTES (en orden correcto para evitar FK violations)
-- =============================================
DELETE FROM [RolePermissions];
DELETE FROM [Permissions];
DELETE FROM [Modules];

-- =============================================
-- 2. INSERTAR MÓDULOS
-- =============================================
SET IDENTITY_INSERT [Modules] ON;

INSERT INTO [Modules] ([Id], [Name], [Description], [IsActive])
VALUES 
    (1, 'CRM', 'Customer Relationship Management', 1),
    (2, 'Sales', 'Sales Management', 1),
    (3, 'Products', 'Product Management', 1),
    (4, 'Users', 'User Management', 1),
    (5, 'Inventory', 'Inventory Management', 1),
    (6, 'Billing', 'CFDI and Billing Management', 1),
    (7, 'Configuration', 'System Configuration', 1);

SET IDENTITY_INSERT [Modules] OFF;

-- =============================================
-- 3. INSERTAR PERMISOS COMPLETOS
-- =============================================
SET IDENTITY_INSERT [Permissions] ON;

INSERT INTO [Permissions] ([Id], [Name], [Resource], [Description], [ModuleId])
VALUES 
    -- ? CRM Module (1) - Customer Permissions
    (1, 'Create', 'Customer', 'Crear clientes', 1),
    (2, 'Read', 'Customer', 'Ver clientes', 1),
    (3, 'Update', 'Customer', 'Actualizar clientes', 1),
    (4, 'Delete', 'Customer', 'Eliminar clientes', 1),
    (5, 'ViewList', 'Customer', 'Ver listado de clientes', 1),
    (6, 'ManageRequests', 'Customer', 'Gestionar solicitudes de clientes', 1),
    (7, 'ManageCredit', 'Customer', 'Gestionar crédito y límites', 1),
    (8, 'ManageContacts', 'Customer', 'Gestionar contactos', 1),
    (9, 'ManageAddresses', 'Customer', 'Gestionar direcciones', 1),
    (10, 'ManageIdentification', 'Customer', 'Gestionar identificación fiscal', 1),
    (11, 'ManagePricing', 'Customer', 'Gestionar precios y descuentos', 1),
    (12, 'ManageDocuments', 'Customer', 'Gestionar documentos CFDI', 1),
    (13, 'ViewLedger', 'Customer', 'Ver estado de cuenta', 1),
    (14, 'ManageOverpayments', 'Customer', 'Gestionar saldos a favor', 1),

    -- ? Sales Module (2) - Sale Permissions
    (15, 'Create', 'Sale', 'Crear ventas', 2),
    (16, 'Read', 'Sale', 'Ver ventas', 2),
    (17, 'Update', 'Sale', 'Actualizar ventas', 2),
    (18, 'Delete', 'Sale', 'Eliminar ventas', 2),
    (19, 'CreateSale', 'Sale', 'Nueva venta POS', 2),
    (20, 'ViewHistory', 'Sale', 'Ver historial de ventas', 2),
    (21, 'ViewProducts', 'Sale', 'Ver productos en venta', 2),
    (22, 'ProcessPayment', 'Sale', 'Procesar pagos', 2),

    -- ? Products Module (3) - Product Permissions
    (23, 'Create', 'Product', 'Crear productos', 3),
    (24, 'Read', 'Product', 'Ver productos', 3),
    (25, 'Update', 'Product', 'Actualizar productos', 3),
    (26, 'Delete', 'Product', 'Eliminar productos', 3),
    (27, 'ViewCatalog', 'Product', 'Ver catálogo de productos', 3),
    (28, 'AdvancedSearch', 'Product', 'Búsqueda avanzada de productos', 3),
    (29, 'ViewStock', 'Product', 'Ver control de stock', 3),
    (30, 'ManageCategories', 'Product', 'Gestionar categorías', 3),
    (31, 'ManageBarcodes', 'Product', 'Gestionar códigos de barras', 3),
    (32, 'ManagePricing', 'Product', 'Gestionar listas de precios', 3),
    (33, 'ViewReports', 'Product', 'Ver reportes de productos', 3),
    (34, 'ManageAlerts', 'Product', 'Gestionar alertas de stock', 3),
    (35, 'ManageSuppliers', 'Product', 'Gestionar proveedores', 3),
    (36, 'ManageOrders', 'Product', 'Gestionar órdenes de compra', 3),

    -- ? Users Module (4) - User Permissions
    (37, 'Create', 'User', 'Crear usuarios', 4),
    (38, 'Read', 'User', 'Ver usuarios', 4),
    (39, 'Update', 'User', 'Actualizar usuarios', 4),
    (40, 'Delete', 'User', 'Eliminar usuarios', 4),

    -- ? Inventory Module (5) - Inventory Permissions
    (41, 'ViewStock', 'Inventory', 'Ver existencias', 5),
    (42, 'ViewKardex', 'Inventory', 'Ver kardex y movimientos', 5),
    (43, 'ManageInbound', 'Inventory', 'Gestionar entradas', 5),
    (44, 'ManageOutbound', 'Inventory', 'Gestionar salidas', 5),
    (45, 'MakeAdjustments', 'Inventory', 'Realizar ajustes', 5),
    (46, 'ManageTransfers', 'Inventory', 'Gestionar traspasos', 5),
    (47, 'ManageWarehouses', 'Inventory', 'Gestionar almacenes', 5),
    (48, 'ManageCounts', 'Inventory', 'Gestionar conteos', 5),
    (49, 'ManageLabels', 'Inventory', 'Gestionar etiquetas', 5),
    (50, 'ViewAlerts', 'Inventory', 'Ver alertas de stock', 5),
    (51, 'ManageReceipts', 'Inventory', 'Gestionar recepciones', 5),

    -- ? Billing Module (6) - CFDI Permissions
    (52, 'ViewInvoices', 'Billing', 'Ver facturas', 6),
    (53, 'CreateInvoice', 'Billing', 'Crear facturas', 6),
    (54, 'ViewPending', 'Billing', 'Ver facturas pendientes', 6),
    (55, 'ProcessStamping', 'Billing', 'Procesar timbrado', 6),
    (56, 'ViewStamped', 'Billing', 'Ver facturas timbradas', 6),
    (57, 'ManageCancellations', 'Billing', 'Gestionar cancelaciones', 6),
    (58, 'ManageCompanies', 'Billing', 'Gestionar empresas emisoras', 6),
    (59, 'ManageDownloads', 'Billing', 'Gestionar descargas', 6),
    (60, 'ViewReports', 'Billing', 'Ver reportes de facturación', 6),
    (61, 'ManageExport', 'Billing', 'Gestionar exportación', 6),
    (62, 'ManageSettings', 'Billing', 'Gestionar configuración CFDI', 6),

    -- ? Configuration Module (7) - Configuration Permissions
    (63, 'ManageGeneral', 'Configuration', 'Gestionar configuración general', 7),
    (64, 'ManageUsers', 'Configuration', 'Gestionar usuarios y roles', 7),
    (65, 'ManagePermissions', 'Configuration', 'Gestionar permisos', 7),
    (66, 'ManageBranches', 'Configuration', 'Gestionar sucursales', 7),
    (67, 'ManageWarehouses', 'Configuration', 'Gestionar almacenes', 7),
    (68, 'ManageShipping', 'Configuration', 'Gestionar paqueterías', 7),
    (69, 'ManagePricing', 'Configuration', 'Gestionar listas de precios', 7),
    (70, 'ManagePayments', 'Configuration', 'Gestionar métodos de pago', 7),
    (71, 'ManageCFDI', 'Configuration', 'Gestionar configuración CFDI', 7),
    (72, 'ManagePOS', 'Configuration', 'Gestionar configuración POS', 7),
    (73, 'ManageIntegrations', 'Configuration', 'Gestionar integraciones', 7),
    (74, 'ManageNotifications', 'Configuration', 'Gestionar notificaciones', 7),
    (75, 'ManageTemplates', 'Configuration', 'Gestionar plantillas', 7),
    (76, 'ViewAudit', 'Configuration', 'Ver auditoría', 7),
    (77, 'ManageBackups', 'Configuration', 'Gestionar respaldos', 7);

SET IDENTITY_INSERT [Permissions] OFF;

-- =============================================
-- 4. ASIGNAR TODOS LOS PERMISOS AL ROL ADMINISTRADOR (RoleId = 1)
-- =============================================
SET IDENTITY_INSERT [RolePermissions] ON;

DECLARE @PermissionId INT = 1;
DECLARE @RolePermissionId INT = 1;

WHILE @PermissionId <= 77
BEGIN
    INSERT INTO [RolePermissions] ([Id], [RoleId], [PermissionId], [CreatedAt])
    VALUES (@RolePermissionId, 1, @PermissionId, GETUTCDATE());
    
    SET @PermissionId = @PermissionId + 1;
    SET @RolePermissionId = @RolePermissionId + 1;
END

SET IDENTITY_INSERT [RolePermissions] OFF;

-- =============================================
-- 5. ASIGNAR PERMISOS BÁSICOS A OTROS ROLES
-- =============================================

-- Rol Usuario (2) - Permisos básicos de lectura
INSERT INTO [RolePermissions] ([RoleId], [PermissionId], [CreatedAt])
VALUES 
    (2, 2, GETUTCDATE()),   -- Customer Read
    (2, 5, GETUTCDATE()),   -- Customer ViewList
    (2, 16, GETUTCDATE()),  -- Sale Read
    (2, 24, GETUTCDATE()),  -- Product Read
    (2, 27, GETUTCDATE());  -- Product ViewCatalog

-- Rol Vendedor (3) - Permisos de ventas y clientes
INSERT INTO [RolePermissions] ([RoleId], [PermissionId], [CreatedAt])
VALUES 
    (3, 1, GETUTCDATE()),   -- Customer Create
    (3, 2, GETUTCDATE()),   -- Customer Read
    (3, 3, GETUTCDATE()),   -- Customer Update
    (3, 5, GETUTCDATE()),   -- Customer ViewList
    (3, 15, GETUTCDATE()),  -- Sale Create
    (3, 16, GETUTCDATE()),  -- Sale Read
    (3, 19, GETUTCDATE()),  -- Sale CreateSale
    (3, 20, GETUTCDATE()),  -- Sale ViewHistory
    (3, 21, GETUTCDATE()),  -- Sale ViewProducts
    (3, 22, GETUTCDATE()),  -- Sale ProcessPayment
    (3, 24, GETUTCDATE()),  -- Product Read
    (3, 27, GETUTCDATE());  -- Product ViewCatalog

-- Rol Almacenista (4) - Permisos de productos e inventario
INSERT INTO [RolePermissions] ([RoleId], [PermissionId], [CreatedAt])
VALUES 
    (4, 23, GETUTCDATE()),  -- Product Create
    (4, 24, GETUTCDATE()),  -- Product Read
    (4, 25, GETUTCDATE()),  -- Product Update
    (4, 27, GETUTCDATE()),  -- Product ViewCatalog
    (4, 29, GETUTCDATE()),  -- Product ViewStock
    (4, 41, GETUTCDATE()),  -- Inventory ViewStock
    (4, 42, GETUTCDATE()),  -- Inventory ViewKardex
    (4, 43, GETUTCDATE()),  -- Inventory ManageInbound
    (4, 44, GETUTCDATE()),  -- Inventory ManageOutbound
    (4, 45, GETUTCDATE());  -- Inventory MakeAdjustments

PRINT '? Sistema de permisos restaurado exitosamente:';
PRINT '   - 7 Módulos creados';
PRINT '   - 77 Permisos creados';
PRINT '   - Administrador: Todos los permisos';
PRINT '   - Usuario: Permisos básicos';
PRINT '   - Vendedor: Permisos de ventas y clientes';
PRINT '   - Almacenista: Permisos de productos e inventario';

-- =============================================
-- 6. VERIFICAR LOS DATOS INSERTADOS
-- =============================================
SELECT 'MÓDULOS' as Tabla, COUNT(*) as Total FROM [Modules]
UNION ALL
SELECT 'PERMISOS' as Tabla, COUNT(*) as Total FROM [Permissions]  
UNION ALL
SELECT 'ROLE_PERMISOS' as Tabla, COUNT(*) as Total FROM [RolePermissions];

GO