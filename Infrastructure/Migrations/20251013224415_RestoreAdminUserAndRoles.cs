using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestoreAdminUserAndRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ PASO 1: Agregar columna IsActive primero
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Roles]') AND name = 'IsActive')
                BEGIN
                    ALTER TABLE [Roles] ADD [IsActive] BIT NOT NULL DEFAULT 1;
                END
            ");

            // ✅ PASO 2: Insertar roles (en un comando separado para que reconozca la columna IsActive)
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT [Roles] ON;
                
                IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE Id = 1)
                    INSERT INTO [Roles] ([Id], [Description], [Name], [IsActive]) VALUES (1, N'Acceso completo al sistema', N'Administrador', 1);
                
                IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE Id = 2)
                    INSERT INTO [Roles] ([Id], [Description], [Name], [IsActive]) VALUES (2, N'Acceso básico al sistema', N'Usuario', 1);
                
                IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE Id = 3)
                    INSERT INTO [Roles] ([Id], [Description], [Name], [IsActive]) VALUES (3, N'Personal de ventas', N'Vendedor', 1);
                
                IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE Id = 4)
                    INSERT INTO [Roles] ([Id], [Description], [Name], [IsActive]) VALUES (4, N'Gestión de inventario', N'Almacenista', 1);
                
                IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE Id = 10)
                    INSERT INTO [Roles] ([Id], [Description], [Name], [IsActive]) VALUES (10, N'Operación de punto de venta', N'Cajero', 1);
                
                IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE Id = 11)
                    INSERT INTO [Roles] ([Id], [Description], [Name], [IsActive]) VALUES (11, N'Supervisión y reportes', N'Gerente', 1);
                
                SET IDENTITY_INSERT [Roles] OFF;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM [Roles] WHERE Id IN (1, 2, 3, 4, 10, 11);
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Roles]') AND name = 'IsActive')
                BEGIN
                    ALTER TABLE [Roles] DROP COLUMN [IsActive];
                END
            ");
        }
    }
}
