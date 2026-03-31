-- Script para corregir datos con encoding incorrecto
USE ERP;

-- Actualizar PriceList con encoding correcto
UPDATE PriceLists 
SET Name = N'Público General',
    Description = N'Precios para público en general'
WHERE Code = 'PUB';

-- Actualizar categorías con tildes correctas  
UPDATE ProductCategories 
SET Name = N'Alimentación',
    Description = N'Productos alimenticios'
WHERE Code = 'ALIM';

UPDATE ProductCategories
SET Name = N'Artículos para el hogar',
    Description = N'Artículos para el hogar'
WHERE Code = 'HOGAR';

-- Verificar cambios
SELECT 'PriceLists updated:' as Message;
SELECT Name, Code, Description FROM PriceLists WHERE Code = 'PUB';

SELECT 'Categories updated:' as Message;
SELECT Name, Code, Description FROM ProductCategories WHERE Code IN ('ALIM', 'HOGAR');
