-- =============================================
-- Script COMPLETO para corregir TODOS los catálogos SAT
-- Usa DELETE + INSERT con prefijo N para garantizar correcta codificación
-- =============================================

USE [ERP]
GO

PRINT '==================================================='
PRINT 'CORRIGIENDO COMPLETAMENTE CATÁLOGOS SAT'
PRINT '==================================================='
PRINT ''

-- =====================================================
-- 1. USO DEL CFDI (c_UsoCFDI)
-- =====================================================
PRINT '1/7 - Corrigiendo Uso del CFDI...'
DELETE FROM [SatUsoCfdi];

INSERT INTO [SatUsoCfdi] (Codigo, Descripcion, AplicaPersonaFisica, AplicaPersonaMoral, FechaInicioVigencia, IsActive)
VALUES 
('G01', N'Adquisición de mercancías', 1, 1, '2022-01-01', 1),
('G02', N'Devoluciones, descuentos o bonificaciones', 1, 1, '2022-01-01', 1),
('G03', N'Gastos en general', 1, 1, '2022-01-01', 1),
('I01', N'Construcciones', 1, 1, '2022-01-01', 1),
('I02', N'Mobiliario y equipo de oficina por inversiones', 1, 1, '2022-01-01', 1),
('I03', N'Equipo de transporte', 1, 1, '2022-01-01', 1),
('I04', N'Equipo de cómputo y accesorios', 1, 1, '2022-01-01', 1),
('I05', N'Dados, troqueles, moldes, matrices y herramental', 1, 1, '2022-01-01', 1),
('I06', N'Comunicaciones telefónicas', 1, 1, '2022-01-01', 1),
('I07', N'Comunicaciones satelitales', 1, 1, '2022-01-01', 1),
('I08', N'Otra maquinaria y equipo', 1, 1, '2022-01-01', 1),
('D01', N'Honorarios médicos, dentales y gastos hospitalarios', 1, 0, '2022-01-01', 1),
('D02', N'Gastos médicos por incapacidad o discapacidad', 1, 0, '2022-01-01', 1),
('D03', N'Gastos funerales', 1, 0, '2022-01-01', 1),
('D04', N'Donativos', 1, 0, '2022-01-01', 1),
('D05', N'Intereses reales efectivamente pagados por créditos hipotecarios (casa habitación)', 1, 0, '2022-01-01', 1),
('D06', N'Aportaciones voluntarias al SAR', 1, 0, '2022-01-01', 1),
('D07', N'Primas por seguros de gastos médicos', 1, 0, '2022-01-01', 1),
('D08', N'Gastos de transportación escolar obligatoria', 1, 0, '2022-01-01', 1),
('D09', N'Depósitos en cuentas para el ahorro, primas que tengan como base planes de pensiones', 1, 0, '2022-01-01', 1),
('D10', N'Pagos por servicios educativos (colegiaturas)', 1, 0, '2022-01-01', 1),
('S01', N'Sin efectos fiscales', 1, 1, '2022-01-01', 1),
('CP01', N'Pagos', 1, 1, '2022-01-01', 1),
('CN01', N'Nómina', 1, 1, '2022-01-01', 1);
PRINT '   ✓ 24 registros insertados'

-- =====================================================
-- 2. RÉGIMEN FISCAL (c_RegimenFiscal)
-- =====================================================
PRINT '2/7 - Corrigiendo Régimen Fiscal...'
DELETE FROM [SatRegimenFiscal];

INSERT INTO [SatRegimenFiscal] (Codigo, Descripcion, AplicaPersonaFisica, AplicaPersonaMoral, FechaInicioVigencia, IsActive)
VALUES 
('601', N'General de Ley Personas Morales', 0, 1, '2022-01-01', 1),
('603', N'Personas Morales con Fines no Lucrativos', 0, 1, '2022-01-01', 1),
('605', N'Sueldos y Salarios e Ingresos Asimilados a Salarios', 1, 0, '2022-01-01', 1),
('606', N'Arrendamiento', 1, 0, '2022-01-01', 1),
('607', N'Régimen de Enajenación o Adquisición de Bienes', 1, 0, '2022-01-01', 1),
('608', N'Demás ingresos', 1, 0, '2022-01-01', 1),
('610', N'Residentes en el Extranjero sin Establecimiento Permanente en México', 1, 1, '2022-01-01', 1),
('611', N'Ingresos por Dividendos (socios y accionistas)', 1, 0, '2022-01-01', 1),
('612', N'Personas Físicas con Actividades Empresariales y Profesionales', 1, 0, '2022-01-01', 1),
('614', N'Ingresos por intereses', 1, 0, '2022-01-01', 1),
('615', N'Régimen de los ingresos por obtención de premios', 1, 0, '2022-01-01', 1),
('616', N'Sin obligaciones fiscales', 1, 0, '2022-01-01', 1),
('620', N'Sociedades Cooperativas de Producción que optan por diferir sus ingresos', 0, 1, '2022-01-01', 1),
('621', N'Incorporación Fiscal', 1, 0, '2022-01-01', 1),
('622', N'Actividades Agrícolas, Ganaderas, Silvícolas y Pesqueras', 0, 1, '2022-01-01', 1),
('623', N'Opcional para Grupos de Sociedades', 0, 1, '2022-01-01', 1),
('624', N'Coordinados', 0, 1, '2022-01-01', 1),
('625', N'Régimen de las Actividades Empresariales con ingresos a través de Plataformas Tecnológicas', 1, 0, '2022-01-01', 1),
('626', N'Régimen Simplificado de Confianza', 1, 1, '2022-01-01', 1);
PRINT '   ✓ 19 registros insertados'

