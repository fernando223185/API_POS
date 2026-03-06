-- ============================================
-- VERIFICAR ESTRUCTURA DE BASE DE DATOS
-- Script Portable - Compatible con SQL Server, MySQL, PostgreSQL
-- ============================================

-- ?? IMPORTANTE: Ejecutar este script ANTES de cualquier migración
-- Verifica que todas las tablas y columnas necesarias existen

-- ============================================
-- CONFIGURACIÓN
-- ============================================

SET NOCOUNT ON;

PRINT '?? Verificando estructura de base de datos...';
PRINT '==============================================';
PRINT '';

-- ============================================
-- 1. VERIFICAR TABLAS PRINCIPALES
-- ============================================

PRINT '?? Verificando tablas principales...';
PRINT '';

DECLARE @MissingTables TABLE (TableName NVARCHAR(100));

-- Verificar Roles
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Roles')
    INSERT INTO @MissingTables VALUES ('Roles');

-- Verificar Users
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
    INSERT INTO @MissingTables VALUES ('Users');

-- Verificar Modules
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Modules')
    INSERT INTO @MissingTables VALUES ('Modules');

-- Verificar Submodules
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Submodules')
    INSERT INTO @MissingTables VALUES ('Submodules');

-- Verificar RoleModulePermissions
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RoleModulePermissions')
    INSERT INTO @MissingTables VALUES ('RoleModulePermissions');

-- Verificar UserModulePermissions
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserModulePermissions')
    INSERT INTO @MissingTables VALUES ('UserModulePermissions');

-- Mostrar tablas faltantes
IF EXISTS (SELECT 1 FROM @MissingTables)
BEGIN
    PRINT '? ERROR: Faltan las siguientes tablas:';
    SELECT TableName AS [Tabla Faltante] FROM @MissingTables;
    PRINT '';
    PRINT '??  Por favor, ejecuta las migraciones de EF Core primero:';
    PRINT '   dotnet ef database update --project Infrastructure --startup-project Web.Api';
    PRINT '';
    RETURN;
END
ELSE
BEGIN
    PRINT '? Todas las tablas principales existen';
    PRINT '';
END

-- ============================================
-- 2. VERIFICAR DATOS BÁSICOS
-- ============================================

PRINT '?? Verificando datos básicos...';
PRINT '';

-- Verificar que existen roles
DECLARE @RoleCount INT;
SELECT @RoleCount = COUNT(*) FROM Roles;

IF @RoleCount = 0
BEGIN
    PRINT '??  ADVERTENCIA: No hay roles en la base de datos';
    PRINT '   Necesitas crear al menos el rol Administrador (ID: 1)';
    PRINT '';
END
ELSE
BEGIN
    PRINT '? Roles encontrados: ' + CAST(@RoleCount AS VARCHAR);
    
    -- Mostrar roles existentes
    SELECT Id, Name, Description, IsActive 
    FROM Roles 
    ORDER BY Id;
    PRINT '';
END

-- Verificar que existen usuarios
DECLARE @UserCount INT;
SELECT @UserCount = COUNT(*) FROM Users WHERE Active = 1;

IF @UserCount = 0
BEGIN
    PRINT '??  ADVERTENCIA: No hay usuarios activos en la base de datos';
    PRINT '';
END
ELSE
BEGIN
    PRINT '? Usuarios activos: ' + CAST(@UserCount AS VARCHAR);
    PRINT '';
END

-- Verificar que existen módulos
DECLARE @ModuleCount INT;
SELECT @ModuleCount = COUNT(*) FROM Modules WHERE IsActive = 1;

IF @ModuleCount = 0
BEGIN
    PRINT '??  ADVERTENCIA: No hay módulos en la base de datos';
    PRINT '   Ejecuta el script de seed data: SeedSystemModules_Portable.sql';
    PRINT '';
END
ELSE
BEGIN
    PRINT '? Módulos activos: ' + CAST(@ModuleCount AS VARCHAR);
    
    -- Mostrar módulos existentes
    SELECT Id, Name, [Order], IsActive 
    FROM Modules 
    WHERE IsActive = 1
    ORDER BY [Order];
    PRINT '';
END

-- Verificar que existen submódulos
DECLARE @SubmoduleCount INT;
SELECT @SubmoduleCount = COUNT(*) FROM Submodules WHERE IsActive = 1;

IF @SubmoduleCount = 0
BEGIN
    PRINT '??  ADVERTENCIA: No hay submódulos en la base de datos';
    PRINT '   Ejecuta el script de seed data: SeedSystemModules_Portable.sql';
    PRINT '';
END
ELSE
BEGIN
    PRINT '? Submódulos activos: ' + CAST(@SubmoduleCount AS VARCHAR);
    PRINT '';
END

-- ============================================
-- 3. VERIFICAR PERMISOS EXISTENTES
-- ============================================

PRINT '?? Verificando permisos existentes...';
PRINT '';

-- Verificar permisos de roles
DECLARE @RolePermCount INT;
SELECT @RolePermCount = COUNT(*) FROM RoleModulePermissions;

IF @RolePermCount = 0
BEGIN
    PRINT '??  No hay permisos asignados a roles';
    PRINT '   Ejecuta el script: AssignFullPermissionsToAdmin_Portable.sql';
    PRINT '';
