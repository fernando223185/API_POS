-- ============================================
-- SCRIPT DE SEED DATA PARA MÓDULOS Y SUBMÓDULOS
-- Sistema ERP POS - Datos Iniciales
-- ============================================

USE ERP;
GO

-- ============================================
-- 1. INSERTAR MÓDULOS PRINCIPALES
-- ============================================

PRINT '?? Insertando módulos principales...';

-- Verificar si ya existen datos
IF NOT EXISTS (SELECT 1 FROM Modules WHERE Id = 1)
BEGIN
    INSERT INTO Modules (Id, Name, Description, Path, Icon, [Order], IsActive, CreatedAt)
    VALUES 
        (1, 'Inicio', 'Panel principal y dashboard del sistema', '/dashboard', 'faHome', 1, 1, GETUTCDATE()),
        (2, 'Ventas', 'Gestión de ventas, cotizaciones y devoluciones', '/sales', 'faShoppingCart', 2, 1, GETUTCDATE()),
        (3, 'Productos', 'Catálogo de productos, categorías y precios', '/products', 'faBoxOpen', 3, 1, GETUTCDATE()),
        (4, 'Inventario', 'Control de stock, kardex y movimientos', '/inventory', 'faWarehouse', 4, 1, GETUTCDATE()),
        (5, 'Clientes', 'Gestión de clientes y CRM', '/customers', 'faUsers', 5, 1, GETUTCDATE()),
        (6, 'CFDI', 'Facturación electrónica y timbrado SAT', '/billing', 'faFileInvoice', 6, 1, GETUTCDATE()),
        (7, 'Reportes', 'Reportes y análisis del sistema', '/reports', 'faChartBar', 7, 1, GETUTCDATE()),
        (8, 'Configuración', 'Configuración del sistema y administración', '/config', 'faCog', 8, 1, GETUTCDATE());
    
    PRINT '? Módulos principales insertados: 8 registros';
END
ELSE
BEGIN
    PRINT '??  Los módulos ya existen, omitiendo inserción';
END
GO

-- ============================================
-- 2. INSERTAR SUBMÓDULOS
-- ============================================

PRINT '?? Insertando submódulos...';

