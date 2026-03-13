-- ========================================
-- SCRIPT: Verificar Permisos del Módulo de Ventas
-- Fecha: 2026-03-11
-- Descripción: Verifica que el módulo de Ventas y sus 
--              permisos estén correctamente configurados
-- ========================================

PRINT '?? ====================================';
PRINT '?? VERIFICANDO PERMISOS DEL MÓDULO DE VENTAS';
PRINT '?? ====================================';
PRINT '';

-- ========================================
-- 1. VERIFICAR MÓDULO DE VENTAS
-- ========================================
PRINT '?? 1. Verificando módulo de Ventas...';
PRINT '';

IF EXISTS (SELECT * FROM Modules WHERE Id = 2)
BEGIN
    SELECT 
        '? MÓDULO ENCONTRADO' AS Estado,
        Id,
        Name AS Nombre,
        Description AS Descripcion,
        Path AS Ruta,
        Icon AS Icono,
        [Order] AS Orden,
        CASE WHEN IsActive = 1 THEN 'Activo' ELSE 'Inactivo' END AS Estado_Activo
    FROM Modules 
    WHERE Id = 2;
    
    PRINT '';
END
ELSE
BEGIN
    PRINT '? ERROR: Módulo de Ventas NO encontrado';
    PRINT '';
END

-- ========================================
-- 2. VERIFICAR SUBMÓDULOS DE VENTAS
-- ========================================
PRINT '?? 2. Verificando submódulos de Ventas...';
PRINT '';

DECLARE @SubmoduleCount INT;
SELECT @SubmoduleCount = COUNT(*) FROM Submodules WHERE ModuleId = 2;

IF @SubmoduleCount > 0
BEGIN
    PRINT '? Submódulos encontrados: ' + CAST(@SubmoduleCount AS NVARCHAR(10));
    PRINT '';
    
    SELECT 
        Id,
        Name AS Nombre,
        Description AS Descripcion,
        Path AS Ruta,
        Icon AS Icono,
        [Order] AS Orden,
        Color,
        CASE WHEN IsActive = 1 THEN 'Activo' ELSE 'Inactivo' END AS Estado
    FROM Submodules 
    WHERE ModuleId = 2
    ORDER BY [Order];
    
    PRINT '';
END
ELSE
BEGIN
    PRINT '??  No se encontraron submódulos para Ventas';
    PRINT '';
END

-- ========================================
-- 3. VERIFICAR ROL ADMINISTRADOR
-- ========================================
PRINT '?? 3. Verificando rol Administrador...';
PRINT '';

IF EXISTS (SELECT * FROM Roles WHERE Id = 1)
BEGIN
    SELECT 
        '? ROL ENCONTRADO' AS Estado,
        Id,
        Name AS Nombre,
        Description AS Descripcion,
        CASE WHEN IsActive = 1 THEN 'Activo' ELSE 'Inactivo' END AS Estado_Activo
    FROM Roles 
    WHERE Id = 1;
    
    PRINT '';
END
ELSE
BEGIN
    PRINT '? ERROR: Rol Administrador NO encontrado';
    PRINT '';
END

-- ========================================
-- 4. VERIFICAR PERMISOS ASIGNADOS
-- ========================================
PRINT '?? 4. Verificando permisos del Administrador en Ventas...';
PRINT '';

DECLARE @PermissionCount INT;
SELECT @PermissionCount = COUNT(*) 
FROM RoleModulePermissions 
WHERE RoleId = 1 AND ModuleId = 2;

IF @PermissionCount > 0
BEGIN
    PRINT '? Permisos encontrados: ' + CAST(@PermissionCount AS NVARCHAR(10));
    PRINT '';
    
    SELECT 
        CASE 
            WHEN rmp.SubmoduleId IS NULL THEN '?? MÓDULO'
            ELSE '  ?? Submódulo'
        END AS Tipo,
        m.Name AS Modulo,
        ISNULL(s.Name, '-') AS Submodulo,
        CASE WHEN rmp.CanView = 1 THEN '?' ELSE '?' END AS Ver,
        CASE WHEN rmp.CanCreate = 1 THEN '?' ELSE '?' END AS Crear,
        CASE WHEN rmp.CanEdit = 1 THEN '?' ELSE '?' END AS Editar,
        CASE WHEN rmp.CanDelete = 1 THEN '?' ELSE '?' END AS Eliminar,
        CASE WHEN rmp.HasAccess = 1 THEN 'Sí' ELSE 'No' END AS Acceso
    FROM RoleModulePermissions rmp
    INNER JOIN Modules m ON m.Id = rmp.ModuleId
    LEFT JOIN Submodules s ON s.Id = rmp.SubmoduleId
    WHERE rmp.RoleId = 1 AND m.Id = 2
    ORDER BY 
        CASE WHEN rmp.SubmoduleId IS NULL THEN 0 ELSE 1 END,
        ISNULL(s.[Order], 0);
    
    PRINT '';
