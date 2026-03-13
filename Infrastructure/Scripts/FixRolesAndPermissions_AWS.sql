-- ============================================
-- FIX: Crear Roles y Permisos del Administrador
-- Para ejecutar después de migraciones fallidas
-- ============================================

USE ERP;
GO

PRINT '?? Reparando Base de Datos: Roles y Permisos';
PRINT '==============================================';
PRINT '';

-- ============================================
-- 1. VERIFICAR Y CREAR ROLES
-- ============================================

PRINT '?? Paso 1: Verificar Roles...';

DECLARE @RolesCount INT;
SELECT @RolesCount = COUNT(*) FROM Roles;

IF @RolesCount = 0
BEGIN
    PRINT '   ??  No hay roles, creando 8 roles básicos...';
    
    SET IDENTITY_INSERT Roles ON;
    
    INSERT INTO Roles (Id, Name, Description, IsActive) VALUES
    (1, 'Administrador', 'Acceso completo al sistema ERP', 1),
    (2, 'Usuario', 'Acceso básico al sistema', 1),
    (3, 'Vendedor', 'Personal de ventas y atención a clientes', 1),
    (4, 'Almacenista', 'Gestión de inventario y productos', 1),
    (5, 'Gerente', 'Supervisión y reportes', 1),
    (6, 'Cajero', 'Operación de punto de venta', 1),
    (7, 'Contador', 'Gestión fiscal y contable', 1),
    (8, 'Comprador', 'Gestión de compras y proveedores', 1);
    
    SET IDENTITY_INSERT Roles OFF;
    
    PRINT '   ? 8 roles creados exitosamente';
END
ELSE
BEGIN
    PRINT '   ? Roles ya existen (' + CAST(@RolesCount AS VARCHAR) + ' roles)';
END

PRINT '';

-- ============================================
-- 2. VERIFICAR Y CREAR PERMISOS DEL ADMINISTRADOR
-- ============================================

PRINT '?? Paso 2: Verificar Permisos del Administrador (RoleId = 1)...';

DECLARE @AdminPermissionsCount INT;
SELECT @AdminPermissionsCount = COUNT(*) 
FROM RolePermissions 
WHERE RoleId = 1;

IF @AdminPermissionsCount = 0
BEGIN
    PRINT '   ??  No hay permisos para Administrador, asignando todos...';
    
    DECLARE @PermissionId INT = 1;
    WHILE @PermissionId <= 77
    BEGIN
        INSERT INTO [RolePermissions] ([RoleId], [PermissionId], [CreatedAt])
        VALUES (1, @PermissionId, GETUTCDATE());
        SET @PermissionId = @PermissionId + 1;
    END
    
    PRINT '   ? 77 permisos asignados al Administrador';
END
ELSE
BEGIN
    PRINT '   ? Administrador ya tiene permisos (' + CAST(@AdminPermissionsCount AS VARCHAR) + ' permisos)';
    
    IF @AdminPermissionsCount < 77
    BEGIN
        PRINT '   ??  Faltan permisos, completando...';
        
        -- Insertar solo los permisos faltantes
        DECLARE @CurrentPermId INT = 1;
        WHILE @CurrentPermId <= 77
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM RolePermissions WHERE RoleId = 1 AND PermissionId = @CurrentPermId)
            BEGIN
                INSERT INTO [RolePermissions] ([RoleId], [PermissionId], [CreatedAt])
                VALUES (1, @CurrentPermId, GETUTCDATE());
            END
            SET @CurrentPermId = @CurrentPermId + 1;
        END
        
        PRINT '   ? Permisos faltantes agregados';
    END
END

PRINT '';

-- ============================================
-- 3. VERIFICAR Y CREAR USUARIO ADMIN001
-- ============================================

PRINT '?? Paso 3: Verificar Usuario ADMIN001...';

IF NOT EXISTS (SELECT 1 FROM Users WHERE Code = 'ADMIN001')
BEGIN
    PRINT '   ??  Usuario ADMIN001 no existe, creando...';
    
    -- Password hash de "admin123"
    INSERT INTO Users (Code, Name, Email, Phone, RoleId, PasswordHash, Active, CreatedAt)
    VALUES (
        'ADMIN001',
        'Administrador',
        'admin@sistema.com',
        '1234567890',
        1,
        0x61646D696E313233,  -- Hash de "admin123"
        1,
        GETUTCDATE()
    );
    
    PRINT '   ? Usuario ADMIN001 creado';
    PRINT '      ?? Email: admin@sistema.com';
    PRINT '      ?? Password: admin123';
END
ELSE
BEGIN
    PRINT '   ? Usuario ADMIN001 ya existe';
    
    -- Verificar password hash
    DECLARE @CurrentHash VARBINARY(MAX);
    SELECT @CurrentHash = PasswordHash FROM Users WHERE Code = 'ADMIN001';
    
    IF @CurrentHash != 0x61646D696E313233
    BEGIN
        PRINT '   ??  Password hash incorrecto, actualizando...';
        UPDATE Users 
        SET PasswordHash = 0x61646D696E313233
        WHERE Code = 'ADMIN001';
        PRINT '   ? Password hash actualizado';
    END
    
    PRINT '      ?? Email: ' + (SELECT Email FROM Users WHERE Code = 'ADMIN001');
    PRINT '      ?? Password: admin123';
END

PRINT '';

-- ============================================
-- 4. RESUMEN FINAL
-- ============================================

PRINT '==============================================';
PRINT '? REPARACIÓN COMPLETADA';
PRINT '==============================================';
PRINT '';

DECLARE @TotalRoles INT, @TotalUsers INT, @TotalModules INT, @TotalPermissions INT, @AdminPerms INT;

SELECT @TotalRoles = COUNT(*) FROM Roles;
SELECT @TotalUsers = COUNT(*) FROM Users;
SELECT @TotalModules = COUNT(*) FROM Modules;
SELECT @TotalPermissions = COUNT(*) FROM Permissions;
SELECT @AdminPerms = COUNT(*) FROM RolePermissions WHERE RoleId = 1;

PRINT '?? Resumen de Base de Datos:';
PRINT '   ?? Roles: ' + CAST(@TotalRoles AS VARCHAR);
PRINT '   ?? Usuarios: ' + CAST(@TotalUsers AS VARCHAR);
PRINT '   ?? Módulos: ' + CAST(@TotalModules AS VARCHAR);
PRINT '   ?? Permisos Totales: ' + CAST(@TotalPermissions AS VARCHAR);
PRINT '   ? Permisos Administrador: ' + CAST(@AdminPerms AS VARCHAR) + ' / 77';
PRINT '';

IF @AdminPerms = 77
BEGIN
    PRINT '?? BASE DE DATOS LISTA PARA USAR';
    PRINT '';
    PRINT '?? Credenciales:';
    PRINT '   Usuario: ADMIN001';
    PRINT '   Password: admin123';
    PRINT '   Email: admin@sistema.com';
END
ELSE
BEGIN
    PRINT '??  ADVERTENCIA: Faltan ' + CAST(77 - @AdminPerms AS VARCHAR) + ' permisos del Administrador';
END

PRINT '';
PRINT '==============================================';

GO
