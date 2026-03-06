-- ============================================
-- CREAR TABLA RoleModulePermissions
-- Sistema UNIFICADO de permisos por módulos/submódulos
-- ============================================

USE ERP;
GO

PRINT '?? Creando tabla RoleModulePermissions...';

-- Verificar si la tabla ya existe
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RoleModulePermissions' AND xtype='U')
BEGIN
    CREATE TABLE RoleModulePermissions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RoleId INT NOT NULL,
        ModuleId INT NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        Path NVARCHAR(200) NOT NULL,
        Icon NVARCHAR(50) NOT NULL,
        [Order] INT NOT NULL,
        HasAccess BIT NOT NULL DEFAULT 0,
        SubmoduleId INT NULL,  -- Puede ser NULL para permisos a nivel módulo
        CanView BIT NOT NULL DEFAULT 0,
        CanCreate BIT NOT NULL DEFAULT 0,
        CanEdit BIT NOT NULL DEFAULT 0,
        CanDelete BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        CreatedByUserId INT NULL,
        
        -- Foreign Keys
        CONSTRAINT FK_RoleModulePermissions_Roles FOREIGN KEY (RoleId) 
            REFERENCES Roles(Id) ON DELETE CASCADE,
        
        -- Índice único para evitar duplicados
        CONSTRAINT UQ_RoleModulePermissions_Role_Module_Submodule 
            UNIQUE (RoleId, ModuleId, SubmoduleId)
    );

    -- Índices para mejorar rendimiento
    CREATE INDEX IX_RoleModulePermissions_RoleId ON RoleModulePermissions(RoleId);
    CREATE INDEX IX_RoleModulePermissions_ModuleId ON RoleModulePermissions(ModuleId);
    CREATE INDEX IX_RoleModulePermissions_SubmoduleId ON RoleModulePermissions(SubmoduleId);

    PRINT '? Tabla RoleModulePermissions creada exitosamente';
END
ELSE
BEGIN
    PRINT '??  La tabla RoleModulePermissions ya existe';
END
GO

PRINT '';
PRINT '?? Verificando estructura de la tabla...';

SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'RoleModulePermissions'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT '? Script ejecutado exitosamente';
GO
