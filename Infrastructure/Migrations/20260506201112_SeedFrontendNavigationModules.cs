using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedFrontendNavigationModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @Now datetime2 = SYSUTCDATETIME();

DECLARE @Modules TABLE
(
    Id int NOT NULL PRIMARY KEY,
    Name nvarchar(100) NOT NULL,
    Description nvarchar(500) NULL,
    Path nvarchar(200) NOT NULL,
    Icon nvarchar(50) NOT NULL,
    [Order] int NOT NULL
);

INSERT INTO @Modules (Id, Name, Description, Path, Icon, [Order])
VALUES
(1, N'Inicio', N'Panel principal del sistema', N'/dashboard', N'faHome', 1),
(2, N'Ventas', N'Ventas POS, delivery, caja y cotizaciones', N'/sales', N'faShoppingCart', 2),
(3, N'Productos', N'Catalogo, precios y control de productos', N'/products', N'faBoxOpen', 3),
(4, N'Inventario', N'Existencias, movimientos, compras y traspasos', N'/inventory', N'faWarehouse', 4),
(5, N'Clientes', N'Clientes, credito, documentos y estado de cuenta', N'/customers', N'faUsers', 5),
(6, N'CFDI', N'Facturacion, cuentas por cobrar y complementos de pago', N'/billing', N'faFileInvoice', 6),
(7, N'Reportes', N'Reportes operativos y gerenciales', N'/reports', N'faChartBar', 7),
(8, N'Configuración', N'Configuracion general del sistema', N'/config', N'faCog', 8);

MERGE dbo.Modules AS target
USING @Modules AS source
ON target.Id = source.Id
WHEN MATCHED THEN
    UPDATE SET
        Name = source.Name,
        Description = source.Description,
        Path = source.Path,
        Icon = source.Icon,
        [Order] = source.[Order],
        IsActive = 1,
        UpdatedAt = @Now
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, Name, Description, Path, Icon, [Order], IsActive, CreatedAt)
    VALUES (source.Id, source.Name, source.Description, source.Path, source.Icon, source.[Order], 1, @Now);

DECLARE @Submodules TABLE
(
    Id int NOT NULL PRIMARY KEY,
    ModuleId int NOT NULL,
    Name nvarchar(100) NOT NULL,
    Description nvarchar(500) NULL,
    Path nvarchar(200) NOT NULL,
    Icon nvarchar(50) NOT NULL,
    [Order] int NOT NULL,
    Color nvarchar(100) NULL
);

INSERT INTO @Submodules (Id, ModuleId, Name, Description, Path, Icon, [Order], Color)
VALUES
(201, 2, N'Nueva Venta POS', N'Crear una nueva venta POS', N'/sales/new', N'faPlus', 1, N'from-emerald-500 to-teal-600'),
(202, 2, N'Historial', N'Historial de ventas', N'/sales/history', N'faHistory', 2, N'from-sky-500 to-blue-600'),
(203, 2, N'Productos', N'Productos para ventas', N'/sales/products', N'faBoxOpen', 3, N'from-indigo-500 to-blue-600'),
(204, 2, N'Cobrar POS', N'Cobro de ventas POS', N'/sales/checkout', N'faCashRegister', 4, N'from-green-500 to-emerald-600'),
(205, 2, N'Caja', N'Operaciones de caja', N'/sales/cashier', N'faCashRegister', 5, N'from-amber-500 to-orange-600'),
(206, 2, N'Ventas Delivery', N'Ventas con entrega a domicilio', N'/sales/delivery', N'faTruck', 6, N'from-cyan-500 to-blue-600'),
(207, 2, N'Cotizaciones', N'Gestion de cotizaciones', N'/quotations', N'faFileInvoiceDollar', 7, N'from-violet-500 to-purple-600'),
(208, 2, N'Cortes de Caja', N'Cortes y turnos de caja', N'/sales/cashier-shifts', N'faReceipt', 8, N'from-slate-500 to-gray-600'),

