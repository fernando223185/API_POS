-- ========================================
-- AGREGAR PERMISOS DE COMPANIES AL ROL ADMINISTRADOR
-- ========================================
-- Este script inserta permisos para el módulo de Empresas (Companies)
-- en el rol Administrador (ID: 1)
-- 
-- IMPORTANTE: El controlador usa [RequirePermission("Configuration", "Empresas")]
-- Por lo tanto, el permiso debe llamarse exactamente "Empresas"

USE [ERP]; -- ?? CAMBIA POR EL NOMBRE DE TU BASE DE DATOS
GO

PRINT '========================================';
PRINT '?? CONFIGURANDO PERMISOS DE EMPRESAS';
PRINT '========================================';
PRINT '';

-- ========================================
-- 1. VERIFICAR QUE EXISTE EL MÓDULO "Configuration"
-- ========================================
DECLARE @ConfigModuleId INT;

SELECT @ConfigModuleId = Id 
FROM [Modules] 
WHERE Name = 'Configuration' OR Name = 'Configuración';

IF @ConfigModuleId IS NULL
BEGIN
    PRINT '? ERROR: No se encontró el módulo "Configuration"';
    PRINT '??  Ejecuta primero el script de creación de módulos';
    RETURN;
END

PRINT '? Módulo "Configuration" encontrado (ID: ' + CAST(@ConfigModuleId AS VARCHAR) + ')';

-- ========================================
-- 2. VERIFICAR/CREAR PERMISO A NIVEL MÓDULO
-- ========================================
IF NOT EXISTS (
    SELECT 1 
    FROM [RoleModulePermissions]
    WHERE RoleId = 1 
      AND ModuleId = @ConfigModuleId
      AND SubmoduleId IS NULL
)
BEGIN
    INSERT INTO [RoleModulePermissions] (
        RoleId, ModuleId, Name, Path, Icon, [Order],
        HasAccess, SubmoduleId,
        CanView, CanCreate, CanEdit, CanDelete,
        CreatedAt, CreatedByUserId
    )
    SELECT 
        1, Id, Name, Path, Icon, [Order],
        1, NULL,
        0, 0, 0, 0,
        GETUTCDATE(), 1
    FROM [Modules]
    WHERE Id = @ConfigModuleId;

    PRINT '? Permiso de módulo "Configuration" creado';
END
ELSE
BEGIN
    PRINT '??  Permiso de módulo "Configuration" ya existe';
END

-- ========================================
-- 3. BUSCAR O CREAR SUBMÓDULO "Empresas"
-- ========================================
DECLARE @SubmoduleId INT;

SELECT @SubmoduleId = Id
FROM [Submodules]
WHERE ModuleId = @ConfigModuleId
  AND Name = 'Empresas';  -- ?? IMPORTANTE: Nombre exacto en la BD

IF @SubmoduleId IS NULL
BEGIN
    PRINT '??  Submódulo "Empresas" no encontrado, creándolo...';
    
    DECLARE @NextOrder INT;
    SELECT @NextOrder = ISNULL(MAX([Order]), 0) + 1
    FROM [Submodules]
    WHERE ModuleId = @ConfigModuleId;

    INSERT INTO [Submodules] (
        ModuleId, Name, Description, Path, Icon, [Order], Color, IsActive, CreatedAt
    )
    VALUES (
        @ConfigModuleId,
        'Empresas',  -- ?? IMPORTANTE: Debe coincidir con RequirePermission
        'Gestión de empresas y configuración multi-empresa',
        '/config/companies',
        'faBuilding',
        @NextOrder,
        '#34495e',
        1,
        GETUTCDATE()
    );

    SET @SubmoduleId = SCOPE_IDENTITY();
    PRINT '? Submódulo "Empresas" creado (ID: ' + CAST(@SubmoduleId AS VARCHAR) + ')';
END
ELSE
BEGIN
    PRINT '? Submódulo "Empresas" encontrado (ID: ' + CAST(@SubmoduleId AS VARCHAR) + ')';
END

-- ========================================
-- 4. ELIMINAR PERMISOS ANTERIORES (LIMPIEZA)
-- ========================================
DELETE FROM [RoleModulePermissions]
WHERE RoleId = 1
  AND ModuleId = @ConfigModuleId
  AND SubmoduleId = @SubmoduleId;

PRINT '?? Permisos anteriores eliminados';

-- ========================================
-- 5. INSERTAR PERMISOS COMPLETOS
-- ========================================
INSERT INTO [RoleModulePermissions] (
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
    m.Name AS Name,
    m.Path AS Path,
    m.Icon AS Icon,
    m.[Order] AS [Order],
    1 AS HasAccess,       -- ? Tiene acceso al submódulo
    @SubmoduleId AS SubmoduleId,
    1 AS CanView,         -- ? Puede Ver (GET)
    1 AS CanCreate,       -- ? Puede Crear (POST)
    1 AS CanEdit,         -- ? Puede Editar (PUT)
    1 AS CanDelete,       -- ? Puede Eliminar (DELETE/Deactivate)
    GETUTCDATE() AS CreatedAt,
    1 AS CreatedByUserId
FROM [Modules] m
WHERE m.Id = @ConfigModuleId;

PRINT '? Permisos completos insertados';
PRINT '';

-- ========================================
-- 6. VERIFICAR RESULTADO
-- ========================================
PRINT '?? VERIFICACIÓN DE PERMISOS:';
PRINT '-----------------------------------';

SELECT 
    '? Rol: ' + r.Name AS Detalle,
    'Módulo: ' + rmp.Name AS Modulo,
    'Submódulo: ' + ISNULL(s.Name, 'N/A') AS Submodulo,
    CASE WHEN rmp.CanView = 1 THEN '? View' ELSE '? View' END AS PermisoView,
    CASE WHEN rmp.CanCreate = 1 THEN '? Create' ELSE '? Create' END AS PermisoCreate,
    CASE WHEN rmp.CanEdit = 1 THEN '? Edit' ELSE '? Edit' END AS PermisoEdit,
    CASE WHEN rmp.CanDelete = 1 THEN '? Delete' ELSE '? Delete' END AS PermisoDelete
FROM [RoleModulePermissions] rmp
INNER JOIN [Roles] r ON r.Id = rmp.RoleId
LEFT JOIN [Submodules] s ON s.Id = rmp.SubmoduleId
WHERE rmp.RoleId = 1
  AND rmp.ModuleId = @ConfigModuleId
  AND (rmp.SubmoduleId = @SubmoduleId OR rmp.SubmoduleId IS NULL);

PRINT '';
PRINT '========================================';
PRINT '? CONFIGURACIÓN COMPLETADA';
PRINT '========================================';
PRINT 'Rol Administrador (ID: 1) ahora tiene acceso a:';
PRINT '';
PRINT '?? Endpoints habilitados:';
PRINT '   GET    /api/companies              ?';
PRINT '   GET    /api/companies/{id}         ?';
PRINT '   GET    /api/companies/active       ?';
PRINT '   GET    /api/companies/main         ?';
PRINT '   POST   /api/companies              ?';
PRINT '   PUT    /api/companies/{id}         ?';
PRINT '   PUT    /api/companies/{id}/deactivate  ?';
PRINT '   PUT    /api/companies/{id}/reactivate  ?';
PRINT '   PUT    /api/companies/{id}/fiscal-config  ?';
PRINT '';
PRINT '?? Prueba con: ADMIN001 / admin123';
PRINT '========================================';
GO