END
ELSE
BEGIN
    PRINT '? Permisos de roles existentes: ' + CAST(@RolePermCount AS VARCHAR);
    
    -- Mostrar resumen de permisos por rol
    SELECT 
        r.Id AS RoleId,
        r.Name AS RoleName,
        COUNT(DISTINCT rmp.ModuleId) AS Modulos,
        COUNT(rmp.Id) AS TotalPermisos
    FROM Roles r
    LEFT JOIN RoleModulePermissions rmp ON rmp.RoleId = r.Id
    WHERE r.IsActive = 1
    GROUP BY r.Id, r.Name
    ORDER BY r.Id;
    PRINT '';
END

-- Verificar permisos personalizados de usuarios
DECLARE @UserPermCount INT;
SELECT @UserPermCount = COUNT(*) FROM UserModulePermissions;

IF @UserPermCount = 0
BEGIN
    PRINT '??  No hay permisos personalizados de usuarios (esto es normal)';
    PRINT '';
END
ELSE
BEGIN
    PRINT '? Permisos personalizados de usuarios: ' + CAST(@UserPermCount AS VARCHAR);
    PRINT '';
END

-- ============================================
-- 4. VERIFICAR INTEGRIDAD REFERENCIAL
-- ============================================

PRINT '?? Verificando integridad referencial...';
PRINT '';

-- Verificar que todos los usuarios tienen un rol válido
DECLARE @InvalidUserRoles INT;
SELECT @InvalidUserRoles = COUNT(*)
FROM Users u
WHERE u.RoleId NOT IN (SELECT Id FROM Roles);

IF @InvalidUserRoles > 0
BEGIN
    PRINT '? ERROR: Hay ' + CAST(@InvalidUserRoles AS VARCHAR) + ' usuarios con RoleId inválido';
    
    SELECT u.Id, u.Code, u.Name, u.RoleId
    FROM Users u
    WHERE u.RoleId NOT IN (SELECT Id FROM Roles);
    PRINT '';
END
ELSE
BEGIN
    PRINT '? Todos los usuarios tienen un rol válido';
    PRINT '';
END

-- Verificar que todos los submódulos tienen un módulo padre válido
DECLARE @InvalidSubmodules INT;
SELECT @InvalidSubmodules = COUNT(*)
FROM Submodules s
WHERE s.ModuleId NOT IN (SELECT Id FROM Modules);

IF @InvalidSubmodules > 0
BEGIN
    PRINT '? ERROR: Hay ' + CAST(@InvalidSubmodules AS VARCHAR) + ' submódulos con ModuleId inválido';
    PRINT '';
END
ELSE
BEGIN
    PRINT '? Todos los submódulos tienen un módulo padre válido';
    PRINT '';
END

-- ============================================
-- 5. RESUMEN FINAL
-- ============================================

PRINT '';
PRINT '?? RESUMEN DE VERIFICACIÓN';
PRINT '===========================';
PRINT '';

-- Contar elementos críticos
DECLARE @Summary TABLE (
    Elemento NVARCHAR(50),
    Cantidad INT,
    Estado NVARCHAR(20)
);

INSERT INTO @Summary VALUES ('Roles', @RoleCount, CASE WHEN @RoleCount > 0 THEN '? OK' ELSE '? FALTA' END);
INSERT INTO @Summary VALUES ('Usuarios Activos', @UserCount, CASE WHEN @UserCount > 0 THEN '? OK' ELSE '??  FALTA' END);
INSERT INTO @Summary VALUES ('Módulos', @ModuleCount, CASE WHEN @ModuleCount > 0 THEN '? OK' ELSE '? FALTA' END);
INSERT INTO @Summary VALUES ('Submódulos', @SubmoduleCount, CASE WHEN @SubmoduleCount > 0 THEN '? OK' ELSE '? FALTA' END);
INSERT INTO @Summary VALUES ('Permisos de Roles', @RolePermCount, CASE WHEN @RolePermCount > 0 THEN '? OK' ELSE '??  FALTA' END);
INSERT INTO @Summary VALUES ('Permisos de Usuarios', @UserPermCount, '??  Opcional');

SELECT * FROM @Summary;

PRINT '';

-- Verificar si está listo para migración
IF @RoleCount = 0 OR @ModuleCount = 0 OR @SubmoduleCount = 0
BEGIN
    PRINT '? LA BASE DE DATOS NO ESTÁ LISTA PARA MIGRACIÓN';
    PRINT '';
    PRINT '??  Pasos requeridos:';
    IF @RoleCount = 0 PRINT '   1. Crear roles en la tabla Roles';
    IF @ModuleCount = 0 OR @SubmoduleCount = 0 PRINT '   2. Ejecutar SeedSystemModules_Portable.sql';
    PRINT '   3. Ejecutar AssignFullPermissionsToAdmin_Portable.sql';
    PRINT '';
END
ELSE
BEGIN
    PRINT '? LA BASE DE DATOS ESTÁ LISTA PARA MIGRACIÓN';
    PRINT '';
    IF @RolePermCount = 0
    BEGIN
        PRINT '??  Recomendación: Ejecuta AssignFullPermissionsToAdmin_Portable.sql';
        PRINT '   para asignar permisos al rol Administrador';
        PRINT '';
    END
END

PRINT '================================================';
PRINT '? Verificación completada';
PRINT '';

GO
