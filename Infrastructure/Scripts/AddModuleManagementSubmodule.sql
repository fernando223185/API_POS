-- ============================================
-- AGREGAR SUBMÓDULO: GESTIÓN DE MÓDULOS
-- Submódulo para administrar módulos y submódulos del sistema
-- ============================================

USE ERP;
GO

PRINT '?? Agregando submódulo de Gestión de Módulos...';
PRINT '================================================';
PRINT '';

-- ============================================
-- VERIFICAR SI EL MÓDULO DE CONFIGURACIÓN EXISTE
-- ============================================

IF NOT EXISTS (SELECT 1 FROM Modules WHERE Id = 8 AND Name = 'Configuración')
BEGIN
    PRINT '? ERROR: El módulo de Configuración (ID: 8) no existe.';
    PRINT '   Ejecuta primero el script SeedSystemModules.sql';
    RETURN;
END

PRINT '? Módulo de Configuración encontrado (ID: 8)';
PRINT '';

-- ============================================
-- ELIMINAR SUBMÓDULO SI YA EXISTE
-- ============================================

IF EXISTS (SELECT 1 FROM Submodules WHERE Id = 85 AND ModuleId = 8)
BEGIN
    DELETE FROM Submodules WHERE Id = 85 AND ModuleId = 8;
    PRINT '???  Submódulo existente eliminado (ID: 85)';
END

-- ============================================
-- INSERTAR NUEVO SUBMÓDULO
-- ============================================

INSERT INTO Submodules (
    Id,
    ModuleId,
    Name,
    Description,
    Path,
    Icon,
    [Order],
    Color,
    IsActive,
    CreatedAt
)
VALUES (
    85,                                     -- ID del submódulo
    8,                                      -- ModuleId (Configuración)
    'Gestión de Módulos',                  -- Nombre
    'Crear, editar y eliminar módulos y submódulos del sistema', -- Descripción
    '/config/modules',                      -- Ruta
    'faCubes',                             -- Icono FontAwesome
    5,                                      -- Orden (después de Permisos Personalizados)
    'from-purple-500 to-pink-600',         -- Color del gradiente
    1,                                      -- IsActive (activo)
    GETUTCDATE()                           -- Fecha de creación
);

PRINT '? Submódulo creado: Gestión de Módulos (ID: 85)';
PRINT '';

-- ============================================
-- ASIGNAR PERMISOS AL ROL ADMINISTRADOR
-- ============================================

PRINT '?? Asignando permisos completos al rol Administrador...';

-- Eliminar permiso existente si hay
DELETE FROM RoleModulePermissions 
WHERE RoleId = 1 AND ModuleId = 8 AND SubmoduleId = 85;

-- Insertar permiso completo
INSERT INTO RoleModulePermissions (
    RoleId,
    ModuleId,
    SubmoduleId,
    Name,
    Path,
    Icon,
    [Order],
    HasAccess,
    CanView,
    CanCreate,
    CanEdit,
    CanDelete,
    CreatedAt
)
VALUES (
    1,                              -- RoleId (Administrador)
    8,                              -- ModuleId (Configuración)
    85,                             -- SubmoduleId (Gestión de Módulos)
    'Gestión de Módulos',          -- Nombre
    '/config/modules',              -- Ruta
    'faCubes',                      -- Icono
    5,                              -- Orden
    1,                              -- HasAccess
    1,                              -- CanView
    1,                              -- CanCreate
    1,                              -- CanEdit
    1,                              -- CanDelete
    GETUTCDATE()                   -- CreatedAt
);

PRINT '? Permisos asignados al Administrador (View, Create, Edit, Delete)';
PRINT '';

-- ============================================
-- VERIFICAR RESULTADO
-- ============================================

PRINT '?? RESUMEN:';
PRINT '===========';
PRINT '';

-- Mostrar submódulo creado
SELECT 
    s.Id AS SubmoduleId,
    m.Name AS Module,
    s.Name AS Submodule,
    s.Description,
    s.Path,
    s.Icon,
    s.[Order],
    s.Color,
    s.IsActive,
    s.CreatedAt
FROM Submodules s
INNER JOIN Modules m ON m.Id = s.ModuleId
WHERE s.Id = 85 AND s.ModuleId = 8;

PRINT '';

-- Mostrar permisos del administrador
SELECT 
    rmp.RoleId,
    r.Name AS RoleName,
    m.Name AS Module,
    s.Name AS Submodule,
    CASE WHEN rmp.CanView = 1 THEN '?' ELSE '?' END AS [View],
    CASE WHEN rmp.CanCreate = 1 THEN '?' ELSE '?' END AS [Create],
    CASE WHEN rmp.CanEdit = 1 THEN '?' ELSE '?' END AS [Edit],
    CASE WHEN rmp.CanDelete = 1 THEN '?' ELSE '?' END AS [Delete]
FROM RoleModulePermissions rmp
INNER JOIN Roles r ON r.Id = rmp.RoleId
INNER JOIN Modules m ON m.Id = rmp.ModuleId
LEFT JOIN Submodules s ON s.Id = rmp.SubmoduleId AND s.ModuleId = rmp.ModuleId
WHERE rmp.RoleId = 1 AND rmp.ModuleId = 8 AND rmp.SubmoduleId = 85;

PRINT '';
PRINT '? SUBMÓDULO AGREGADO EXITOSAMENTE';
PRINT '';
PRINT '?? Detalles:';
PRINT '   - Módulo: Configuración (ID: 8)';
PRINT '   - Submódulo: Gestión de Módulos (ID: 85)';
PRINT '   - Ruta: /config/modules';
PRINT '   - Icono: faCubes';
PRINT '   - Permisos Administrador: View ? Create ? Edit ? Delete ?';
PRINT '';
PRINT '?? Endpoint disponible:';
PRINT '   GET  /api/system/modules';
PRINT '   POST /api/system/modules';
PRINT '   PUT  /api/system/modules/{id}';
PRINT '   DELETE /api/system/modules/{id}';
PRINT '';

GO
