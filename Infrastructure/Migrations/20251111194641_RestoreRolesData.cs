using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestoreRolesData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =============================================
            // RESTAURAR ROLES COMPLETOS DEL SISTEMA ERP
            // =============================================
            
            // Primero limpiar roles existentes (si los hay)
            migrationBuilder.Sql("DELETE FROM [Roles];");
            
            // Insertar todos los roles del sistema
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT [Roles] ON;
                
                INSERT INTO [Roles] ([Id], [Name], [Description], [IsActive])
                VALUES 
                    (1, 'Administrador', 'Acceso completo al sistema ERP', 1),
                    (2, 'Usuario', 'Acceso básico al sistema', 1),
                    (3, 'Vendedor', 'Personal de ventas y atención a clientes', 1),
                    (4, 'Almacenista', 'Gestión de inventario y productos', 1),
                    (5, 'Gerente', 'Supervisión y reportes', 1),
                    (6, 'Cajero', 'Operación de punto de venta', 1),
                    (7, 'Contador', 'Gestión fiscal y contable', 1),
                    (8, 'Comprador', 'Gestión de compras y proveedores', 1),
                    (9, 'Supervisor', 'Supervisión de operaciones', 1),
                    (10, 'Auditor', 'Acceso de solo lectura para auditoría', 1);
                
                SET IDENTITY_INSERT [Roles] OFF;
            ");
            
            // Asignar permisos adicionales a los nuevos roles
            migrationBuilder.Sql(@"
                -- Rol Gerente (5) - Todos los permisos excepto configuración crítica
                INSERT INTO [RolePermissions] ([RoleId], [PermissionId], [CreatedAt])
                SELECT 5, [Id], GETUTCDATE() 
                FROM [Permissions] 
                WHERE [Resource] IN ('Customer', 'Sale', 'Product', 'Inventory', 'Billing')
                   OR ([Resource] = 'Configuration' AND [Name] NOT IN ('ManageUsers', 'ManagePermissions'));

                -- Rol Cajero (6) - Solo operación POS
                INSERT INTO [RolePermissions] ([RoleId], [PermissionId], [CreatedAt])
                VALUES 
                    (6, 19, GETUTCDATE()),  -- Sale CreateSale
                    (6, 20, GETUTCDATE()),  -- Sale ViewHistory  
                    (6, 21, GETUTCDATE()),  -- Sale ViewProducts
                    (6, 22, GETUTCDATE()),  -- Sale ProcessPayment
                    (6, 24, GETUTCDATE()),  -- Product Read
                    (6, 27, GETUTCDATE()),  -- Product ViewCatalog
                    (6, 2, GETUTCDATE()),   -- Customer Read
                    (6, 5, GETUTCDATE());   -- Customer ViewList

                -- Rol Contador (7) - Gestión fiscal y reportes
                INSERT INTO [RolePermissions] ([RoleId], [PermissionId], [CreatedAt])
                SELECT 7, [Id], GETUTCDATE() 
                FROM [Permissions] 
                WHERE [Resource] IN ('Billing', 'Customer') 
                   OR ([Resource] = 'Configuration' AND [Name] IN ('ManageCFDI', 'ViewAudit'))
                   OR ([Resource] = 'Product' AND [Name] = 'ViewReports')
                   OR ([Resource] = 'Inventory' AND [Name] = 'ViewStock');

                -- Rol Comprador (8) - Gestión de compras y proveedores
                INSERT INTO [RolePermissions] ([RoleId], [PermissionId], [CreatedAt])
                VALUES 
                    (8, 35, GETUTCDATE()),  -- Product ManageSuppliers
                    (8, 36, GETUTCDATE()),  -- Product ManageOrders
                    (8, 23, GETUTCDATE()),  -- Product Create
                    (8, 24, GETUTCDATE()),  -- Product Read
                    (8, 25, GETUTCDATE()),  -- Product Update
                    (8, 27, GETUTCDATE()),  -- Product ViewCatalog
                    (8, 29, GETUTCDATE()),  -- Product ViewStock
                    (8, 43, GETUTCDATE()),  -- Inventory ManageInbound
                    (8, 51, GETUTCDATE());  -- Inventory ManageReceipts

                -- Rol Supervisor (9) - Supervisión de operaciones
                INSERT INTO [RolePermissions] ([RoleId], [PermissionId], [CreatedAt])
                SELECT 9, [Id], GETUTCDATE() 
                FROM [Permissions] 
                WHERE [Resource] IN ('Customer', 'Sale', 'Product', 'Inventory') 
                   AND [Name] NOT IN ('Delete', 'ManageUsers', 'ManagePermissions');

                -- Rol Auditor (10) - Solo lectura
                INSERT INTO [RolePermissions] ([RoleId], [PermissionId], [CreatedAt])
                SELECT 10, [Id], GETUTCDATE() 
                FROM [Permissions] 
                WHERE [Name] IN ('Read', 'ViewList', 'ViewHistory', 'ViewCatalog', 'ViewStock', 
                               'ViewKardex', 'ViewInvoices', 'ViewReports', 'ViewAudit');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: Eliminar roles agregados (mantener solo los básicos)
            migrationBuilder.Sql(@"
                DELETE FROM [RolePermissions] WHERE [RoleId] IN (5, 6, 7, 8, 9, 10);
                DELETE FROM [Roles] WHERE [Id] IN (5, 6, 7, 8, 9, 10);
            ");
        }
    }
}