(301, 3, N'Catálogo', N'Catalogo de productos', N'/products/catalog', N'faList', 1, N'from-blue-500 to-indigo-600'),
(302, 3, N'Nuevo Producto', N'Alta de producto', N'/products/new', N'faPlus', 2, N'from-emerald-500 to-teal-600'),
(303, 3, N'Búsqueda Avanzada', N'Busqueda avanzada de productos', N'/products/search', N'faSearch', 3, N'from-cyan-500 to-blue-600'),
(304, 3, N'Control de Stock', N'Control de stock por producto', N'/products/inventory', N'faBoxes', 4, N'from-amber-500 to-orange-600'),
(305, 3, N'Categorías', N'Categorias de productos', N'/products/categories', N'faTags', 5, N'from-pink-500 to-rose-600'),
(306, 3, N'Códigos de Barras', N'Codigos de barras de productos', N'/products/barcodes', N'faBarcode', 6, N'from-slate-500 to-gray-600'),
(307, 3, N'Listas de Precios', N'Listas de precios', N'/products/pricing', N'faMoneyBillWave', 7, N'from-green-500 to-emerald-600'),
(308, 3, N'Reportes', N'Reportes de productos', N'/products/reports', N'faChartBar', 8, N'from-indigo-500 to-violet-600'),
(309, 3, N'Stock Mínimo', N'Alertas de stock minimo', N'/products/alerts', N'faBell', 9, N'from-red-500 to-orange-600'),

(401, 4, N'Existencias', N'Existencias por almacen', N'/inventory/stock', N'faBoxes', 1, N'from-sky-500 to-indigo-600'),
(402, 4, N'Kardex / Movs.', N'Kardex y movimientos de inventario', N'/inventory/kardex', N'faClipboardList', 2, N'from-cyan-500 to-blue-600'),
(403, 4, N'Ajustes', N'Ajustes de inventario', N'/inventory/adjustments', N'faSlidersH', 3, N'from-amber-500 to-orange-600'),
(404, 4, N'Traspasos', N'Traspasos de inventario', N'/inventory/transfers', N'faExchangeAlt', 4, N'from-violet-500 to-purple-600'),
(405, 4, N'Salidas de Almacén', N'Salidas de almacen por traspaso', N'/inventory/salidas-almacen', N'faDolly', 5, N'from-red-500 to-orange-600'),
(406, 4, N'Entradas por Traspaso', N'Entradas por traspaso', N'/inventory/entradas-almacen', N'faInbox', 6, N'from-green-500 to-emerald-600'),
(407, 4, N'Conteos / Ciclos', N'Conteos ciclicos de inventario', N'/inventory/counts', N'faTasks', 7, N'from-blue-500 to-indigo-600'),
(408, 4, N'Etiquetas / Códigos', N'Etiquetas y codigos de inventario', N'/inventory/labels', N'faBarcode', 8, N'from-slate-500 to-gray-600'),
(409, 4, N'Alertas de Stock', N'Alertas de stock', N'/inventory/alerts', N'faBell', 9, N'from-red-500 to-orange-600'),
(410, 4, N'Órdenes de Compra', N'Ordenes de compra', N'/inventory/ordenes', N'faShoppingBag', 10, N'from-emerald-500 to-teal-600'),
(411, 4, N'Recepciones', N'Recepciones de compra', N'/inventory/recepciones', N'faTruckLoading', 11, N'from-cyan-500 to-blue-600'),
(412, 4, N'Proveedores', N'Proveedores', N'/inventory/proveedores', N'faHandshake', 12, N'from-indigo-500 to-blue-600'),

(501, 5, N'Listado', N'Listado de clientes', N'/customers/list', N'faList', 1, N'from-sky-500 to-indigo-600'),
(502, 5, N'Nuevo Cliente', N'Alta de cliente', N'/customers/new', N'faUserPlus', 2, N'from-emerald-500 to-teal-600'),
(503, 5, N'Solicitudes', N'Solicitudes de clientes', N'/customers/requests', N'faInbox', 3, N'from-cyan-500 to-blue-600'),
(504, 5, N'Crédito y Límites', N'Credito y limites de clientes', N'/customers/credit', N'faCreditCard', 4, N'from-amber-500 to-orange-600'),
(505, 5, N'Contactos', N'Contactos de clientes', N'/customers/contacts', N'faAddressBook', 5, N'from-indigo-500 to-blue-600'),
(506, 5, N'Direcciones', N'Direcciones de clientes', N'/customers/addresses', N'faMapMarkerAlt', 6, N'from-green-500 to-emerald-600'),
(507, 5, N'Identificación', N'Identificacion de clientes', N'/customers/identity', N'faIdCard', 7, N'from-slate-500 to-gray-600'),
(508, 5, N'Precios y Descuentos', N'Precios y descuentos por cliente', N'/customers/pricing', N'faPercent', 8, N'from-pink-500 to-rose-600'),
(509, 5, N'CFDI / Facturas', N'Documentos CFDI de clientes', N'/customers/documents', N'faFileInvoice', 9, N'from-violet-500 to-purple-600'),
(510, 5, N'Estado de Cuenta', N'Estado de cuenta de clientes', N'/customers/ledger', N'faBook', 10, N'from-blue-500 to-indigo-600'),
(511, 5, N'Saldos a Favor', N'Saldos a favor de clientes', N'/customers/overpayments', N'faWallet', 11, N'from-emerald-500 to-teal-600'),

