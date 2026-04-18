using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultReportTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- =====================================================
                -- SEED: PLANTILLAS DE REPORTE POR DEFECTO (6 tipos)
                -- CompanyId = NULL → plantillas globales del sistema
                -- =====================================================

                -- 1. VENTAS (Sales)
                INSERT INTO [ReportTemplates] ([Name],[ReportType],[IsDefault],[IsActive],[Description],[CompanyId],[CreatedByUserId],[CreatedAt],[SectionsJson])
                VALUES (
                    N'Reporte de Venta Estándar',
                    'Sales', 1, 1,
                    N'Plantilla por defecto para comprobantes de venta POS',
                    NULL, NULL, GETUTCDATE(),
                    N'[{""type"":""Header"",""title"":""Información de la Venta"",""order"":1,""showTitle"":true,""fields"":[{""field"":""saleCode"",""label"":""N° de Venta"",""bold"":true,""fontSize"":10,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""saleDate"",""label"":""Fecha"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""datetime"",""inline"":true},{""field"":""saleType"",""label"":""Tipo"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""customerName"",""label"":""Cliente"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""customerTaxId"",""label"":""RFC"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":true},{""field"":""sellerName"",""label"":""Vendedor"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""branchName"",""label"":""Sucursal"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":true}],""columns"":[]},{""type"":""Table"",""title"":""Detalle de Productos"",""order"":2,""showTitle"":true,""fields"":[],""columns"":[{""field"":""productCode"",""label"":""Código"",""width"":60,""align"":""left"",""format"":""text"",""bold"":false},{""field"":""productName"",""label"":""Descripción"",""width"":0,""align"":""left"",""format"":""text"",""bold"":false},{""field"":""quantity"",""label"":""Cant."",""width"":45,""align"":""center"",""format"":""number"",""bold"":false},{""field"":""unitPrice"",""label"":""P.Unit"",""width"":65,""align"":""right"",""format"":""currency"",""bold"":false},{""field"":""discountPercentage"",""label"":""%Desc."",""width"":45,""align"":""right"",""format"":""percentage"",""bold"":false},{""field"":""taxAmount"",""label"":""IVA"",""width"":60,""align"":""right"",""format"":""currency"",""bold"":false},{""field"":""lineTotal"",""label"":""Total"",""width"":70,""align"":""right"",""format"":""currency"",""bold"":true}]},{""type"":""Summary"",""title"":""Totales"",""order"":3,""showTitle"":true,""fields"":[{""field"":""totalSubtotal"",""label"":""Subtotal"",""bold"":false,""fontSize"":9,""align"":""right"",""format"":""currency"",""inline"":false},{""field"":""totalDiscount"",""label"":""Descuento"",""bold"":false,""fontSize"":9,""align"":""right"",""format"":""currency"",""inline"":false},{""field"":""totalTax"",""label"":""IVA"",""bold"":false,""fontSize"":9,""align"":""right"",""format"":""currency"",""inline"":false},{""field"":""totalAmount"",""label"":""TOTAL"",""bold"":true,""fontSize"":11,""align"":""right"",""format"":""currency"",""inline"":false},{""field"":""paymentMethods"",""label"":""Forma de pago"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":false}],""columns"":[]}]'
                );

                -- 2. ENTREGA A DOMICILIO (Delivery)
                INSERT INTO [ReportTemplates] ([Name],[ReportType],[IsDefault],[IsActive],[Description],[CompanyId],[CreatedByUserId],[CreatedAt],[SectionsJson])
                VALUES (
                    N'Reporte de Entrega Estándar',
                    'Delivery', 1, 1,
                    N'Plantilla por defecto para comprobantes de entrega a domicilio',
                    NULL, NULL, GETUTCDATE(),
                    N'[{""type"":""Header"",""title"":""Información de la Entrega"",""order"":1,""showTitle"":true,""fields"":[{""field"":""saleCode"",""label"":""N° de Pedido"",""bold"":true,""fontSize"":10,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""saleDate"",""label"":""Fecha de venta"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""datetime"",""inline"":true},{""field"":""scheduledDate"",""label"":""Entrega programada"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""date"",""inline"":false},{""field"":""deliveredAt"",""label"":""Entregado el"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""datetime"",""inline"":true},{""field"":""customerName"",""label"":""Cliente"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""deliveryAddress"",""label"":""Dirección de entrega"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""sellerName"",""label"":""Vendedor"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""branchName"",""label"":""Sucursal"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":true}],""columns"":[]},{""type"":""Table"",""title"":""Productos"",""order"":2,""showTitle"":true,""fields"":[],""columns"":[{""field"":""productCode"",""label"":""Código"",""width"":60,""align"":""left"",""format"":""text"",""bold"":false},{""field"":""productName"",""label"":""Descripción"",""width"":0,""align"":""left"",""format"":""text"",""bold"":false},{""field"":""quantity"",""label"":""Cant."",""width"":45,""align"":""center"",""format"":""number"",""bold"":false},{""field"":""unitPrice"",""label"":""P.Unit"",""width"":65,""align"":""right"",""format"":""currency"",""bold"":false},{""field"":""lineTotal"",""label"":""Total"",""width"":70,""align"":""right"",""format"":""currency"",""bold"":true}]},{""type"":""Summary"",""title"":""Totales"",""order"":3,""showTitle"":true,""fields"":[{""field"":""totalAmount"",""label"":""TOTAL"",""bold"":true,""fontSize"":11,""align"":""right"",""format"":""currency"",""inline"":false},{""field"":""paymentMethods"",""label"":""Forma de pago"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":false}],""columns"":[]}]'
                );

                -- 3. COTIZACIONES (Quotation)
                INSERT INTO [ReportTemplates] ([Name],[ReportType],[IsDefault],[IsActive],[Description],[CompanyId],[CreatedByUserId],[CreatedAt],[SectionsJson])
                VALUES (
                    N'Cotización Estándar',
                    'Quotation', 1, 1,
                    N'Plantilla por defecto para cotizaciones',
                    NULL, NULL, GETUTCDATE(),
                    N'[{""type"":""Header"",""title"":""Información de la Cotización"",""order"":1,""showTitle"":true,""fields"":[{""field"":""quotationCode"",""label"":""N° de Cotización"",""bold"":true,""fontSize"":10,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""quotationDate"",""label"":""Fecha"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""datetime"",""inline"":true},{""field"":""validUntil"",""label"":""Válida hasta"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""date"",""inline"":false},{""field"":""status"",""label"":""Estado"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":true},{""field"":""customerName"",""label"":""Cliente"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""customerTaxId"",""label"":""RFC"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":true},{""field"":""sellerName"",""label"":""Vendedor"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""branchName"",""label"":""Sucursal"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":true}],""columns"":[]},{""type"":""Table"",""title"":""Detalle"",""order"":2,""showTitle"":true,""fields"":[],""columns"":[{""field"":""productCode"",""label"":""Código"",""width"":60,""align"":""left"",""format"":""text"",""bold"":false},{""field"":""productName"",""label"":""Descripción"",""width"":0,""align"":""left"",""format"":""text"",""bold"":false},{""field"":""quantity"",""label"":""Cant."",""width"":45,""align"":""center"",""format"":""number"",""bold"":false},{""field"":""unitPrice"",""label"":""P.Unit"",""width"":65,""align"":""right"",""format"":""currency"",""bold"":false},{""field"":""discountPercentage"",""label"":""%Desc."",""width"":45,""align"":""right"",""format"":""percentage"",""bold"":false},{""field"":""lineTotal"",""label"":""Total"",""width"":70,""align"":""right"",""format"":""currency"",""bold"":true}]},{""type"":""Summary"",""title"":""Totales"",""order"":3,""showTitle"":true,""fields"":[{""field"":""totalSubtotal"",""label"":""Subtotal"",""bold"":false,""fontSize"":9,""align"":""right"",""format"":""currency"",""inline"":false},{""field"":""totalDiscount"",""label"":""Descuento"",""bold"":false,""fontSize"":9,""align"":""right"",""format"":""currency"",""inline"":false},{""field"":""totalTax"",""label"":""IVA"",""bold"":false,""fontSize"":9,""align"":""right"",""format"":""currency"",""inline"":false},{""field"":""totalAmount"",""label"":""TOTAL"",""bold"":true,""fontSize"":11,""align"":""right"",""format"":""currency"",""inline"":false}],""columns"":[]}]'
                );

                -- 4. ÓRDENES DE COMPRA (Purchase)
                INSERT INTO [ReportTemplates] ([Name],[ReportType],[IsDefault],[IsActive],[Description],[CompanyId],[CreatedByUserId],[CreatedAt],[SectionsJson])
                VALUES (
                    N'Orden de Compra Estándar',
                    'Purchase', 1, 1,
                    N'Plantilla por defecto para órdenes de compra a proveedores',
                    NULL, NULL, GETUTCDATE(),
                    N'[{""type"":""Header"",""title"":""Información de la Orden"",""order"":1,""showTitle"":true,""fields"":[{""field"":""poCode"",""label"":""N° de Orden"",""bold"":true,""fontSize"":10,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""poDate"",""label"":""Fecha"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""datetime"",""inline"":true},{""field"":""status"",""label"":""Estado"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""supplierName"",""label"":""Proveedor"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""supplierTaxId"",""label"":""RFC"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":true},{""field"":""warehouseName"",""label"":""Almacén destino"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""notes"",""label"":""Notas"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":false}],""columns"":[]},{""type"":""Table"",""title"":""Productos"",""order"":2,""showTitle"":true,""fields"":[],""columns"":[{""field"":""productCode"",""label"":""Código"",""width"":60,""align"":""left"",""format"":""text"",""bold"":false},{""field"":""productName"",""label"":""Descripción"",""width"":0,""align"":""left"",""format"":""text"",""bold"":false},{""field"":""quantityOrdered"",""label"":""Pedido"",""width"":55,""align"":""center"",""format"":""number"",""bold"":false},{""field"":""quantityReceived"",""label"":""Recibido"",""width"":60,""align"":""center"",""format"":""number"",""bold"":false},{""field"":""unitCost"",""label"":""Costo Unit."",""width"":70,""align"":""right"",""format"":""currency"",""bold"":false},{""field"":""lineTotal"",""label"":""Total"",""width"":70,""align"":""right"",""format"":""currency"",""bold"":true}]},{""type"":""Summary"",""title"":""Total"",""order"":3,""showTitle"":true,""fields"":[{""field"":""totalAmount"",""label"":""TOTAL ORDEN"",""bold"":true,""fontSize"":11,""align"":""right"",""format"":""currency"",""inline"":false}],""columns"":[]}]'
                );

                -- 5. INVENTARIO (Inventory)
                INSERT INTO [ReportTemplates] ([Name],[ReportType],[IsDefault],[IsActive],[Description],[CompanyId],[CreatedByUserId],[CreatedAt],[SectionsJson])
                VALUES (
                    N'Reporte de Inventario Estándar',
                    'Inventory', 1, 1,
                    N'Plantilla por defecto para reportes de stock por almacén',
                    NULL, NULL, GETUTCDATE(),
                    N'[{""type"":""Header"",""title"":""Información del Reporte"",""order"":1,""showTitle"":true,""fields"":[{""field"":""reportDate"",""label"":""Fecha del reporte"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""datetime"",""inline"":false},{""field"":""warehouseName"",""label"":""Almacén"",""bold"":true,""fontSize"":10,""align"":""left"",""format"":""text"",""inline"":true},{""field"":""companyName"",""label"":""Empresa"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":false}],""columns"":[]},{""type"":""Table"",""title"":""Stock de Productos"",""order"":2,""showTitle"":true,""fields"":[],""columns"":[{""field"":""productCode"",""label"":""Código"",""width"":60,""align"":""left"",""format"":""text"",""bold"":false},{""field"":""productName"",""label"":""Producto"",""width"":0,""align"":""left"",""format"":""text"",""bold"":false},{""field"":""category"",""label"":""Categoría"",""width"":80,""align"":""left"",""format"":""text"",""bold"":false},{""field"":""currentStock"",""label"":""Stock"",""width"":55,""align"":""right"",""format"":""number"",""bold"":false},{""field"":""minimumStock"",""label"":""Mínimo"",""width"":55,""align"":""right"",""format"":""number"",""bold"":false},{""field"":""unitCost"",""label"":""Costo Unit."",""width"":70,""align"":""right"",""format"":""currency"",""bold"":false},{""field"":""totalValue"",""label"":""Valor Total"",""width"":75,""align"":""right"",""format"":""currency"",""bold"":true}]},{""type"":""Summary"",""title"":""Resumen"",""order"":3,""showTitle"":true,""fields"":[{""field"":""totalProducts"",""label"":""Total productos"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""number"",""inline"":false},{""field"":""belowMinimum"",""label"":""Bajo mínimo"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""number"",""inline"":true},{""field"":""totalStockValue"",""label"":""Valor total inventario"",""bold"":true,""fontSize"":11,""align"":""right"",""format"":""currency"",""inline"":false}],""columns"":[]}]'
                );

                -- 6. TURNO DE CAJERO (CashierShift)
                INSERT INTO [ReportTemplates] ([Name],[ReportType],[IsDefault],[IsActive],[Description],[CompanyId],[CreatedByUserId],[CreatedAt],[SectionsJson])
                VALUES (
                    N'Cierre de Turno Estándar',
                    'CashierShift', 1, 1,
                    N'Plantilla por defecto para cierres de caja',
                    NULL, NULL, GETUTCDATE(),
                    N'[{""type"":""Header"",""title"":""Información del Turno"",""order"":1,""showTitle"":true,""fields"":[{""field"":""shiftCode"",""label"":""Código de turno"",""bold"":true,""fontSize"":10,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""cashierName"",""label"":""Cajero"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":true},{""field"":""openedAt"",""label"":""Apertura"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""datetime"",""inline"":false},{""field"":""closedAt"",""label"":""Cierre"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""datetime"",""inline"":true},{""field"":""warehouseName"",""label"":""Caja / Almacén"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""openingCash"",""label"":""Fondo inicial"",""bold"":false,""fontSize"":9,""align"":""right"",""format"":""currency"",""inline"":false},{""field"":""closingCash"",""label"":""Efectivo final"",""bold"":false,""fontSize"":9,""align"":""right"",""format"":""currency"",""inline"":true},{""field"":""difference"",""label"":""Diferencia"",""bold"":true,""fontSize"":9,""align"":""right"",""format"":""currency"",""inline"":false}],""columns"":[]},{""type"":""Table"",""title"":""Ventas del Turno"",""order"":2,""showTitle"":true,""fields"":[],""columns"":[{""field"":""saleCode"",""label"":""Venta"",""width"":70,""align"":""left"",""format"":""text"",""bold"":false},{""field"":""saleTime"",""label"":""Hora"",""width"":55,""align"":""center"",""format"":""datetime"",""bold"":false},{""field"":""customerName"",""label"":""Cliente"",""width"":0,""align"":""left"",""format"":""text"",""bold"":false},{""field"":""paymentMethod"",""label"":""Pago"",""width"":70,""align"":""left"",""format"":""text"",""bold"":false},{""field"":""saleTotal"",""label"":""Total"",""width"":70,""align"":""right"",""format"":""currency"",""bold"":true}]},{""type"":""Summary"",""title"":""Resumen por Forma de Pago"",""order"":3,""showTitle"":true,""fields"":[{""field"":""cashTotal"",""label"":""Efectivo"",""bold"":false,""fontSize"":9,""align"":""right"",""format"":""currency"",""inline"":false},{""field"":""cardTotal"",""label"":""Tarjeta"",""bold"":false,""fontSize"":9,""align"":""right"",""format"":""currency"",""inline"":true},{""field"":""transferTotal"",""label"":""Transferencia"",""bold"":false,""fontSize"":9,""align"":""right"",""format"":""currency"",""inline"":false},{""field"":""salesCount"",""label"":""N° ventas"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""number"",""inline"":true},{""field"":""totalSales"",""label"":""TOTAL TURNO"",""bold"":true,""fontSize"":11,""align"":""right"",""format"":""currency"",""inline"":false}],""columns"":[]}]'
                );

                PRINT N'✅ 6 plantillas de reporte por defecto insertadas exitosamente';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM [ReportTemplates]
                WHERE [CompanyId] IS NULL
                  AND [CreatedByUserId] IS NULL
                  AND [IsDefault] = 1
                  AND [ReportType] IN ('Sales','Delivery','Quotation','Purchase','Inventory','CashierShift');

                PRINT N'🗑️ Plantillas de reporte por defecto eliminadas (rollback)';
            ");
        }
    }
}
