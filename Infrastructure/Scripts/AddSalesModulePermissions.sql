-- ========================================
-- SCRIPT: Agregar Módulo de Ventas y Permisos
-- Fecha: 2026-03-11
-- Descripción: Agrega el módulo de Ventas, sus submódulos 
--              y asigna permisos completos al rol Administrador
-- ========================================

BEGIN TRANSACTION;

PRINT '?? Iniciando configuración del módulo de Ventas...';

-- ========================================
-- VERIFICAR TABLAS REQUERIDAS
-- ========================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Modules' AND TABLE_SCHEMA = 'dbo')
BEGIN
    PRINT '? ERROR: Tabla Modules no existe';
    ROLLBACK TRANSACTION;
    RETURN;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Submodules' AND TABLE_SCHEMA = 'dbo')
BEGIN
    PRINT '? ERROR: Tabla Submodules no existe';
    ROLLBACK TRANSACTION;
    RETURN;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RoleModulePermissions' AND TABLE_SCHEMA = 'dbo')
BEGIN
    PRINT '? ERROR: Tabla RoleModulePermissions no existe';
    ROLLBACK TRANSACTION;
    RETURN;
END

-- ========================================
-- MÓDULO: VENTAS (ID: 2)
-- ========================================
IF NOT EXISTS (SELECT * FROM Modules WHERE Id = 2)
BEGIN
    PRINT '?? Creando módulo Ventas...';
    
    INSERT INTO Modules (Id, Name, Description, Path, Icon, [Order], IsActive, CreatedAt)
    VALUES (
        2,
        'Ventas',
        'Punto de venta y gestión de ventas',
        '/sales',
        'fa-shopping-cart',
        2,
        1,
        CURRENT_TIMESTAMP
    );
    
    PRINT '? Módulo Ventas creado';
END
ELSE
BEGIN
    PRINT '??  Módulo Ventas ya existe (ID: 2)';
END

-- ========================================
-- SUBMÓDULOS DE VENTAS
-- ========================================

-- Submódulo: Nueva Venta (ID: 10)
IF NOT EXISTS (SELECT * FROM Submodules WHERE Id = 10)
BEGIN
    PRINT '?? Creando submódulo Nueva Venta...';
    
    INSERT INTO Submodules (Id, ModuleId, Name, Description, Path, Icon, [Order], Color, IsActive, CreatedAt)
    VALUES (
        10,
        2,
        'Nueva Venta',
        'Punto de venta para crear nuevas ventas',
        '/sales/new',
        'fa-cash-register',
        1,
        '#10B981',
        1,
        CURRENT_TIMESTAMP
    );
    
    PRINT '? Submódulo Nueva Venta creado';
END
ELSE
BEGIN
    PRINT '??  Submódulo Nueva Venta ya existe (ID: 10)';
END

-- Submódulo: Lista de Ventas (ID: 11)
IF NOT EXISTS (SELECT * FROM Submodules WHERE Id = 11)
BEGIN
    PRINT '?? Creando submódulo Lista de Ventas...';
    
    INSERT INTO Submodules (Id, ModuleId, Name, Description, Path, Icon, [Order], Color, IsActive, CreatedAt)
    VALUES (
        11,
        2,
        'Lista de Ventas',
        'Ver historial y detalle de ventas realizadas',
        '/sales/list',
        'fa-list',
        2,
        '#3B82F6',
        1,
        CURRENT_TIMESTAMP
    );
    
    PRINT '? Submódulo Lista de Ventas creado';
END
ELSE
BEGIN
    PRINT '??  Submódulo Lista de Ventas ya existe (ID: 11)';
END

-- Submódulo: Reportes de Ventas (ID: 12)
IF NOT EXISTS (SELECT * FROM Submodules WHERE Id = 12)
BEGIN
    PRINT '?? Creando submódulo Reportes de Ventas...';
    
    INSERT INTO Submodules (Id, ModuleId, Name, Description, Path, Icon, [Order], Color, IsActive, CreatedAt)
    VALUES (
        12,
        2,
        'Reportes de Ventas',
        'Estadísticas y reportes de ventas',
        '/sales/reports',
        'fa-chart-line',
        3,
        '#8B5CF6',
        1,
        CURRENT_TIMESTAMP
    );
    
    PRINT '? Submódulo Reportes de Ventas creado';
END
ELSE
BEGIN
    PRINT '??  Submódulo Reportes de Ventas ya existe (ID: 12)';
END

-- Submódulo: Cobranza (ID: 13)
IF NOT EXISTS (SELECT * FROM Submodules WHERE Id = 13)
BEGIN
    PRINT '?? Creando submódulo Cobranza...';
    
    INSERT INTO Submodules (Id, ModuleId, Name, Description, Path, Icon, [Order], Color, IsActive, CreatedAt)
    VALUES (
        13,
        2,
        'Cobranza',
        'Gestión de pagos y cuentas por cobrar',
        '/sales/payments',
        'fa-money-bill-wave',
        4,
        '#F59E0B',
        1,
        CURRENT_TIMESTAMP
    );
    
    PRINT '? Submódulo Cobranza creado';
END
ELSE
BEGIN
    PRINT '??  Submódulo Cobranza ya existe (ID: 13)';
