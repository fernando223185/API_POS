-- ================================================================
-- ASIGNAR PERMISO ManageModules AL ROL ADMIN
-- Permite crear, editar y eliminar módulos y submódulos
-- ================================================================

USE [API_POS_DB];  -- Cambiar por tu base de datos
GO

PRINT '?? Asignando permiso ManageModules al rol Admin...';
GO

BEGIN TRANSACTION;

BEGIN TRY
    -- ========================================
    -- 1. OBTENER IDS
    -- ========================================
    DECLARE @RoleId INT;
    DECLARE @ModuleId INT;
    DECLARE @SubmoduleId INT;

    -- Obtener ID del rol Admin (corregido: usar 'Name' en lugar de 'RoleName')
    SELECT @RoleId = Id FROM Roles WHERE Name = 'Administrador';
    
    IF @RoleId IS NULL
    BEGIN
        PRINT '? ERROR: Rol Administrador no encontrado';
        ROLLBACK TRANSACTION;
        RETURN;
    END

    PRINT '? Rol Administrador encontrado - ID: ' + CAST(@RoleId AS VARCHAR(10));

    -- Obtener ID del módulo Configuration (CORREGIDO: 'Configuración' con acento)
    SELECT @ModuleId = Id FROM Modules WHERE Name = 'Configuración';
    
    IF @ModuleId IS NULL
    BEGIN
        PRINT '? ERROR: Módulo Configuración no encontrado';
        ROLLBACK TRANSACTION;
        RETURN;
    END

    PRINT '? Módulo Configuración encontrado - ID: ' + CAST(@ModuleId AS VARCHAR(10));

    -- ========================================
    -- 2. CREAR SUBMÓDULO SI NO EXISTE
    -- ========================================
    PRINT '';
    PRINT '?? Verificando si existe submódulo de Gestión de Módulos...';
    
    -- Obtener ID del submódulo de Gestión de Módulos
    SELECT @SubmoduleId = Id 
    FROM Submodules 
    WHERE ModuleId = @ModuleId 
    AND Name LIKE '%Módulo%';

    IF @SubmoduleId IS NOT NULL
    BEGIN
        PRINT '? Submódulo Gestión de Módulos encontrado - ID: ' + CAST(@SubmoduleId AS VARCHAR(10));
    END
    ELSE
    BEGIN
        PRINT '??  Submódulo Gestión de Módulos no encontrado';
        PRINT '? Creando submódulo Gestión de Módulos...';
        
        -- Obtener el próximo ID disponible para el submódulo
        DECLARE @NextSubmoduleId INT;
        SELECT @NextSubmoduleId = ISNULL(MAX(Id), 86) + 1 FROM Submodules WHERE ModuleId = @ModuleId;
        
        -- Insertar el nuevo submódulo
        INSERT INTO Submodules (Id, ModuleId, Name, Description, Path, Icon, [Order], Color, IsActive, CreatedAt)
        VALUES (
            @NextSubmoduleId,                              -- Id
            @ModuleId,                                     -- ModuleId (Configuración)
            'Gestión de Módulos',                          -- Name
            'Crear, editar y eliminar módulos y submódulos del sistema', -- Description
            '/config/modules',                             -- Path
            'faThLarge',                                   -- Icon
            7,                                             -- Order (después de Apariencia)
            'from-indigo-500 to-blue-600',                 -- Color
            1,                                             -- IsActive
            GETDATE()                                      -- CreatedAt
        );
        
        SET @SubmoduleId = @NextSubmoduleId;
        PRINT '? Submódulo Gestión de Módulos creado exitosamente - ID: ' + CAST(@SubmoduleId AS VARCHAR(10));
    END

    -- ========================================
    -- 3. VERIFICAR SI YA EXISTE EL PERMISO
    -- ========================================
    PRINT '';
    PRINT '?? Verificando permisos existentes...';
    
    IF EXISTS (
        SELECT 1 
        FROM RoleModulePermissions 
        WHERE RoleId = @RoleId 
        AND ModuleId = @ModuleId 
        AND SubmoduleId = @SubmoduleId
    )
    BEGIN
        PRINT '??  El permiso para Gestión de Módulos ya existe para el rol Administrador';
        PRINT '   Actualizando permisos...';
        
        UPDATE RoleModulePermissions
        SET 
            HasAccess = 1,
            CanView = 1,
            CanCreate = 1,
            CanEdit = 1,
            CanDelete = 1,
            UpdatedAt = GETDATE()
        WHERE 
            RoleId = @RoleId 
            AND ModuleId = @ModuleId 
            AND SubmoduleId = @SubmoduleId;
        
        PRINT '? Permiso actualizado exitosamente';
    END
    ELSE
    BEGIN
        -- ========================================
        -- 4. INSERTAR PERMISO
        -- ========================================
        PRINT '? Insertando nuevo permiso para Gestión de Módulos...';
        
        -- Obtener información del submódulo para insertar
        DECLARE @Name NVARCHAR(100);
        DECLARE @Path NVARCHAR(200);
        DECLARE @Icon NVARCHAR(50);
        DECLARE @Order INT;
        
        SELECT 
            @Name = Name,
            @Path = Path,
            @Icon = Icon,
            @Order = [Order]
        FROM Submodules 
        WHERE Id = @SubmoduleId;
        
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
            @RoleId,           -- RoleId (Administrador)
            @ModuleId,         -- ModuleId (Configuración)
            @SubmoduleId,      -- SubmoduleId
            @Name,             -- Name
            @Path,             -- Path
            @Icon,             -- Icon
            @Order,            -- Order
            1,                 -- HasAccess
            1,                 -- CanView
            1,                 -- CanCreate
            1,                 -- CanEdit
            1,                 -- CanDelete
            GETDATE()          -- CreatedAt
        );
        
        PRINT '? Permiso creado exitosamente';
    END

    -- ========================================
    -- 5. VERIFICACIÓN FINAL
    -- ========================================
    PRINT '';
    PRINT '?? Verificación de permisos asignados:';
    PRINT '======================================';
    
    SELECT 
        r.Name as RoleName,
        m.Name as ModuleName,
        sm.Name as SubmoduleName,
        rmp.Name as PermissionName,
        rmp.HasAccess,
        rmp.CanView,
        rmp.CanCreate,
        rmp.CanEdit,
        rmp.CanDelete
    FROM RoleModulePermissions rmp
    INNER JOIN Roles r ON r.Id = rmp.RoleId
    INNER JOIN Modules m ON m.Id = rmp.ModuleId
    LEFT JOIN Submodules sm ON sm.Id = rmp.SubmoduleId
    WHERE rmp.RoleId = @RoleId 
    AND rmp.ModuleId = @ModuleId
    AND rmp.SubmoduleId = @SubmoduleId;

    COMMIT TRANSACTION;
    
    PRINT '';
    PRINT '? ¡Permiso asignado exitosamente!';
    PRINT '';
    PRINT '?? El rol Administrador ahora puede:';
    PRINT '   ? Ver módulos y submódulos';
    PRINT '   ? Crear módulos y submódulos';
    PRINT '   ? Editar módulos y submódulos';
    PRINT '   ? Eliminar módulos y submódulos';
    PRINT '';
    PRINT '?? Endpoints disponibles:';
    PRINT '   POST   /api/modules';
    PRINT '   POST   /api/modules/submodules';
    PRINT '   PUT    /api/modules/{id}';
    PRINT '   PUT    /api/modules/submodules/{id}';
    PRINT '   DELETE /api/modules/{id}';
    PRINT '   DELETE /api/modules/submodules/{id}';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    PRINT '';
    PRINT '? ERROR al asignar permiso:';
    PRINT '   Mensaje: ' + ERROR_MESSAGE();
    PRINT '   Línea: ' + CAST(ERROR_LINE() AS VARCHAR(10));
    PRINT '   Procedimiento: ' + ISNULL(ERROR_PROCEDURE(), 'N/A');
    
    -- Re-lanzar el error
    THROW;
END CATCH
GO

PRINT '';
PRINT '?? Script ejecutado: ' + CONVERT(VARCHAR(23), GETDATE(), 121);
GO
