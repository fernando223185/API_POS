-- =============================================
-- Script: Actualizar tabla Customer - Campos ERP Avanzados
-- Descripción: Agregar campos fiscales y comerciales para ERP profesional
-- Fecha: $(fecha)
-- =============================================

USE [ERP]
GO

-- Agregar nuevas columnas a la tabla Customer
ALTER TABLE [dbo].[Customer]
ADD 
    -- Información fiscal para CFDI
    [CompanyName] NVARCHAR(200) NULL,                    -- Razón social
    [SatTaxRegime] NVARCHAR(10) NULL,                    -- Régimen fiscal SAT (601, 603, etc.)
    [SatCfdiUse] NVARCHAR(10) NULL DEFAULT 'G03',       -- Uso del CFDI (G03 por defecto)
    
    -- Configuraciones comerciales
    [PriceListId] INT NULL,                              -- Lista de precios asignada
    [DiscountPercentage] DECIMAL(5,4) NULL DEFAULT 0,   -- Porcentaje de descuento
    [CreditLimit] DECIMAL(18,2) NULL DEFAULT 0,         -- Límite de crédito
    [PaymentTermsDays] INT NULL DEFAULT 0,               -- Días de crédito
    
    -- Control y auditoría
    [IsActive] BIT NULL DEFAULT 1,                       -- Estado activo/inactivo
    [CreatedAt] DATETIME2(7) NULL DEFAULT GETUTCDATE(), -- Fecha de creación (nuevo formato)
    [UpdatedAt] DATETIME2(7) NULL,                       -- Fecha de actualización
    [CreatedByUserId] INT NULL,                          -- Usuario que creó
    [UpdatedByUserId] INT NULL                           -- Usuario que actualizó
GO

-- Actualizar valores por defecto para registros existentes
UPDATE [dbo].[Customer] 
SET 
    [SatCfdiUse] = 'G03',
    [DiscountPercentage] = 0,
    [CreditLimit] = 0,
    [PaymentTermsDays] = 0,
    [IsActive] = 1,
    [CreatedAt] = [Created_at]  -- Copiar fecha existente al nuevo campo
WHERE [SatCfdiUse] IS NULL
GO

-- Crear índices para mejorar rendimiento
CREATE NONCLUSTERED INDEX [IX_Customer_IsActive] 
ON [dbo].[Customer] ([IsActive])
GO

CREATE NONCLUSTERED INDEX [IX_Customer_PriceListId] 
ON [dbo].[Customer] ([PriceListId])
GO

CREATE NONCLUSTERED INDEX [IX_Customer_TaxId] 
ON [dbo].[Customer] ([TaxId])
GO

-- Agregar restricciones (constraints)
ALTER TABLE [dbo].[Customer]
ADD CONSTRAINT [CK_Customer_DiscountPercentage] 
CHECK ([DiscountPercentage] >= 0 AND [DiscountPercentage] <= 100)
GO

ALTER TABLE [dbo].[Customer]
ADD CONSTRAINT [CK_Customer_CreditLimit] 
CHECK ([CreditLimit] >= 0)
GO

ALTER TABLE [dbo].[Customer]
ADD CONSTRAINT [CK_Customer_PaymentTermsDays] 
CHECK ([PaymentTermsDays] >= 0 AND [PaymentTermsDays] <= 365)
GO

-- =============================================
-- Verificar los cambios
-- =============================================
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Customer'
ORDER BY ORDINAL_POSITION
GO

PRINT 'Tabla Customer actualizada exitosamente con campos ERP avanzados'
GO