END

-- ========================================
-- ASIGNAR PERMISOS AL ROL ADMINISTRADOR (ID: 1)
-- ========================================

PRINT '';
PRINT '?? Asignando permisos al rol Administrador...';

-- Verificar que el rol Administrador existe
IF NOT EXISTS (SELECT * FROM Roles WHERE Id = 1)
BEGIN
    PRINT '? ERROR: Rol Administrador (ID: 1) no existe';
    PRINT '   Ejecute primero los scripts de inicialización de roles';
    ROLLBACK TRANSACTION;
    RETURN;
END

-- Permiso al módulo principal de Ventas
IF NOT EXISTS (SELECT * FROM RoleModulePermissions WHERE RoleId = 1 AND ModuleId = 2 AND SubmoduleId IS NULL)
BEGIN
    DECLARE @ModuleName NVARCHAR(100);
    DECLARE @ModulePath NVARCHAR(200);
    DECLARE @ModuleIcon NVARCHAR(50);
    DECLARE @ModuleOrder INT;
    
    SELECT @ModuleName = Name, @ModulePath = Path, @ModuleIcon = Icon, @ModuleOrder = [Order]
    FROM Modules WHERE Id = 2;
    
    INSERT INTO RoleModulePermissions (
        RoleId, ModuleId, SubmoduleId, Name, Path, Icon, [Order], 
        HasAccess, CanView, CanCreate, CanEdit, CanDelete, CreatedAt
    )
    VALUES (
        1, 2, NULL, @ModuleName, @ModulePath, @ModuleIcon, @ModuleOrder,
        1, 1, 1, 1, 1, CURRENT_TIMESTAMP
    );
    
    PRINT '? Permiso asignado al módulo Ventas';
END
ELSE
BEGIN
    PRINT '??  Permiso al módulo Ventas ya existe';
END

-- Permisos a submódulos de Ventas
DECLARE @SubmoduleId INT;
DECLARE @SubmoduleName NVARCHAR(200);
DECLARE @SubmodulePath NVARCHAR(200);
DECLARE @SubmoduleIcon NVARCHAR(50);
DECLARE @SubmoduleOrder INT;

-- Cursor para recorrer todos los submódulos de Ventas
DECLARE submodule_cursor CURSOR FOR
SELECT Id, Name, Path, Icon, [Order] FROM Submodules WHERE ModuleId = 2;

OPEN submodule_cursor;
FETCH NEXT FROM submodule_cursor INTO @SubmoduleId, @SubmoduleName, @SubmodulePath, @SubmoduleIcon, @SubmoduleOrder;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT * FROM RoleModulePermissions WHERE RoleId = 1 AND ModuleId = 2 AND SubmoduleId = @SubmoduleId)
    BEGIN
        INSERT INTO RoleModulePermissions (
            RoleId, ModuleId, SubmoduleId, Name, Path, Icon, [Order], 
            HasAccess, CanView, CanCreate, CanEdit, CanDelete, CreatedAt
        )
        VALUES (
            1, 2, @SubmoduleId, @SubmoduleName, @SubmodulePath, @SubmoduleIcon, @SubmoduleOrder,
            1, 1, 1, 1, 1, CURRENT_TIMESTAMP
        );
        
        PRINT '   ? Permiso asignado a submódulo: ' + @SubmoduleName;
    END
    ELSE
    BEGIN
        PRINT '   ??  Permiso a submódulo ' + @SubmoduleName + ' ya existe';
    END
    
    FETCH NEXT FROM submodule_cursor INTO @SubmoduleId, @SubmoduleName, @SubmodulePath, @SubmoduleIcon, @SubmoduleOrder;
END

CLOSE submodule_cursor;
DEALLOCATE submodule_cursor;

-- ========================================
-- VERIFICACIÓN FINAL
-- ========================================
DECLARE @ModuleCount INT;
DECLARE @SubmoduleCount INT;
DECLARE @PermissionCount INT;

SELECT @ModuleCount = COUNT(*) FROM Modules WHERE Id = 2;
SELECT @SubmoduleCount = COUNT(*) FROM Submodules WHERE ModuleId = 2;
SELECT @PermissionCount = COUNT(*) FROM RoleModulePermissions WHERE RoleId = 1 AND ModuleId = 2;

-- ========================================
-- COMMIT
-- ========================================
COMMIT TRANSACTION;

PRINT '';
PRINT '?? ====================================';
PRINT '? Módulo de Ventas configurado exitosamente';
PRINT '====================================';
PRINT '';
PRINT '?? Resumen:';
PRINT '   ? Módulo Ventas: ' + CAST(@ModuleCount AS NVARCHAR(10));
PRINT '   ? Submódulos creados: ' + CAST(@SubmoduleCount AS NVARCHAR(10));
PRINT '   ? Permisos asignados al Administrador: ' + CAST(@PermissionCount AS NVARCHAR(10));
PRINT '';
PRINT '?? Submódulos disponibles:';
PRINT '   1. Nueva Venta (POS)';
PRINT '   2. Lista de Ventas';
PRINT '   3. Reportes de Ventas';
PRINT '   4. Cobranza';
PRINT '';
PRINT '?? El módulo de Ventas está listo para usarse';
PRINT '';
