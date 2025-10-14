-- Script para agregar los módulos faltantes y sus permisos

-- 1. Insertar módulos faltantes con IDENTITY_INSERT
SET IDENTITY_INSERT Modules ON;
INSERT INTO Modules (Id, Name, Description, IsActive) VALUES
(1, 'Dashboard', 'Panel principal', 1),
(2, 'Sales', 'Gestión de ventas', 1),  
(3, 'Products', 'Catálogo de productos', 1),
(4, 'Inventory', 'Control de inventario', 1);
SET IDENTITY_INSERT Modules OFF;

-- 2. Insertar permisos para Dashboard (ID 1)
INSERT INTO Permissions (Name, Resource, Description, ModuleId) VALUES
('Access', 'Dashboard', 'Acceso al dashboard', 1);

-- 3. Insertar permisos para Sales (ID 2)  
INSERT INTO Permissions (Name, Resource, Description, ModuleId) VALUES
('CreateSale', 'Sale', 'Nueva Venta', 2),
('ViewHistory', 'Sale', 'Ver Historial', 2),
('ViewProducts', 'Sale', 'Ver Productos para Venta', 2),
('ProcessPayment', 'Sale', 'Procesar Cobros', 2);

-- 4. Insertar permisos para Products (ID 3)
INSERT INTO Permissions (Name, Resource, Description, ModuleId) VALUES
('ViewCatalog', 'Product', 'Ver Catálogo', 3),
('Create', 'Product', 'Nuevo Producto', 3),
('AdvancedSearch', 'Product', 'Búsqueda Avanzada', 3),
('ViewStock', 'Product', 'Control de Stock', 3),
('ManageCategories', 'Product', 'Gestionar Categorías', 3),
('ManageBarcodes', 'Product', 'Códigos de Barras', 3),
('ManagePricing', 'Product', 'Listas de Precios', 3),
('ViewReports', 'Product', 'Ver Reportes', 3),
('ManageAlerts', 'Product', 'Stock Mínimo', 3),
('ManageSuppliers', 'Product', 'Gestionar Proveedores', 3),
('ManageOrders', 'Product', 'Órdenes de Compra', 3);

-- 5. Insertar permisos para Inventory (ID 4)
INSERT INTO Permissions (Name, Resource, Description, ModuleId) VALUES
('ViewStock', 'Inventory', 'Ver Existencias', 4),
('ViewKardex', 'Inventory', 'Kardex / Movimientos', 4),
('ManageInbound', 'Inventory', 'Gestionar Entradas', 4),
('ManageOutbound', 'Inventory', 'Gestionar Salidas', 4),
('MakeAdjustments', 'Inventory', 'Hacer Ajustes', 4),
('ManageTransfers', 'Inventory', 'Gestionar Traspasos', 4),
('ManageWarehouses', 'Inventory', 'Gestionar Almacenes', 4),
('ManageCounts', 'Inventory', 'Conteos / Ciclos', 4),
('ManageLabels', 'Inventory', 'Etiquetas / Códigos', 4),
('ViewAlerts', 'Inventory', 'Alertas de Stock', 4),
('ManageReceipts', 'Inventory', 'Gestionar Recepciones', 4);

-- 6. Insertar permisos del Administrador para los nuevos módulos
-- Dashboard
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt) 
SELECT 1, Id, GETUTCDATE() FROM Permissions WHERE Resource = 'Dashboard';

-- Sales  
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT 1, Id, GETUTCDATE() FROM Permissions WHERE Resource = 'Sale';

-- Products
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT 1, Id, GETUTCDATE() FROM Permissions WHERE Resource = 'Product';

-- Inventory
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT 1, Id, GETUTCDATE() FROM Permissions WHERE Resource = 'Inventory';

-- 7. Agregar algunos permisos ejemplo para el rol Vendedor (RoleId = 3)
-- Dashboard + Sales + Products (solo lectura)
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT 3, Id, GETUTCDATE() FROM Permissions 
WHERE Resource IN ('Dashboard') 
   OR (Resource = 'Sale')
   OR (Resource = 'Product' AND Name IN ('ViewCatalog', 'AdvancedSearch', 'ViewStock'));

-- 8. Agregar permisos para el rol Cajero (RoleId = 10) 
-- Dashboard + Sales (Nueva Venta y Procesar Cobros) + Products (Ver Catálogo)
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT 10, Id, GETUTCDATE() FROM Permissions 
WHERE Resource = 'Dashboard'
   OR (Resource = 'Sale' AND Name IN ('CreateSale', 'ProcessPayment'))
   OR (Resource = 'Product' AND Name = 'ViewCatalog');

PRINT 'Módulos y permisos faltantes agregados correctamente';
PRINT 'Total módulos: 7';
PRINT 'Nuevos permisos agregados: 28';

-- Verificar resultados
SELECT 'Módulos' as Tabla, COUNT(*) as Total FROM Modules WHERE IsActive = 1
UNION ALL
SELECT 'Permisos' as Tabla, COUNT(*) as Total FROM Permissions
UNION ALL  
SELECT 'RolePermissions' as Tabla, COUNT(*) as Total FROM RolePermissions;