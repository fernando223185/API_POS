using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateModulesAndPermissionsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================
            // PASO 1: CREAR TABLAS DEL SISTEMA UNIFICADO
            // ============================================
            migrationBuilder.Sql(@"
                -- Crear tabla Modules si no existe
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Modules]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [Modules] (
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
                    
                    CREATE INDEX [IX_Modules_Order] ON [Modules]([Order]);
                    CREATE INDEX [IX_Modules_IsActive] ON [Modules]([IsActive]);
                END
                
                -- Crear tabla Submodules si no existe
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Submodules]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [Submodules] (
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
                            REFERENCES [Modules]([Id]) ON DELETE CASCADE
                    );
                    
                    CREATE INDEX [IX_Submodules_ModuleId] ON [Submodules]([ModuleId]);
                    CREATE INDEX [IX_Submodules_Order] ON [Submodules]([Order]);
                    CREATE INDEX [IX_Submodules_IsActive] ON [Submodules]([IsActive]);
                END
                
                -- Crear tabla RoleModulePermissions si no existe
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RoleModulePermissions]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [RoleModulePermissions] (
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
                            REFERENCES [Roles]([Id]) ON DELETE CASCADE
                    );
                    
                    CREATE INDEX [IX_RoleModulePermissions_RoleId] ON [RoleModulePermissions]([RoleId]);
                    CREATE INDEX [IX_RoleModulePermissions_ModuleId] ON [RoleModulePermissions]([ModuleId]);
                    CREATE UNIQUE INDEX [IX_RoleModulePermissions_RoleId_ModuleId_SubmoduleId] 
                        ON [RoleModulePermissions]([RoleId], [ModuleId], [SubmoduleId]) 
                        WHERE [SubmoduleId] IS NOT NULL;
                END
            ");

            // ============================================
            // PASO 2: INSERTAR MÓDULOS Y SUBMÓDULOS
            // ============================================
            migrationBuilder.Sql(@"
                -- Insertar módulos solo si la tabla está vacía
                IF NOT EXISTS (SELECT 1 FROM [Modules])
                BEGIN
                    INSERT INTO [Modules] ([Id], [Name], [Description], [Path], [Icon], [Order], [IsActive], [CreatedAt])
                    VALUES
                        (1, 'Inicio', 'Dashboard principal del sistema', '/dashboard', 'faHome', 1, 1, GETUTCDATE()),
                        (2, 'Ventas', 'Módulo de ventas y facturación', '/sales', 'faShoppingCart', 2, 1, GETUTCDATE()),
                        (3, 'Productos', 'Gestión de productos', '/products', 'faBoxOpen', 3, 1, GETUTCDATE()),
                        (4, 'Inventario', 'Control de inventario', '/inventory', 'faWarehouse', 4, 1, GETUTCDATE()),
                        (5, 'Clientes', 'CRM y gestión de clientes', '/customers', 'faUsers', 5, 1, GETUTCDATE()),
                        (6, 'CFDI', 'Facturación electrónica', '/billing', 'faFileInvoice', 6, 1, GETUTCDATE()),
                        (7, 'Reportes', 'Reportes y estadísticas', '/reports', 'faChartBar', 7, 1, GETUTCDATE()),
                        (8, 'Configuración', 'Configuración del sistema', '/config', 'faCog', 8, 1, GETUTCDATE());
                    
                    -- Submódulos de Ventas (Módulo 2)
                    INSERT INTO [Submodules] ([Id], [ModuleId], [Name], [Description], [Path], [Icon], [Order], [Color], [IsActive], [CreatedAt])
                    VALUES
                        (21, 2, 'Nueva Venta', 'Punto de venta', '/sales/new', 'faCashRegister', 1, '#27ae60', 1, GETUTCDATE()),
                        (22, 2, 'Historial', 'Historial de ventas', '/sales/history', 'faHistory', 2, '#3498db', 1, GETUTCDATE()),
                        (23, 2, 'Cotizaciones', 'Gestión de cotizaciones', '/sales/quotes', 'faFileAlt', 3, '#f39c12', 1, GETUTCDATE()),
                        (24, 2, 'Devoluciones', 'Gestión de devoluciones', '/sales/returns', 'faUndo', 4, '#e74c3c', 1, GETUTCDATE()),
                        
                        -- Submódulos de Productos (Módulo 3)
                        (31, 3, 'Catálogo', 'Ver y gestionar productos', '/products/catalog', 'faList', 1, '#9b59b6', 1, GETUTCDATE()),
                        (32, 3, 'Nuevo Producto', 'Agregar nuevo producto', '/products/new', 'faPlusCircle', 2, '#1abc9c', 1, GETUTCDATE()),
                        (33, 3, 'Categorías', 'Gestión de categorías', '/products/categories', 'faTags', 3, '#34495e', 1, GETUTCDATE()),
                        (34, 3, 'Listas de Precios', 'Gestión de precios', '/products/price-lists', 'faMoneyBill', 4, '#2980b9', 1, GETUTCDATE()),
                        
                        -- Submódulos de Inventario (Módulo 4)
                        (41, 4, 'Stock Actual', 'Ver stock actual', '/inventory/current', 'faBoxes', 1, '#16a085', 1, GETUTCDATE()),
                        (42, 4, 'Movimientos', 'Historial de movimientos', '/inventory/movements', 'faExchangeAlt', 2, '#e67e22', 1, GETUTCDATE()),
                        (43, 4, 'Ajustes', 'Ajustes de inventario', '/inventory/adjustments', 'faEdit', 3, '#95a5a6', 1, GETUTCDATE()),
                        (44, 4, 'Transferencias', 'Transferencias entre almacenes', '/inventory/transfers', 'faTruck', 4, '#3498db', 1, GETUTCDATE()),
                        
                        -- Submódulos de Clientes (Módulo 5)
                        (51, 5, 'Directorio', 'Lista de clientes', '/customers/directory', 'faAddressBook', 1, '#3498db', 1, GETUTCDATE()),
                        (52, 5, 'Nuevo Cliente', 'Registrar cliente', '/customers/new', 'faUserPlus', 2, '#2ecc71', 1, GETUTCDATE()),
                        (53, 5, 'Estado de Cuenta', 'Cuentas por cobrar', '/customers/account-status', 'faFileInvoice', 3, '#e67e22', 1, GETUTCDATE()),
                        
                        -- Submódulos de CFDI (Módulo 6)
                        (61, 6, 'Generar Factura', 'Crear factura electrónica', '/billing/generate', 'faFileInvoice', 1, '#27ae60', 1, GETUTCDATE()),
                        (62, 6, 'Facturas Emitidas', 'Consultar facturas', '/billing/issued', 'faReceipt', 2, '#3498db', 1, GETUTCDATE()),
                        (63, 6, 'Notas de Crédito', 'Gestión de notas de crédito', '/billing/credit-notes', 'faFileMedical', 3, '#e74c3c', 1, GETUTCDATE()),
                        (64, 6, 'Configuración SAT', 'Configuración fiscal', '/billing/sat-config', 'faCogs', 4, '#95a5a6', 1, GETUTCDATE()),
                        
                        -- Submódulos de Reportes (Módulo 7)
                        (71, 7, 'Ventas', 'Reporte de ventas', '/reports/sales', 'faChartLine', 1, '#3498db', 1, GETUTCDATE()),
                        (72, 7, 'Productos', 'Análisis de productos', '/reports/products', 'faTrophy', 2, '#f39c12', 1, GETUTCDATE()),
                        (73, 7, 'Clientes', 'Reporte de clientes', '/reports/customers', 'faUsers', 3, '#9b59b6', 1, GETUTCDATE()),
                        (74, 7, 'Inventario', 'Reporte de inventario', '/reports/inventory', 'faBoxes', 4, '#16a085', 1, GETUTCDATE()),
                        
                        -- Submódulos de Configuración (Módulo 8)
                        (81, 8, 'Empresa', 'Datos de la empresa', '/config/company', 'faBuilding', 1, '#34495e', 1, GETUTCDATE()),
                        (82, 8, 'Sucursales', 'Gestión de sucursales', '/config/branches', 'faStore', 2, '#16a085', 1, GETUTCDATE()),
                        (83, 8, 'Usuarios', 'Gestión de usuarios', '/config/users', 'faUsersCog', 3, '#2c3e50', 1, GETUTCDATE()),
                        (84, 8, 'Roles', 'Gestión de roles', '/config/roles', 'faUserTag', 4, '#8e44ad', 1, GETUTCDATE()),
                        (85, 8, 'Permisos', 'Asignación de permisos', '/config/permissions', 'faKey', 5, '#c0392b', 1, GETUTCDATE()),
                        (86, 8, 'Módulos', 'Gestión de módulos', '/config/modules', 'faThLarge', 6, '#3498db', 1, GETUTCDATE());
                END
            ");

            // ============================================
            // PASO 3: ASIGNAR PERMISOS AL ADMINISTRADOR
            // ============================================
            migrationBuilder.Sql(@"
                -- Limpiar permisos existentes del rol Administrador
                DELETE FROM [RoleModulePermissions] WHERE [RoleId] = 1;

                -- Insertar permisos para módulos principales (8 módulos)
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

                -- Insertar permisos para submódulos (29 submódulos)
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
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS [RoleModulePermissions];
                DROP TABLE IF EXISTS [Submodules];
                DROP TABLE IF EXISTS [Modules];
            ");
        }
    }
}