(601, 6, N'Listado', N'Listado de facturas CFDI', N'/billing/invoices', N'faFileInvoice', 1, N'from-sky-500 to-indigo-600'),
(602, 6, N'Nueva Factura', N'Crear factura CFDI', N'/billing/new', N'faPlus', 2, N'from-emerald-500 to-teal-600'),
(603, 6, N'Ventas Pendientes', N'Ventas pendientes de facturar', N'/billing/pending-sales', N'faClock', 3, N'from-amber-500 to-orange-600'),
(604, 6, N'CxC - Complementos de Pago', N'Complementos de pago PPD', N'/billing/invoices-ppd', N'faReceipt', 4, N'from-green-500 to-emerald-600'),
(605, 6, N'CxC - Reportes', N'Reportes de cuentas por cobrar', N'/billing/reports', N'faChartBar', 5, N'from-indigo-500 to-violet-600'),
(606, 6, N'Empresas y Configuración', N'Empresas y configuracion CFDI', N'/billing/companies', N'faBuilding', 6, N'from-slate-500 to-gray-600'),
(607, 6, N'Descargas', N'Descargas CFDI', N'/billing/downloads', N'faDownload', 7, N'from-cyan-500 to-blue-600'),
(608, 6, N'Exportación', N'Exportacion CFDI', N'/billing/export', N'faFileExport', 8, N'from-blue-500 to-indigo-600'),

(801, 8, N'General', N'Configuracion general', N'/config/general', N'faCog', 1, N'from-slate-500 to-gray-600'),
(802, 8, N'Empresas', N'Gestion de empresas', N'/config/companies', N'faBuilding', 2, N'from-blue-500 to-indigo-600'),
(803, 8, N'Módulos del Sistema', N'Gestion de modulos del sistema', N'/config/modules', N'faCubes', 3, N'from-violet-500 to-purple-600'),
(804, 8, N'Usuarios & Roles', N'Usuarios y roles', N'/config/users', N'faUserCog', 4, N'from-emerald-500 to-teal-600'),
(805, 8, N'Permisos', N'Permisos del sistema', N'/config/permissions', N'faUserShield', 5, N'from-sky-500 to-indigo-600'),
(806, 8, N'Sucursales', N'Gestion de sucursales', N'/config/branches', N'faStore', 6, N'from-cyan-500 to-blue-600'),
(807, 8, N'Almacenes', N'Gestion de almacenes', N'/config/warehouses', N'faWarehouse', 7, N'from-amber-500 to-orange-600'),
(808, 8, N'Paqueterías', N'Gestion de paqueterias', N'/config/shipping-carriers', N'faTruck', 8, N'from-green-500 to-emerald-600'),
(809, 8, N'Listas de Precios', N'Listas de precios', N'/config/pricing', N'faMoneyBillWave', 9, N'from-indigo-500 to-blue-600'),
(810, 8, N'Métodos de Pago', N'Metodos de pago', N'/config/payments', N'faCreditCard', 10, N'from-pink-500 to-rose-600'),
(811, 8, N'CFDI / Facturación', N'Configuracion CFDI', N'/config/cfdi', N'faFileInvoice', 11, N'from-violet-500 to-purple-600'),
(812, 8, N'POS', N'Configuracion POS', N'/config/pos', N'faCashRegister', 12, N'from-emerald-500 to-teal-600'),
(813, 8, N'Integraciones', N'Integraciones del sistema', N'/config/integrations', N'faPlug', 13, N'from-cyan-500 to-blue-600'),
(814, 8, N'Notificaciones', N'Configuracion de notificaciones', N'/config/notifications', N'faBell', 14, N'from-amber-500 to-orange-600'),
(815, 8, N'Plantillas de Reportes', N'Plantillas de reportes', N'/config/templates', N'faFileAlt', 15, N'from-blue-500 to-indigo-600'),
(816, 8, N'Auditoría', N'Auditoria del sistema', N'/config/audit', N'faClipboardCheck', 16, N'from-slate-500 to-gray-600'),
(817, 8, N'Respaldos', N'Respaldos del sistema', N'/config/backups', N'faDatabase', 17, N'from-green-500 to-emerald-600');

