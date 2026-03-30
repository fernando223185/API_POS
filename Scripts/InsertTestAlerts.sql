-- =============================================================================
-- SCRIPT DE PRUEBA: Insertar alertas de ejemplo
-- Propósito: Visualizar el panel de alertas en el frontend antes de que
--            los jobs automáticos generen datos reales.
-- Cómo usar: Ejecutar en SQL Server Management Studio o Azure Data Studio
--            apuntando a la BD del proyecto.
-- Para limpiar después: ejecutar el bloque DELETE al final del archivo.
-- =============================================================================

-- Leer los IDs reales de la empresa y roles disponibles
DECLARE @CompanyId    INT = (SELECT TOP 1 Id FROM Companies ORDER BY Id)
DECLARE @BranchId     INT = (SELECT TOP 1 Id FROM Branches WHERE CompanyId = @CompanyId ORDER BY Id)
DECLARE @AdminRoleId  INT = (SELECT TOP 1 Id FROM Roles WHERE Name = 'Admin' ORDER BY Id)
DECLARE @SalesRoleId  INT = (SELECT TOP 1 Id FROM Roles WHERE Name != 'Admin' ORDER BY Id)
DECLARE @AnyUserId    INT = (SELECT TOP 1 Id FROM Users WHERE CompanyId = @CompanyId ORDER BY Id)
DECLARE @Now          DATETIME2 = GETUTCDATE()

-- Verificar que encontró datos
IF @CompanyId IS NULL
BEGIN
    RAISERROR ('No se encontró ninguna empresa en la tabla Companies.', 16, 1)
    RETURN
END

PRINT 'CompanyId:   ' + CAST(@CompanyId   AS VARCHAR)
PRINT 'BranchId:    ' + CAST(@BranchId    AS VARCHAR)
PRINT 'AdminRoleId: ' + ISNULL(CAST(@AdminRoleId AS VARCHAR), 'NULL')
PRINT 'SalesRoleId: ' + ISNULL(CAST(@SalesRoleId AS VARCHAR), 'NULL')
PRINT 'AnyUserId:   ' + ISNULL(CAST(@AnyUserId   AS VARCHAR), 'NULL')
PRINT '---'

-- =============================================================================
-- INSERTAR ALERTAS DE PRUEBA
-- UniqueKey usa el prefijo TEST_ para identificarlas y poder borrarlas fácil
-- =============================================================================

INSERT INTO Alerts
    (Type,          Severity,   ReferenceId, ReferenceType, CompanyId,  BranchId,  UserId,       TargetRoleId,  Title,                                       Message,                                                                                        UniqueKey,                   Status,     CreatedAt,                    ReadAt, ResolvedAt, LastDetectedAt)
VALUES

-- ── InvoiceDue · Critical (vencida hace 2 días) ──────────────────────────────
(   'InvoiceDue',  'Critical',  1001,        'Invoice',     @CompanyId, @BranchId, NULL,         @AdminRoleId,
    'Factura A-1001 vencida hace 2 días',
    'La factura A-1001 PPD tiene un saldo pendiente de $18,500.00 y venció el 28/03/2026. Se requiere acción inmediata.',
    'TEST_INVOICEDUE_1001_OVERDUE',       'Pending',  DATEADD(HOUR,-48,@Now),       NULL,   NULL,       DATEADD(HOUR,-6,@Now)   ),

-- ── InvoiceDue · Warning (vence en 1 día) ────────────────────────────────────
(   'InvoiceDue',  'Warning',   1002,        'Invoice',     @CompanyId, @BranchId, NULL,         @AdminRoleId,
    'Factura B-1002 vence mañana',
    'La factura B-1002 PPD tiene un saldo pendiente de $7,200.00 y vence el 31/03/2026.',
    'TEST_INVOICEDUE_1002_1DAY',          'Pending',  DATEADD(HOUR,-3,@Now),        NULL,   NULL,       DATEADD(HOUR,-3,@Now)   ),

-- ── InvoiceDue · Warning (vence en 3 días) ───────────────────────────────────
(   'InvoiceDue',  'Warning',   1003,        'Invoice',     @CompanyId, @BranchId, NULL,         @AdminRoleId,
    'Factura C-1003 vence en 3 días',
    'La factura C-1003 PPD tiene un saldo pendiente de $3,450.00 y vence el 02/04/2026.',
    'TEST_INVOICEDUE_1003_3DAYS',         'Pending',  DATEADD(HOUR,-1,@Now),        NULL,   NULL,       DATEADD(HOUR,-1,@Now)   ),

