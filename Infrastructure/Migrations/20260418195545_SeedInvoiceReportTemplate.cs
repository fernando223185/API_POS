using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedInvoiceReportTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- =====================================================
                -- SEED: PLANTILLA DE FACTURA CFDI (Invoice)
                -- CompanyId = NULL → plantilla global del sistema
                -- =====================================================

                INSERT INTO [ReportTemplates] ([Name],[ReportType],[IsDefault],[IsActive],[Description],[CompanyId],[CreatedByUserId],[CreatedAt],[SectionsJson])
                VALUES (
                    N'Factura CFDI Estándar',
                    'Invoice', 1, 1,
                    N'Plantilla por defecto para facturas CFDI (Comprobante Fiscal Digital)',
                    NULL, NULL, GETUTCDATE(),
                    N'[{""type"":""Header"",""title"":""Datos del Comprobante"",""order"":1,""showTitle"":true,""fields"":[{""field"":""invoiceSerie"",""label"":""Serie"",""bold"":true,""fontSize"":10,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""invoiceFolio"",""label"":""Folio"",""bold"":true,""fontSize"":10,""align"":""left"",""format"":""text"",""inline"":true},{""field"":""invoiceDate"",""label"":""Fecha"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""datetime"",""inline"":false},{""field"":""invoiceStatus"",""label"":""Estado"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":true},{""field"":""tipoDeComprobante"",""label"":""Tipo"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""metodoPago"",""label"":""Método de pago"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""formaPago"",""label"":""Forma de pago"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":true},{""field"":""moneda"",""label"":""Moneda"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""lugarExpedicion"",""label"":""Lugar expedición (CP)"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":true}],""columns"":[]},{""type"":""Header"",""title"":""Emisor"",""order"":2,""showTitle"":true,""fields"":[{""field"":""emisorNombre"",""label"":""Razón social"",""bold"":true,""fontSize"":10,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""emisorRfc"",""label"":""RFC"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":true},{""field"":""emisorRegimenFiscal"",""label"":""Régimen fiscal"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":false}],""columns"":[]},{""type"":""Header"",""title"":""Receptor"",""order"":3,""showTitle"":true,""fields"":[{""field"":""receptorNombre"",""label"":""Razón social"",""bold"":true,""fontSize"":10,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""receptorRfc"",""label"":""RFC"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":true},{""field"":""receptorDomicilioFiscal"",""label"":""CP domicilio fiscal"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""receptorRegimenFiscal"",""label"":""Régimen fiscal"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":true},{""field"":""receptorUsoCfdi"",""label"":""Uso CFDI"",""bold"":false,""fontSize"":9,""align"":""left"",""format"":""text"",""inline"":false}],""columns"":[]},{""type"":""Table"",""title"":""Conceptos"",""order"":4,""showTitle"":true,""fields"":[],""columns"":[{""field"":""claveProdServ"",""label"":""ClaveSAT"",""width"":65,""align"":""left"",""format"":""text"",""bold"":false},{""field"":""noIdentificacion"",""label"":""Clave"",""width"":60,""align"":""left"",""format"":""text"",""bold"":false},{""field"":""descripcion"",""label"":""Descripción"",""width"":0,""align"":""left"",""format"":""text"",""bold"":false},{""field"":""cantidad"",""label"":""Cant."",""width"":40,""align"":""center"",""format"":""number"",""bold"":false},{""field"":""claveUnidad"",""label"":""UM"",""width"":35,""align"":""center"",""format"":""text"",""bold"":false},{""field"":""valorUnitario"",""label"":""V.Unit."",""width"":65,""align"":""right"",""format"":""currency"",""bold"":false},{""field"":""descuento"",""label"":""Desc."",""width"":55,""align"":""right"",""format"":""currency"",""bold"":false},{""field"":""importe"",""label"":""Importe"",""width"":70,""align"":""right"",""format"":""currency"",""bold"":false},{""field"":""trasladoTasa"",""label"":""IVA%"",""width"":45,""align"":""right"",""format"":""percentage"",""bold"":false},{""field"":""trasladoImporte"",""label"":""IVA$"",""width"":60,""align"":""right"",""format"":""currency"",""bold"":true}]},{""type"":""Summary"",""title"":""Totales"",""order"":5,""showTitle"":true,""fields"":[{""field"":""subTotal"",""label"":""Subtotal"",""bold"":false,""fontSize"":9,""align"":""right"",""format"":""currency"",""inline"":false},{""field"":""discountAmount"",""label"":""Descuento"",""bold"":false,""fontSize"":9,""align"":""right"",""format"":""currency"",""inline"":false},{""field"":""taxAmount"",""label"":""IVA"",""bold"":false,""fontSize"":9,""align"":""right"",""format"":""currency"",""inline"":false},{""field"":""total"",""label"":""TOTAL"",""bold"":true,""fontSize"":12,""align"":""right"",""format"":""currency"",""inline"":false}],""columns"":[]},{""type"":""Footer"",""title"":""Timbre Fiscal"",""order"":6,""showTitle"":true,""fields"":[{""field"":""uuid"",""label"":""UUID / Folio Fiscal"",""bold"":false,""fontSize"":8,""align"":""left"",""format"":""text"",""inline"":false},{""field"":""timbradoAt"",""label"":""Fecha timbrado"",""bold"":false,""fontSize"":8,""align"":""left"",""format"":""datetime"",""inline"":true},{""field"":""saleCode"",""label"":""Venta origen"",""bold"":false,""fontSize"":8,""align"":""left"",""format"":""text"",""inline"":false}],""columns"":[]}]'
                );

                PRINT N'✅ Plantilla de Factura CFDI insertada exitosamente';
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
                  AND [ReportType] = 'Invoice';

                PRINT N'🗑️ Plantilla de Factura CFDI eliminada (rollback)';
            ");
        }
    }
}