-- =====================================================
-- 3. FORMA DE PAGO (c_FormaPago)
-- =====================================================
PRINT '3/7 - Corrigiendo Forma de Pago...'
DELETE FROM [SatFormaPago];

INSERT INTO [SatFormaPago] (Codigo, Descripcion, Bancarizado, FechaInicioVigencia, IsActive)
VALUES 
('01', N'Efectivo', NULL, '2017-01-01', 1),
('02', N'Cheque nominativo', N'Sí', '2017-01-01', 1),
('03', N'Transferencia electrónica de fondos', N'Sí', '2017-01-01', 1),
('04', N'Tarjeta de crédito', N'Sí', '2017-01-01', 1),
('05', N'Monedero electrónico', N'Sí', '2017-01-01', 1),
('06', N'Dinero electrónico', N'Sí', '2017-01-01', 1),
('08', N'Vales de despensa', NULL, '2017-01-01', 1),
('12', N'Dación en pago', NULL, '2017-01-01', 1),
('13', N'Pago por subrogación', NULL, '2017-01-01', 1),
('14', N'Pago por consignación', NULL, '2017-01-01', 1),
('15', N'Condonación', NULL, '2017-01-01', 1),
('17', N'Compensación', NULL, '2017-01-01', 1),
('23', N'Novación', NULL, '2017-01-01', 1),
('24', N'Confusión', NULL, '2017-01-01', 1),
('25', N'Remisión de deuda', NULL, '2017-01-01', 1),
('26', N'Prescripción o caducidad', NULL, '2017-01-01', 1),
('27', N'A satisfacción del acreedor', NULL, '2017-01-01', 1),
('28', N'Tarjeta de débito', N'Sí', '2017-01-01', 1),
('29', N'Tarjeta de servicios', N'Sí', '2017-01-01', 1),
('30', N'Aplicación de anticipos', NULL, '2017-01-01', 1),
('31', N'Intermediario pagos', NULL, '2017-01-01', 1),
('99', N'Por definir', NULL, '2017-01-01', 1);
PRINT '   ✓ 22 registros insertados'

-- =====================================================
-- 4. MÉTODO DE PAGO (c_MetodoPago)
-- =====================================================
PRINT '4/7 - Corrigiendo Método de Pago...'
DELETE FROM [SatMetodoPago];

INSERT INTO [SatMetodoPago] (Codigo, Descripcion, FechaInicioVigencia, IsActive)
VALUES 
('PUE', N'Pago en una sola exhibición', '2017-01-01', 1),
('PPD', N'Pago en parcialidades o diferido', '2017-01-01', 1);
PRINT '   ✓ 2 registros insertados'

-- =====================================================
-- 5. TIPO DE COMPROBANTE (c_TipoDeComprobante)  
-- =====================================================
PRINT '5/7 - Corrigiendo Tipo de Comprobante...'
DELETE FROM [SatTipoComprobante];

INSERT INTO [SatTipoComprobante] (Codigo, Descripcion, FechaInicioVigencia, IsActive)
VALUES 
('I', N'Ingreso', '2017-01-01', 1),
('E', N'Egreso', '2017-01-01', 1),
('T', N'Traslado', '2017-01-01', 1),
('N', N'Nómina', '2017-01-01', 1),
('P', N'Pago', '2017-01-01', 1);
PRINT '   ✓ 5 registros insertados'

-- =====================================================
-- 6. UNIDAD DE MEDIDA (c_ClaveUnidad)
-- =====================================================
PRINT '6/7 - Corrigiendo Unidad de Medida...'
DELETE FROM [SatUnidadMedida];