-- ── InvoiceDue · Info (vence en 7 días) — ya leída ───────────────────────────
(   'InvoiceDue',  'Info',      1004,        'Invoice',     @CompanyId, @BranchId, NULL,         @AdminRoleId,
    'Factura D-1004 vence en 7 días',
    'La factura D-1004 PPD tiene un saldo pendiente de $12,000.00 y vence el 06/04/2026.',
    'TEST_INVOICEDUE_1004_7DAYS',         'Read',     DATEADD(DAY,-1,@Now),         @Now,   NULL,       DATEADD(HOUR,-6,@Now)   ),

-- ── StockCritical · Critical (broadcast empresa) ─────────────────────────────
(   'StockCritical','Critical', 201,         'ProductStock',@CompanyId, @BranchId, NULL,         @SalesRoleId,
    'Stock crítico: Papel Carta 75g',
    'El producto "Papel Carta 75g Bond" tiene 5 unidades en el almacén Principal, por debajo del 50% del stock mínimo (50 u.).',
    'TEST_STOCKCRITICAL_PS_201',          'Pending',  DATEADD(MINUTE,-45,@Now),     NULL,   NULL,       DATEADD(MINUTE,-15,@Now) ),

-- ── StockCritical · Critical (dirigida a un usuario directo) ─────────────────
(   'StockCritical','Critical', 202,         'ProductStock',@CompanyId, @BranchId, @AnyUserId,   NULL,
    'Stock crítico: Tóner HP 85A',
    'El producto "Tóner HP 85A Negro" tiene 1 unidad en el almacén Norte, por debajo del 50% del stock mínimo (20 u.).',
    'TEST_STOCKCRITICAL_PS_202',          'Pending',  DATEADD(MINUTE,-20,@Now),     NULL,   NULL,       DATEADD(MINUTE,-5,@Now)  ),

-- ── StockMin · Warning ────────────────────────────────────────────────────────
(   'StockMin',    'Warning',   203,         'ProductStock',@CompanyId, @BranchId, NULL,         @SalesRoleId,
    'Stock mínimo: Engrapadora Bostitch B8',
    'El producto "Engrapadora Bostitch B8" tiene 12 unidades, igual al stock mínimo definido (12 u.). Considere reabastecer.',
    'TEST_STOCKMIN_PS_203',               'Pending',  DATEADD(HOUR,-2,@Now),        NULL,   NULL,       DATEADD(MINUTE,-30,@Now) ),

-- ── StockMin · Warning — ya leída ────────────────────────────────────────────
(   'StockMin',    'Warning',   204,         'ProductStock',@CompanyId, @BranchId, NULL,         @SalesRoleId,
    'Stock mínimo: Folders Manila Carta',
    'El producto "Folders Manila Carta" tiene 30 unidades, igual al stock mínimo definido (30 u.).',
    'TEST_STOCKMIN_PS_204',               'Read',     DATEADD(HOUR,-5,@Now),        DATEADD(HOUR,-4,@Now), NULL, DATEADD(HOUR,-1,@Now) ),

-- ── Broadcast empresa (sin UserId ni TargetRoleId) ───────────────────────────
(   'InvoiceDue',  'Critical',  1005,        'Invoice',     @CompanyId, @BranchId, NULL,         NULL,
    'Factura E-1005 vencida hace 5 días',
    'La factura E-1005 tiene un saldo de $55,000.00 y venció el 25/03/2026. Esta alerta es visible para todos los usuarios de la empresa.',
    'TEST_INVOICEDUE_1005_BROADCAST',     'Pending',  DATEADD(DAY,-2,@Now),         NULL,   NULL,       DATEADD(HOUR,-6,@Now)   )

SELECT 'Alertas insertadas correctamente. Total TEST alerts:' AS Resultado,
       COUNT(*) AS Cantidad
FROM Alerts
WHERE UniqueKey LIKE 'TEST_%'

-- =============================================================================
-- LIMPIEZA (ejecutar cuando ya no necesites las alertas de prueba)
-- =============================================================================
/*
DELETE FROM Alerts WHERE UniqueKey LIKE 'TEST_%'
SELECT 'Alertas de prueba eliminadas.' AS Resultado
*/
