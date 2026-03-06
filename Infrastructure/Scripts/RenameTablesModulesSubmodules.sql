-- ============================================
-- SCRIPT PARA RENOMBRAR TABLAS
-- AppModules ? Modules
-- AppSubmodules ? Submodules
-- ============================================

USE ERP;
GO

PRINT '?? Iniciando proceso de renombrado de tablas...';

-- 1. Eliminar tabla anterior de Modules (sistema de permisos antiguo) si existe
IF OBJECT_ID('Permissions', 'U') IS NOT NULL
BEGIN
    -- Verificar si existe la columna ModuleId en Permissions
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Permissions') AND name = 'ModuleId')
    BEGIN
        PRINT '??  Eliminando relación Permissions->Modules antigua...';
        
        -- Eliminar FK constraint si existe
        DECLARE @ConstraintName NVARCHAR(200);
        SELECT @ConstraintName = name 
        FROM sys.foreign_keys 
        WHERE parent_object_id = OBJECT_ID('Permissions') 
        AND referenced_object_id = OBJECT_ID('Modules');
        
        IF @ConstraintName IS NOT NULL
        BEGIN
            EXEC('ALTER TABLE Permissions DROP CONSTRAINT ' + @ConstraintName);
            PRINT '? FK Constraint eliminada';
        END
        
        -- Eliminar columna ModuleId de Permissions
        ALTER TABLE Permissions DROP COLUMN ModuleId;
        PRINT '? Columna ModuleId eliminada de Permissions';
    END
END
GO

-- 2. Eliminar tabla Modules antigua (sistema de permisos)
IF OBJECT_ID('Modules', 'U') IS NOT NULL
BEGIN
    PRINT '???  Eliminando tabla Modules antigua (sistema de permisos)...';
    DROP TABLE Modules;
    PRINT '? Tabla Modules antigua eliminada';
END
GO

-- 3. Eliminar tablas AppModules y AppSubmodules si existen
IF OBJECT_ID('AppSubmodules', 'U') IS NOT NULL
BEGIN
    PRINT '???  Eliminando tabla AppSubmodules...';
    DROP TABLE AppSubmodules;
    PRINT '? Tabla AppSubmodules eliminada';
END
GO

IF OBJECT_ID('AppModules', 'U') IS NOT NULL
BEGIN
    PRINT '???  Eliminando tabla AppModules...';
    DROP TABLE AppModules;
    PRINT '? Tabla AppModules eliminada';
END
GO

-- 4. Crear nuevas tablas Modules y Submodules
PRINT '?? Creando nuevas tablas Modules y Submodules...';

CREATE TABLE Modules (
    Id INT NOT NULL PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Path NVARCHAR(200) NOT NULL,
    Icon NVARCHAR(50) NOT NULL,
    [Order] INT NOT NULL,
    IsActive BIT NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL
);

CREATE INDEX IX_Modules_IsActive ON Modules (IsActive);
CREATE INDEX IX_Modules_Order ON Modules ([Order]);

PRINT '? Tabla Modules creada';

CREATE TABLE Submodules (
    Id INT NOT NULL PRIMARY KEY,
    ModuleId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Path NVARCHAR(200) NOT NULL,
    Icon NVARCHAR(50) NOT NULL,
    [Order] INT NOT NULL,
    Color NVARCHAR(100) NULL,
    IsActive BIT NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Submodules_Modules FOREIGN KEY (ModuleId) REFERENCES Modules(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Submodules_IsActive ON Submodules (IsActive);
CREATE INDEX IX_Submodules_ModuleId ON Submodules (ModuleId);
CREATE INDEX IX_Submodules_Order ON Submodules ([Order]);

PRINT '? Tabla Submodules creada';
GO

PRINT '? Proceso de renombrado completado exitosamente';
PRINT '?? Ahora puedes ejecutar el script de Seed Data';
GO
