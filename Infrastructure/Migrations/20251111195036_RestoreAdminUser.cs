using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestoreAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =============================================
            // RESTAURAR USUARIO ADMINISTRADOR
            // =============================================
            
            // Verificar si el usuario existe y eliminarlo para recrearlo
            migrationBuilder.Sql("DELETE FROM [Users] WHERE [Code] = 'ADMIN001';");
            
            // Insertar el usuario administrador con la contraseña hasheada correctamente
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT [Users] ON;
                
                INSERT INTO [Users] ([Id], [Code], [Name], [PasswordHash], [Email], [Phone], [RoleId], [Active], [CreatedAt])
                VALUES (
                    1, 
                    'ADMIN001', 
                    'Administrador', 
                    0x240BE518FABD2724DDB6F04EEB1DA59674C8D7E831C08C8FA8228F74C720A9, -- SHA256 de 'admin123'
                    'admin@sistema.com', 
                    '1234567890', 
                    1, -- RoleId Administrador
                    1, -- Active = true
                    GETUTCDATE()
                );
                
                SET IDENTITY_INSERT [Users] OFF;
            ");
            
            // Crear algunos usuarios adicionales para pruebas
            migrationBuilder.Sql(@"
                INSERT INTO [Users] ([Code], [Name], [PasswordHash], [Email], [Phone], [RoleId], [Active], [CreatedAt])
                VALUES 
                    -- Vendedor de prueba
                    ('VEND001', 'Juan Vendedor', 0x240BE518FABD2724DDB6F04EEB1DA59674C8D7E831C08C8FA8228F74C720A9, 'vendedor@sistema.com', '5551234567', 3, 1, GETUTCDATE()),
                    -- Cajero de prueba  
                    ('CAJ001', 'María Cajera', 0x240BE518FABD2724DDB6F04EEB1DA59674C8D7E831C08C8FA8228F74C720A9, 'cajero@sistema.com', '5559876543', 6, 1, GETUTCDATE()),
                    -- Gerente de prueba
                    ('GER001', 'Carlos Gerente', 0x240BE518FABD2724DDB6F04EEB1DA59674C8D7E831C08C8FA8228F74C720A9, 'gerente@sistema.com', '5555555555', 5, 1, GETUTCDATE()),
                    -- Almacenista de prueba
                    ('ALM001', 'Ana Almacén', 0x240BE518FABD2724DDB6F04EEB1DA59674C8D7E831C08C8FA8228F74C720A9, 'almacen@sistema.com', '5552222222', 4, 1, GETUTCDATE());
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: Eliminar usuarios creados
            migrationBuilder.Sql(@"
                DELETE FROM [Users] WHERE [Code] IN ('ADMIN001', 'VEND001', 'CAJ001', 'GER001', 'ALM001');
            ");
        }
    }
}