-- Verificar si ya existen datos
IF NOT EXISTS (SELECT 1 FROM Submodules WHERE Id = 21)
BEGIN
    -- ?? MÓDULO 2: VENTAS
    INSERT INTO Submodules (Id, ModuleId, Name, Description, Path, Icon, [Order], Color, IsActive, CreatedAt)
    VALUES 
        (21, 2, 'Nueva Venta', 'Crear ticket y capturar productos', '/sales/new', 'faPlus', 1, 'from-emerald-500 to-teal-600', 1, GETUTCDATE()),
        (22, 2, 'Historial de Ventas', 'Ventas recientes y filtros', '/sales/history', 'faHistory', 2, 'from-sky-500 to-blue-600', 1, GETUTCDATE()),
        (23, 2, 'Cotizaciones', 'Gestión de cotizaciones', '/sales/quotes', 'faFileAlt', 3, 'from-violet-500 to-purple-600', 1, GETUTCDATE()),
        (24, 2, 'Devoluciones', 'Procesamiento de devoluciones', '/sales/returns', 'faUndo', 4, 'from-amber-500 to-orange-600', 1, GETUTCDATE());

    -- ?? MÓDULO 3: PRODUCTOS
    INSERT INTO Submodules (Id, ModuleId, Name, Description, Path, Icon, [Order], Color, IsActive, CreatedAt)
    VALUES 
        (31, 3, 'Catálogo de Productos', 'Ver, buscar y gestionar productos', '/products/catalog', 'faList', 1, 'from-blue-500 to-indigo-600', 1, GETUTCDATE()),
        (32, 3, 'Nuevo Producto', 'Alta rápida con códigos y precios', '/products/new', 'faPlus', 2, 'from-emerald-500 to-teal-600', 1, GETUTCDATE()),
        (33, 3, 'Importar Productos', 'Importación masiva desde Excel/CSV', '/products/import', 'faFileUpload', 3, 'from-purple-500 to-violet-600', 1, GETUTCDATE()),
        (34, 3, 'Categorías', 'Organización del catálogo', '/products/categories', 'faTags', 4, 'from-cyan-500 to-blue-600', 1, GETUTCDATE());

    -- ?? MÓDULO 4: INVENTARIO
    INSERT INTO Submodules (Id, ModuleId, Name, Description, Path, Icon, [Order], Color, IsActive, CreatedAt)
    VALUES 
        (41, 4, 'Stock Actual', 'Inventario actual por almacén', '/inventory/stock', 'faBoxes', 1, 'from-sky-500 to-indigo-600', 1, GETUTCDATE()),
        (42, 4, 'Kardex', 'Movimientos de inventario', '/inventory/kardex', 'faClipboardList', 2, 'from-cyan-500 to-blue-600', 1, GETUTCDATE()),
        (43, 4, 'Alertas de Stock', 'Alertas de stock mínimo', '/inventory/alerts', 'faExclamationTriangle', 3, 'from-red-500 to-orange-500', 1, GETUTCDATE()),
        (44, 4, 'Movimientos', 'Entradas, salidas y ajustes', '/inventory/movements', 'faExchangeAlt', 4, 'from-amber-500 to-orange-600', 1, GETUTCDATE());

    -- ?? MÓDULO 5: CLIENTES
    INSERT INTO Submodules (Id, ModuleId, Name, Description, Path, Icon, [Order], Color, IsActive, CreatedAt)
    VALUES 
        (51, 5, 'Listado de Clientes', 'Buscar, filtrar y editar clientes', '/customers/list', 'faList', 1, 'from-sky-500 to-indigo-600', 1, GETUTCDATE()),
        (52, 5, 'Nuevo Cliente', 'Alta rápida con datos fiscales', '/customers/new', 'faUserPlus', 2, 'from-emerald-500 to-teal-600', 1, GETUTCDATE()),
        (53, 5, 'Importar Clientes', 'Importación masiva desde Excel/CSV', '/customers/import', 'faFileUpload', 3, 'from-purple-500 to-violet-600', 1, GETUTCDATE());

    -- ?? MÓDULO 6: CFDI
    INSERT INTO Submodules (Id, ModuleId, Name, Description, Path, Icon, [Order], Color, IsActive, CreatedAt)
    VALUES 
        (61, 6, 'Nueva Factura', 'Crear CFDI con múltiples empresas', '/billing/new', 'faPlus', 1, 'from-emerald-500 to-teal-600', 1, GETUTCDATE()),
        (62, 6, 'Facturas Emitidas', 'Listado de todas las facturas', '/billing/invoices', 'faFileInvoice', 2, 'from-sky-500 to-indigo-600', 1, GETUTCDATE()),
        (63, 6, 'Facturas Pendientes', 'Facturas por timbrar', '/billing/pending', 'faClock', 3, 'from-amber-500 to-orange-600', 1, GETUTCDATE()),
        (64, 6, 'Facturas Timbradas', 'Facturas completadas y timbradas', '/billing/stamped', 'faCheckCircle', 4, 'from-lime-500 to-green-600', 1, GETUTCDATE());

    -- ?? MÓDULO 7: REPORTES
    INSERT INTO Submodules (Id, ModuleId, Name, Description, Path, Icon, [Order], Color, IsActive, CreatedAt)
    VALUES 
        (71, 7, 'Reporte de Ventas', 'Análisis de ventas por período', '/reports/sales', 'faChartLine', 1, 'from-green-500 to-emerald-600', 1, GETUTCDATE()),
        (72, 7, 'Reporte de Inventario', 'Estado del inventario', '/reports/inventory', 'faBoxes', 2, 'from-sky-500 to-indigo-600', 1, GETUTCDATE()),
        (73, 7, 'Reporte de Productos', 'Rotación y análisis de productos', '/reports/products', 'faBoxOpen', 3, 'from-violet-500 to-purple-600', 1, GETUTCDATE()),
        (74, 7, 'Reporte de Clientes', 'Análisis de cartera de clientes', '/reports/customers', 'faUsers', 4, 'from-cyan-500 to-blue-600', 1, GETUTCDATE());

    -- ?? MÓDULO 8: CONFIGURACIÓN
    INSERT INTO Submodules (Id, ModuleId, Name, Description, Path, Icon, [Order], Color, IsActive, CreatedAt)
    VALUES 
        (81, 8, 'Usuarios', 'Gestión de usuarios del sistema', '/config/users', 'faUserCog', 1, 'from-emerald-500 to-teal-600', 1, GETUTCDATE()),
        (82, 8, 'Roles', 'Gestión de roles y perfiles', '/config/roles', 'faUserShield', 2, 'from-sky-500 to-indigo-600', 1, GETUTCDATE()),
        (83, 8, 'Permisos', 'Accesos por módulo/acción', '/config/permissions', 'faLock', 3, 'from-cyan-500 to-blue-600', 1, GETUTCDATE()),
        (84, 8, 'Datos de Empresa', 'Información de la empresa', '/config/company', 'faBuilding', 4, 'from-amber-500 to-orange-600', 1, GETUTCDATE()),
        (85, 8, 'Sucursales', 'Gestión de sucursales', '/config/branches', 'faMapMarkerAlt', 5, 'from-violet-500 to-purple-600', 1, GETUTCDATE()),
        (86, 8, 'Apariencia', 'Personalización del sistema', '/config/appearance', 'faPalette', 6, 'from-rose-500 to-pink-600', 1, GETUTCDATE());

    PRINT '? Submódulos insertados: 30 registros';
END
ELSE
BEGIN
    PRINT '??  Los submódulos ya existen, omitiendo inserción';
END
GO

-- ============================================
-- 3. VERIFICAR DATOS INSERTADOS
-- ============================================

PRINT '';
PRINT '?? RESUMEN DE DATOS INSERTADOS:';
PRINT '================================';

SELECT 
    COUNT(*) AS TotalModulos
FROM Modules
WHERE IsActive = 1;

SELECT 
    m.Id AS ModuloId,
    m.Name AS Modulo,
    COUNT(s.Id) AS TotalSubmodulos
FROM Modules m
LEFT JOIN Submodules s ON s.ModuleId = m.Id AND s.IsActive = 1
WHERE m.IsActive = 1
GROUP BY m.Id, m.Name
ORDER BY m.Id;

PRINT '';
PRINT '? Script de seed data ejecutado exitosamente';
PRINT '================================================';
GO
