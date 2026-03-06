-- ============================================
-- ASIGNAR PERMISOS COMPLETOS AL ROL ADMINISTRADOR
-- Rol ID: 1 (Administrador)
-- Asigna acceso total a todos los módulos y submódulos
-- ============================================

USE ERP;
GO

PRINT '?? Asignando permisos completos al Rol Administrador...';
PRINT '========================================================';
PRINT '';

-- ============================================
-- VERIFICAR QUE EL ROL EXISTE
-- ============================================

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Id = 1)
BEGIN
    PRINT '? ERROR: El Rol ID 1 (Administrador) no existe';
    PRINT '   Por favor, crea el rol primero antes de ejecutar este script.';
    RETURN;
END

DECLARE @RoleName NVARCHAR(100);
SELECT @RoleName = Name FROM Roles WHERE Id = 1;

PRINT '? Rol encontrado: ' + @RoleName + ' (ID: 1)';
PRINT '';

-- ============================================
-- LIMPIAR PERMISOS EXISTENTES DEL ROL
-- ============================================

PRINT '???  Limpiando permisos existentes del rol...';

DELETE FROM RoleModulePermissions WHERE RoleId = 1;

PRINT '? Permisos anteriores eliminados';
PRINT '';

-- ============================================
-- ASIGNAR PERMISOS A NIVEL MÓDULO
-- ============================================

PRINT '?? Asignando permisos a nivel módulo...';

INSERT INTO RoleModulePermissions (
    RoleId,
    ModuleId,
    Name,
    Path,
    Icon,
    [Order],
    HasAccess,
    SubmoduleId,
    CanView,
    CanCreate,
    CanEdit,
    CanDelete,
    CreatedAt,
    CreatedByUserId
)
SELECT 
    1 AS RoleId,
    m.Id AS ModuleId,
    m.Name,
    m.Path,
    m.Icon,
    m.[Order],
    1 AS HasAccess,
    NULL AS SubmoduleId,
    0 AS CanView,      -- A nivel módulo no tienen acciones específicas
    0 AS CanCreate,
    0 AS CanEdit,
    0 AS CanDelete,
    GETUTCDATE() AS CreatedAt,
    NULL AS CreatedByUserId
FROM Modules m
WHERE m.IsActive = 1;

DECLARE @ModulesCount INT = @@ROWCOUNT;
PRINT '? Permisos de módulos asignados: ' + CAST(@ModulesCount AS VARCHAR) + ' registros';
PRINT '';

-- ============================================
-- ASIGNAR PERMISOS A NIVEL SUBMÓDULO (TODOS)
-- ============================================

PRINT '?? Asignando permisos completos a nivel submódulo...';

INSERT INTO RoleModulePermissions (
    RoleId,
    ModuleId,
    Name,
    Path,
    Icon,
    [Order],
    HasAccess,
    SubmoduleId,
    CanView,
    CanCreate,
    CanEdit,
    CanDelete,
    CreatedAt,
    CreatedByUserId
)
SELECT 
    1 AS RoleId,
    s.ModuleId,
    m.Name,
    m.Path,
    m.Icon,
    m.[Order],
    1 AS HasAccess,
    s.Id AS SubmoduleId,
    1 AS CanView,      -- ? Acceso total
    1 AS CanCreate,    -- ? Acceso total
    1 AS CanEdit,      -- ? Acceso total
    1 AS CanDelete,    -- ? Acceso total
    GETUTCDATE() AS CreatedAt,
    NULL AS CreatedByUserId
FROM Submodules s
INNER JOIN Modules m ON m.Id = s.ModuleId
WHERE s.IsActive = 1 AND m.IsActive = 1;

DECLARE @SubmodulesCount INT = @@ROWCOUNT;
PRINT '? Permisos de submódulos asignados: ' + CAST(@SubmodulesCount AS VARCHAR) + ' registros';
PRINT '';

-- ============================================
-- VERIFICAR PERMISOS ASIGNADOS
-- ============================================

