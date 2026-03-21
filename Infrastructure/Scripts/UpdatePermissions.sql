-- Script para actualizar módulos y permisos del sistema

-- 1. Deshabilitar restricciones de clave foránea temporalmente
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"

-- 2. Limpiar datos existentes
DELETE FROM RolePermissions;
DELETE FROM Permissions;
DELETE FROM Modules WHERE Id > 0;
DELETE FROM Roles WHERE Id > 1; -- Mantener solo Admin

-- 3. Reiniciar IDENTITY
DBCC CHECKIDENT ('Modules', RESEED, 0);
DBCC CHECKIDENT ('Permissions', RESEED, 0);
DBCC CHECKIDENT ('RolePermissions', RESEED, 0);

-- 4. Insertar nuevos módulos
INSERT INTO Modules (Name, Description, IsActive) VALUES
('Dashboard', 'Panel principal', 1),
('Sales', 'Gestión de ventas', 1),
('Products', 'Catálogo de productos', 1),
('Inventory', 'Control de inventario', 1),
('Customers', 'Gestión de clientes', 1),
('Billing', 'CFDI y facturación', 1),
('Configuration', 'Configuración del sistema', 1);

-- 5. Insertar nuevos roles
INSERT INTO Roles (Name, Description, IsActive) VALUES
('Usuario', 'Acceso básico al sistema', 1),
('Vendedor', 'Personal de ventas', 1),
('Almacenista', 'Gestión de inventario', 1),
('Cajero', 'Operación de punto de venta', 1),
('Gerente', 'Supervisión y reportes', 1);

-- 6. Insertar permisos detallados
INSERT INTO Permissions (Name, Resource, Description, ModuleId) VALUES
-- Dashboard (1)
('Access', 'Dashboard', 'Acceso al dashboard', 1),

-- Sales (2-5)
('CreateSale', 'Sale', 'Nueva Venta', 2),
('ViewHistory', 'Sale', 'Ver Historial', 2),
('ViewProducts', 'Sale', 'Ver Productos para Venta', 2),
('ProcessPayment', 'Sale', 'Procesar Cobros', 2),

-- Products (6-16)
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
('ManageOrders', 'Product', 'Órdenes de Compra', 3),

-- Inventory (17-27)
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
('ManageReceipts', 'Inventory', 'Gestionar Recepciones', 4),

-- Customers (28-38)
('ViewList', 'Customer', 'Ver listado', 5),
('Create', 'Customer', 'Nuevo Cliente', 5),
('ManageRequests', 'Customer', 'Gestionar Solicitudes', 5),
('ManageCredit', 'Customer', 'Crédito y Límites', 5),
('ManageContacts', 'Customer', 'Gestionar Contactos', 5),
('ManageAddresses', 'Customer', 'Gestionar Direcciones', 5),
('ManageIdentification', 'Customer', 'Gestionar Identificación', 5),
('ManagePricing', 'Customer', 'Precios y Descuentos', 5),
('ManageDocuments', 'Customer', 'CFDI / Facturas', 5),
('ViewLedger', 'Customer', 'Estado de Cuenta', 5),
('ManageOverpayments', 'Customer', 'Saldos a Favor', 5),

-- Billing (39)
('Access', 'Billing', 'Acceso a CFDI', 6),

-- Configuration (40-54)
('ManageGeneral', 'Configuration', 'Configuración General', 7),
('ManageUsers', 'Configuration', 'Usuarios & Roles', 7),
('ManagePermissions', 'Configuration', 'Gestionar Permisos', 7),
('ManageBranches', 'Configuration', 'Gestionar Sucursales', 7),
('ManageWarehouses', 'Configuration', 'Gestionar Almacenes', 7),
('ManageShipping', 'Configuration', 'Gestionar Paqueterías', 7),
('ManagePricing', 'Configuration', 'Listas de Precios', 7),
('ManagePayments', 'Configuration', 'Métodos de Pago', 7),
('ManageCFDI', 'Configuration', 'CFDI / Facturación', 7),
('ManagePOS', 'Configuration', 'Configurar POS', 7),
('ManageIntegrations', 'Configuration', 'Gestionar Integraciones', 7),
('ManageNotifications', 'Configuration', 'Gestionar Notificaciones', 7),
('ManageTemplates', 'Configuration', 'Gestionar Plantillas', 7),
('ViewAudit', 'Configuration', 'Ver Auditoría', 7),
('ManageBackups', 'Configuration', 'Gestionar Respaldos', 7);

-- 7. Permisos para Administrador (todos)
DECLARE @i INT = 1;
WHILE @i <= 54
BEGIN
    INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt) VALUES (1, @i, GETUTCDATE());
    SET @i = @i + 1;
END

-- 8. Permisos para Vendedor (ID 3)
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt) VALUES
(3, 1, GETUTCDATE()),  -- Dashboard
(3, 2, GETUTCDATE()),  -- Nueva Venta
(3, 3, GETUTCDATE()),  -- Ver Historial
(3, 4, GETUTCDATE()),  -- Ver Productos
(3, 5, GETUTCDATE()),  -- Procesar Cobros
(3, 6, GETUTCDATE()),  -- Ver Catálogo
(3, 8, GETUTCDATE()),  -- Búsqueda Avanzada
(3, 9, GETUTCDATE()),  -- Ver Stock
(3, 28, GETUTCDATE()), -- Ver listado clientes
(3, 29, GETUTCDATE()), -- Nuevo Cliente
(3, 32, GETUTCDATE()), -- Gestionar Contactos
(3, 33, GETUTCDATE()); -- Gestionar Direcciones

-- 9. Permisos para Cajero (ID 5)
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt) VALUES
(5, 1, GETUTCDATE()),  -- Dashboard
(5, 2, GETUTCDATE()),  -- Nueva Venta
(5, 4, GETUTCDATE()),  -- Ver Productos
(5, 5, GETUTCDATE()),  -- Procesar Cobros
(5, 6, GETUTCDATE());  -- Ver Catálogo

-- 10. Rehabilitar restricciones de clave foránea
EXEC sp_MSforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all"

PRINT 'Actualización completada exitosamente';