INSERT INTO [SatUnidadMedida] (ClaveUnidad, Nombre, Simbolo, FechaInicioVigencia, IsActive)
VALUES 
('H87', N'Pieza', N'Pieza', '2017-01-01', 1),
('EA', N'Elemento', N'Elemento', '2017-01-01', 1),
('E48', N'Unidad de servicio', N'Servicio', '2017-01-01', 1),
('ACT', N'Actividad', N'Actividad', '2017-01-01', 1),
('KGM', N'Kilogramo', N'kg', '2017-01-01', 1),
('GRM', N'Gramo', N'g', '2017-01-01', 1),
('LTR', N'Litro', N'l', '2017-01-01', 1),
('MTR', N'Metro', N'm', '2017-01-01', 1),
('MTK', N'Metro cuadrado', N'm²', '2017-01-01', 1),
('MTQ', N'Metro cúbico', N'm³', '2017-01-01', 1),
('XBX', N'Caja', N'Caja', '2017-01-01', 1),
('XPK', N'Paquete', N'Paquete', '2017-01-01', 1),
('HUR', N'Hora', N'hr', '2017-01-01', 1),
('DAY', N'Día', N'día', '2017-01-01', 1),
('MON', N'Mes', N'mes', '2017-01-01', 1),
('ANN', N'Año', N'año', '2017-01-01', 1),
('TNE', N'Tonelada', N't', '2017-01-01', 1),
('XUN', N'Unidad', N'Unidad', '2017-01-01', 1),
('SET', N'Conjunto', N'Conjunto', '2017-01-01', 1),
('BX', N'Caja base', N'Caja', '2017-01-01', 1);
PRINT '   ✓ 20 registros insertados'

-- =====================================================
-- 7. PRODUCTO/SERVICIO (c_ClaveProdServ)
-- =====================================================
PRINT '7/7 - Corrigiendo Producto/Servicio...'
DELETE FROM [SatProductoServicio];

INSERT INTO [SatProductoServicio] (ClaveProdServ, Descripcion, IncluyeIva, IncluyeIeps, FechaInicioVigencia, IsActive)
VALUES 
('01010101', N'No existe en el catálogo', N'No', N'No', '2017-01-01', 1),
('50202200', N'Café y té', N'Sí', N'No', '2017-01-01', 1),
('50202201', N'Café', N'Sí', N'No', '2017-01-01', 1),
('50202202', N'Té', N'Sí', N'No', '2017-01-01', 1),
('50161700', N'Productos de panadería', N'Sí', N'No', '2017-01-01', 1),
('50192300', N'Bebidas no alcohólicas', N'Sí', N'Sí', '2017-01-01', 1),
('50192304', N'Refrescos', N'Sí', N'Sí', '2017-01-01', 1),
('50192400', N'Bebidas alcohólicas', N'Sí', N'Sí', '2017-01-01', 1),
('43231500', N'Computadoras', N'Sí', N'No', '2017-01-01', 1),
('43211500', N'Equipo de cómputo', N'Sí', N'No', '2017-01-01', 1),
('81112200', N'Servicios de contabilidad', N'Sí', N'No', '2017-01-01', 1),
('81111500', N'Servicios legales', N'Sí', N'No', '2017-01-01', 1),
('80101600', N'Servicios de consultoría', N'Sí', N'No', '2017-01-01', 1),
('80111600', N'Servicios de publicidad', N'Sí', N'No', '2017-01-01', 1),
('76121500', N'Servicios de restaurante', N'Sí', N'No', '2017-01-01', 1),
('78101800', N'Servicios de transporte', N'Sí', N'No', '2017-01-01', 1),
('43211503', N'Laptops', N'Sí', N'No', '2017-01-01', 1),
('43211708', N'Monitores', N'Sí', N'No', '2017-01-01', 1),
('43211900', N'Impresoras', N'Sí', N'No', '2017-01-01', 1),
('43222600', N'Software', N'Sí', N'No', '2017-01-01', 1);
PRINT '   ✓ 20 registros insertados'

PRINT ''
PRINT '==================================================='
PRINT 'CORRECCIÓN COMPLETADA - RESUMEN:'
PRINT '==================================================='
SELECT 'Uso del CFDI' AS Catalogo, COUNT(*) AS Total FROM [SatUsoCfdi]
UNION ALL
SELECT 'Régimen Fiscal', COUNT(*) FROM [SatRegimenFiscal]
UNION ALL
SELECT 'Forma de Pago', COUNT(*) FROM [SatFormaPago]
UNION ALL
SELECT 'Método de Pago', COUNT(*) FROM [SatMetodoPago]
UNION ALL
SELECT 'Tipo de Comprobante', COUNT(*) FROM [SatTipoComprobante]
UNION ALL
SELECT 'Unidad de Medida', COUNT(*) FROM [SatUnidadMedida]
UNION ALL
SELECT 'Producto/Servicio', COUNT(*) FROM [SatProductoServicio];

GO