PRINT '?? RESUMEN DE PERMISOS ASIGNADOS:';
PRINT '==================================';
PRINT '';

-- Total de permisos
SELECT 
    COUNT(*) AS TotalPermisos
FROM RoleModulePermissions
WHERE RoleId = 1;

PRINT '';

-- Permisos por módulo
SELECT 
    m.Id AS ModuloId,
    m.Name AS Modulo,
    COUNT(CASE WHEN rmp.SubmoduleId IS NULL THEN 1 END) AS PermisoModulo,
    COUNT(CASE WHEN rmp.SubmoduleId IS NOT NULL THEN 1 END) AS PermisosSubmodulos,
    COUNT(rmp.Id) AS TotalPermisos,
    SUM(CASE WHEN rmp.CanView = 1 THEN 1 ELSE 0 END) AS CanView,
    SUM(CASE WHEN rmp.CanCreate = 1 THEN 1 ELSE 0 END) AS CanCreate,
    SUM(CASE WHEN rmp.CanEdit = 1 THEN 1 ELSE 0 END) AS CanEdit,
    SUM(CASE WHEN rmp.CanDelete = 1 THEN 1 ELSE 0 END) AS CanDelete
FROM Modules m
LEFT JOIN RoleModulePermissions rmp ON rmp.ModuleId = m.Id AND rmp.RoleId = 1
WHERE m.IsActive = 1
GROUP BY m.Id, m.Name
ORDER BY m.Id;

PRINT '';

-- ============================================
-- DETALLE DE SUBMÓDULOS CON PERMISOS
-- ============================================

PRINT '?? DETALLE DE PERMISOS POR SUBMÓDULO:';
PRINT '======================================';
PRINT '';

SELECT 
    m.Name AS Modulo,
    s.Name AS Submodulo,
    CASE WHEN rmp.CanView = 1 THEN '?' ELSE '?' END AS [View],
    CASE WHEN rmp.CanCreate = 1 THEN '?' ELSE '?' END AS [Create],
    CASE WHEN rmp.CanEdit = 1 THEN '?' ELSE '?' END AS [Edit],
    CASE WHEN rmp.CanDelete = 1 THEN '?' ELSE '?' END AS [Delete]
FROM Modules m
INNER JOIN Submodules s ON s.ModuleId = m.Id
LEFT JOIN RoleModulePermissions rmp ON rmp.ModuleId = m.Id 
    AND rmp.SubmoduleId = s.Id 
    AND rmp.RoleId = 1
WHERE m.IsActive = 1 AND s.IsActive = 1
ORDER BY m.Id, s.[Order];

PRINT '';

-- ============================================
-- RESUMEN FINAL
-- ============================================

DECLARE @TotalPermissions INT;
DECLARE @TotalModules INT;
DECLARE @TotalSubmodules INT;

SELECT @TotalPermissions = COUNT(*) FROM RoleModulePermissions WHERE RoleId = 1;
SELECT @TotalModules = COUNT(*) FROM Modules WHERE IsActive = 1;
SELECT @TotalSubmodules = COUNT(*) FROM Submodules WHERE IsActive = 1;

PRINT '? PERMISOS ASIGNADOS EXITOSAMENTE';
PRINT '===================================';
PRINT '';
PRINT '?? Estadísticas:';
PRINT '   - Rol: ' + @RoleName + ' (ID: 1)';
PRINT '   - Total de módulos: ' + CAST(@TotalModules AS VARCHAR);
PRINT '   - Total de submódulos: ' + CAST(@TotalSubmodules AS VARCHAR);
PRINT '   - Total de permisos creados: ' + CAST(@TotalPermissions AS VARCHAR);
PRINT '';
PRINT '?? Permisos asignados:';
PRINT '   - View (Ver): ? TODOS';
PRINT '   - Create (Crear): ? TODOS';
PRINT '   - Edit (Editar): ? TODOS';
PRINT '   - Delete (Eliminar): ? TODOS';
PRINT '';
PRINT '? El Rol Administrador ahora tiene acceso completo a todo el sistema';
PRINT '';

GO