END
ELSE
BEGIN
    PRINT '? ERROR: No se encontraron permisos asignados';
    PRINT '';
END

-- ========================================
-- 5. VERIFICAR USUARIOS CON ROL ADMINISTRADOR
-- ========================================
PRINT '?? 5. Verificando usuarios con rol Administrador...';
PRINT '';

DECLARE @AdminUserCount INT;
SELECT @AdminUserCount = COUNT(*) FROM Users WHERE RoleId = 1 AND Active = 1;

IF @AdminUserCount > 0
BEGIN
    PRINT '? Usuarios Administradores activos: ' + CAST(@AdminUserCount AS NVARCHAR(10));
    PRINT '';
    
    SELECT 
        Id,
        Code AS Codigo,
        Name AS Nombre,
        Email,
        CASE WHEN Active = 1 THEN 'Activo' ELSE 'Inactivo' END AS Estado
    FROM Users 
    WHERE RoleId = 1
    ORDER BY Code;
    
    PRINT '';
END
ELSE
BEGIN
    PRINT '??  No se encontraron usuarios con rol Administrador';
    PRINT '';
END

-- ========================================
-- RESUMEN FINAL
-- ========================================
PRINT '';
PRINT '?? ====================================';
PRINT '?? RESUMEN DE VERIFICACIÓN';
PRINT '?? ====================================';
PRINT '';

DECLARE @ModuleExists BIT = CASE WHEN EXISTS (SELECT * FROM Modules WHERE Id = 2) THEN 1 ELSE 0 END;
DECLARE @RoleExists BIT = CASE WHEN EXISTS (SELECT * FROM Roles WHERE Id = 1) THEN 1 ELSE 0 END;
DECLARE @HasPermissions BIT = CASE WHEN @PermissionCount > 0 THEN 1 ELSE 0 END;
DECLARE @HasAdminUsers BIT = CASE WHEN @AdminUserCount > 0 THEN 1 ELSE 0 END;

SELECT @SubmoduleCount = COUNT(*) FROM Submodules WHERE ModuleId = 2;

IF @ModuleExists = 1
    PRINT '   ? Módulo de Ventas: EXISTE';
ELSE
    PRINT '   ? Módulo de Ventas: NO EXISTE';

PRINT '   ?? Submódulos de Ventas: ' + CAST(@SubmoduleCount AS NVARCHAR(10));

IF @RoleExists = 1
    PRINT '   ? Rol Administrador: EXISTE';
ELSE
    PRINT '   ? Rol Administrador: NO EXISTE';

PRINT '   ?? Permisos asignados: ' + CAST(@PermissionCount AS NVARCHAR(10));
PRINT '   ?? Usuarios Administradores: ' + CAST(@AdminUserCount AS NVARCHAR(10));

PRINT '';

-- Validación final
IF @ModuleExists = 1 AND @HasPermissions = 1 AND @HasAdminUsers = 1
BEGIN
    PRINT '?? ====================================';
    PRINT '? SISTEMA CORRECTAMENTE CONFIGURADO';
    PRINT '?? ====================================';
    PRINT '';
    PRINT 'El módulo de Ventas está listo para usarse.';
    PRINT 'Los usuarios con rol Administrador tienen acceso completo.';
END
ELSE
BEGIN
    PRINT '??  ====================================';
    PRINT '??  ADVERTENCIAS ENCONTRADAS';
    PRINT '??  ====================================';
    PRINT '';
    
    IF @ModuleExists = 0
        PRINT '   ? Falta: Módulo de Ventas';
    
    IF @SubmoduleCount = 0
        PRINT '   ??  Falta: Submódulos de Ventas';
    
    IF @RoleExists = 0
        PRINT '   ? Falta: Rol Administrador';
    
    IF @HasPermissions = 0
        PRINT '   ? Falta: Permisos asignados al Administrador';
    
    IF @HasAdminUsers = 0
        PRINT '   ??  Advertencia: No hay usuarios Administradores';
    
    PRINT '';
    PRINT 'Ejecute el script AddSalesModulePermissions.sql para configurar los permisos.';
END

PRINT '';
