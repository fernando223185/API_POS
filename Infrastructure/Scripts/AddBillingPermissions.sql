-- Script para agregar los permisos faltantes del módulo CFDI/Billing

-- Insertar permisos faltantes para CFDI/Billing (ModuleId = 6)
INSERT INTO Permissions (Name, Resource, Description, ModuleId) VALUES
('ViewInvoices', 'Billing', 'Ver listado de facturas', 6),
('CreateInvoice', 'Billing', 'Crear nueva factura', 6),
('ViewPending', 'Billing', 'Ver facturas pendientes', 6),
('ProcessStamping', 'Billing', 'Procesar timbrado SAT', 6),
('ViewStamped', 'Billing', 'Ver facturas timbradas', 6),
('ManageCancellations', 'Billing', 'Gestionar cancelaciones', 6),
('ManageCompanies', 'Billing', 'Gestionar empresas emisoras', 6),
('ManageDownloads', 'Billing', 'Gestionar descargas', 6),
('ViewReports', 'Billing', 'Ver reportes de facturación', 6),
('ManageExport', 'Billing', 'Gestionar exportación', 6),
('ManageSettings', 'Billing', 'Gestionar configuración CFDI', 6);

-- Asignar todos los permisos de CFDI al Administrador (RoleId = 1)
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT 1, Id, GETUTCDATE() FROM Permissions 
WHERE Resource = 'Billing' AND ModuleId = 6;

-- Verificar que se agregaron correctamente
SELECT 'Permisos CFDI agregados' as Status, COUNT(*) as Total 
FROM Permissions WHERE Resource = 'Billing' AND ModuleId = 6;

PRINT 'Permisos de CFDI agregados correctamente';