MERGE dbo.Submodules AS target
USING @Submodules AS source
ON target.ModuleId = source.ModuleId AND target.Path = source.Path
WHEN MATCHED THEN
    UPDATE SET
        Name = source.Name,
        Description = source.Description,
        Icon = source.Icon,
        [Order] = source.[Order],
        Color = source.Color,
        IsActive = 1,
        UpdatedAt = @Now
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, ModuleId, Name, Description, Path, Icon, [Order], Color, IsActive, CreatedAt)
    VALUES (source.Id, source.ModuleId, source.Name, source.Description, source.Path, source.Icon, source.[Order], source.Color, 1, @Now);

-- Ensure admin role can see and manage every active configured module/submodule.
MERGE dbo.RoleModulePermissions AS target
USING (
    SELECT
        1 AS RoleId,
        m.Id AS ModuleId,
        CAST(NULL AS int) AS SubmoduleId,
        m.Name,
        m.Path,
        m.Icon,
        m.[Order]
    FROM dbo.Modules m
    WHERE m.IsActive = 1
) AS source
ON target.RoleId = source.RoleId AND target.ModuleId = source.ModuleId AND target.SubmoduleId IS NULL
WHEN MATCHED THEN
    UPDATE SET
        Name = source.Name,
        Path = source.Path,
        Icon = source.Icon,
        [Order] = source.[Order],
        HasAccess = 1,
        CanView = 1,
        CanCreate = 1,
        CanEdit = 1,
        CanDelete = 1,
        UpdatedAt = @Now
WHEN NOT MATCHED BY TARGET THEN
    INSERT (RoleId, ModuleId, SubmoduleId, Name, Path, Icon, [Order], HasAccess, CanView, CanCreate, CanEdit, CanDelete, CreatedAt)
    VALUES (source.RoleId, source.ModuleId, source.SubmoduleId, source.Name, source.Path, source.Icon, source.[Order], 1, 1, 1, 1, 1, @Now);

MERGE dbo.RoleModulePermissions AS target
USING (
    SELECT
        1 AS RoleId,
        s.ModuleId,
        s.Id AS SubmoduleId,
        s.Name,
        s.Path,
        s.Icon,
        s.[Order]
    FROM dbo.Submodules s
    INNER JOIN dbo.Modules m ON m.Id = s.ModuleId
    WHERE s.IsActive = 1 AND m.IsActive = 1
) AS source
ON target.RoleId = source.RoleId AND target.ModuleId = source.ModuleId AND target.SubmoduleId = source.SubmoduleId
WHEN MATCHED THEN
    UPDATE SET
        Name = source.Name,
        Path = source.Path,
        Icon = source.Icon,
        [Order] = source.[Order],
        HasAccess = 1,
        CanView = 1,
        CanCreate = 1,
        CanEdit = 1,
        CanDelete = 1,
        UpdatedAt = @Now
WHEN NOT MATCHED BY TARGET THEN
    INSERT (RoleId, ModuleId, SubmoduleId, Name, Path, Icon, [Order], HasAccess, CanView, CanCreate, CanEdit, CanDelete, CreatedAt)
    VALUES (source.RoleId, source.ModuleId, source.SubmoduleId, source.Name, source.Path, source.Icon, source.[Order], 1, 1, 1, 1, 1, @Now);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This migration seeds navigation data idempotently. Down is intentionally
            // non-destructive to avoid removing modules/submodules already tied to roles or users.
        }
    }
}
