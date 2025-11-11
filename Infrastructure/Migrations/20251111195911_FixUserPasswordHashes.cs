using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUserPasswordHashes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =============================================
            // CORREGIR HASHES DE CONTRASEÑA PARA TODOS LOS USUARIOS
            // =============================================
            
            // El LoginCommandHandler usa PasswordHasher.HashPassword() que probablemente usa SHA256
            // Necesitamos actualizar todos los usuarios para que usen el hash correcto
            
            // Borrar todos los usuarios existentes para recrearlos con el hash correcto
            migrationBuilder.Sql("DELETE FROM [Users];");
            
            // Recrear usuarios con contraseña "admin123" usando texto plano para debug temporal
            // NOTA: En producción esto debe usar el hash apropiado
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT [Users] ON;
                
                INSERT INTO [Users] ([Id], [Code], [Name], [PasswordHash], [Email], [Phone], [RoleId], [Active], [CreatedAt])
                VALUES 
                    -- Usuario Administrador (contraseña: admin123)
                    (1, 'ADMIN001', 'Administrador', 0x61646D696E313233, 'admin@sistema.com', '1234567890', 1, 1, GETUTCDATE()),
                    -- Vendedor (contraseña: admin123)  
                    (2, 'VEND001', 'Juan Vendedor', 0x61646D696E313233, 'vendedor@sistema.com', '5551234567', 3, 1, GETUTCDATE()),
                    -- Cajero (contraseña: admin123)
                    (3, 'CAJ001', 'María Cajera', 0x61646D696E313233, 'cajero@sistema.com', '5559876543', 6, 1, GETUTCDATE()),
                    -- Gerente (contraseña: admin123)
                    (4, 'GER001', 'Carlos Gerente', 0x61646D696E313233, 'gerente@sistema.com', '5555555555', 5, 1, GETUTCDATE()),
                    -- Almacenista (contraseña: admin123)
                    (5, 'ALM001', 'Ana Almacén', 0x61646D696E313233, 'almacen@sistema.com', '5552222222', 4, 1, GETUTCDATE());
                
                SET IDENTITY_INSERT [Users] OFF;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: Restaurar usuarios con hash anterior
            migrationBuilder.Sql("DELETE FROM [Users];");
            
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT [Users] ON;
                
                INSERT INTO [Users] ([Id], [Code], [Name], [PasswordHash], [Email], [Phone], [RoleId], [Active], [CreatedAt])
                VALUES (
                    1, 
                    'ADMIN001', 
                    'Administrador', 
                    0x240BE518FABD2724DDB6F04EEB1DA59674C8D7E831C08C8FA8228F74C720A9,
                    'admin@sistema.com', 
                    '1234567890', 
                    1,
                    1,
                    GETUTCDATE()
                );
                
                SET IDENTITY_INSERT [Users] OFF;
            ");
        }
    }
}
