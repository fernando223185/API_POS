using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUTF8Collation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- ═══════════════════════════════════════════════════════════════
                -- CAMBIAR COLLATION A UTF-8 PARA SOPORTAR TILDES Y CARACTERES ESPECIALES
                -- Estrategia: Drop ALL indexes dinámicamente, alterar,  recrear
                -- ═══════════════════════════════════════════════════════════════
                
                -- Guardar definición de índices en tabla temporal
                SELECT 
                    t.name AS TableName,
                    i.name AS IndexName,
                    i.type_desc AS IndexType,
                    i.is_unique AS IsUnique,
                    STUFF((
                        SELECT ', ' + c.name + CASE WHEN ic.is_descending_key = 1 THEN ' DESC' ELSE ' ASC' END
                        FROM sys.index_columns ic
                        JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id  
                        WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 0
                        ORDER BY ic.key_ordinal
                        FOR XML PATH('')
                    ), 1, 2, '') AS KeyColumns,
                    STUFF((
                        SELECT ', ' + c.name
                        FROM sys.index_columns ic
                        JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                        WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 1
                        FOR XML PATH('')
                    ), 1, 2, '') AS IncludedColumns,
                    i.filter_definition AS FilterDefinition
                INTO #IndexesToRecreate
                FROM sys.indexes i
                JOIN sys.tables t ON i.object_id = t.object_id
                WHERE t.name IN ('Customer', 'Products', 'Sales', 'Users', 'Companies', 'Branches', 'Warehouses', 'Invoices', 'InvoiceDetails', 'Suppliers')
                AND i.type > 0  AND i.is_primary_key = 0  AND i.is_unique_constraint = 0;

                -- Drop todos los índices
                DECLARE @DropSql NVARCHAR(MAX) = '';
                SELECT @DropSql = @DropSql + 'DROP INDEX ' + QUOTENAME(IndexName) + ' ON ' + QUOTENAME(TableName) + ';' + CHAR(13)
                FROM #IndexesToRecreate;
                EXEC sp_executesql @DropSql;

                -- CUSTOMER
                ALTER TABLE Customer ALTER COLUMN Name NVARCHAR(200) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Customer ALTER COLUMN LastName NVARCHAR(200) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Customer ALTER COLUMN Code NVARCHAR(50) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Customer ALTER COLUMN Email NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Customer ALTER COLUMN Phone NVARCHAR(20) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Customer ALTER COLUMN Address NVARCHAR(500) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Customer ALTER COLUMN TaxId NVARCHAR(20) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Customer ALTER COLUMN ZipCode NVARCHAR(10) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Customer ALTER COLUMN Commentary NVARCHAR(1000) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Customer ALTER COLUMN InteriorNumber NVARCHAR(50) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Customer ALTER COLUMN ExteriorNumber NVARCHAR(50) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Customer ALTER COLUMN CompanyName NVARCHAR(200) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Customer ALTER COLUMN SatTaxRegime NVARCHAR(10) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Customer ALTER COLUMN SatCfdiUse NVARCHAR(10) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;

                -- PRODUCTS
                ALTER TABLE Products ALTER COLUMN name NVARCHAR(200) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Products ALTER COLUMN description NVARCHAR(1000) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Products ALTER COLUMN code NVARCHAR(50) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Products ALTER COLUMN barcode NVARCHAR(50) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Products ALTER COLUMN Brand NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Products ALTER COLUMN Model NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Products ALTER COLUMN SatCode NVARCHAR(10) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Products ALTER COLUMN SatUnit NVARCHAR(10) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Products ALTER COLUMN SatTaxType NVARCHAR(20) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Products ALTER COLUMN CustomsCode NVARCHAR(20) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Products ALTER COLUMN CountryOfOrigin NVARCHAR(50) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;

                -- SALES
                ALTER TABLE Sales ALTER COLUMN Code NVARCHAR(20) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Sales ALTER COLUMN CustomerName NVARCHAR(200) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;

                -- USERS
                ALTER TABLE Users ALTER COLUMN Code NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Users ALTER COLUMN Name NVARCHAR(255) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Users ALTER COLUMN Email NVARCHAR(255) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Users ALTER COLUMN Phone NVARCHAR(20) COLLATE Latin1_General_100_CI_AS_SC_UTF8;

                -- COMPANIES
                ALTER TABLE Companies ALTER COLUMN Code NVARCHAR(20) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Companies ALTER COLUMN LegalName NVARCHAR(300) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Companies ALTER COLUMN TradeName NVARCHAR(200) COLLATE Latin1_General_100_CI_AS_SC_UTF8;

                -- BRANCHES
                ALTER TABLE Branches ALTER COLUMN Code NVARCHAR(20) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Branches ALTER COLUMN Name NVARCHAR(200) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Branches ALTER COLUMN Description NVARCHAR(500) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Branches ALTER COLUMN Address NVARCHAR(500) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Branches ALTER COLUMN City NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Branches ALTER COLUMN State NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8;

                -- WAREHOUSES
                ALTER TABLE Warehouses ALTER COLUMN Code NVARCHAR(20) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Warehouses ALTER COLUMN Name NVARCHAR(200) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Warehouses ALTER COLUMN Description NVARCHAR(500) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Warehouses ALTER COLUMN WarehouseType NVARCHAR(50) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Warehouses ALTER COLUMN PhysicalLocation NVARCHAR(200) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;

                -- INVOICES
                ALTER TABLE Invoices ALTER COLUMN Serie NVARCHAR(10) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Invoices ALTER COLUMN Folio NVARCHAR(20) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Invoices ALTER COLUMN FormaPago NVARCHAR(2) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Invoices ALTER COLUMN MetodoPago NVARCHAR(3) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Invoices ALTER COLUMN CondicionesDePago NVARCHAR(500) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Invoices ALTER COLUMN TipoDeComprobante NVARCHAR(1) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Invoices ALTER COLUMN LugarExpedicion NVARCHAR(5) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Invoices ALTER COLUMN EmisorRfc NVARCHAR(13) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Invoices ALTER COLUMN EmisorNombre NVARCHAR(300) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Invoices ALTER COLUMN EmisorRegimenFiscal NVARCHAR(3) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Invoices ALTER COLUMN ReceptorRfc NVARCHAR(13) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Invoices ALTER COLUMN ReceptorNombre NVARCHAR(300) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Invoices ALTER COLUMN ReceptorDomicilioFiscal NVARCHAR(5) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Invoices ALTER COLUMN ReceptorRegimenFiscal NVARCHAR(3) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Invoices ALTER COLUMN ReceptorUsoCfdi NVARCHAR(4) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Invoices ALTER COLUMN Moneda NVARCHAR(3) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Invoices ALTER COLUMN Status NVARCHAR(20) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Invoices ALTER COLUMN Uuid NVARCHAR(50) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Invoices ALTER COLUMN NoCertificadoCfdi NVARCHAR(20) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Invoices ALTER COLUMN NoCertificadoSat NVARCHAR(20) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Invoices ALTER COLUMN CancellationReason NVARCHAR(500) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Invoices ALTER COLUMN CancellationMotivo NVARCHAR(2) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Invoices ALTER COLUMN CancellationFolioSustitucion NVARCHAR(36) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Invoices ALTER COLUMN CancellationSatCode NVARCHAR(10) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Invoices ALTER COLUMN Notes NVARCHAR(1000) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;

                -- INVOICE DETAILS
                ALTER TABLE InvoiceDetails ALTER COLUMN ClaveProdServ NVARCHAR(8) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE InvoiceDetails ALTER COLUMN NoIdentificacion NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE InvoiceDetails ALTER COLUMN ClaveUnidad NVARCHAR(3) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE InvoiceDetails ALTER COLUMN Unidad NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE InvoiceDetails ALTER COLUMN Descripcion NVARCHAR(1000) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE InvoiceDetails ALTER COLUMN ObjetoImp NVARCHAR(2) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE InvoiceDetails ALTER COLUMN TrasladoImpuesto NVARCHAR(3) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE InvoiceDetails ALTER COLUMN TrasladoTipoFactor NVARCHAR(10) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;

                -- SUPPLIERS
                ALTER TABLE Suppliers ALTER COLUMN Name NVARCHAR(200) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Suppliers ALTER COLUMN Code NVARCHAR(50) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Suppliers ALTER COLUMN TaxId NVARCHAR(20) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Suppliers ALTER COLUMN ContactPerson NVARCHAR(200) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Suppliers ALTER COLUMN Email NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Suppliers ALTER COLUMN Phone NVARCHAR(20) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Suppliers ALTER COLUMN Address NVARCHAR(500) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Suppliers ALTER COLUMN City NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Suppliers ALTER COLUMN State NVARCHAR(50) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Suppliers ALTER COLUMN ZipCode NVARCHAR(10) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Suppliers ALTER COLUMN Country NVARCHAR(50) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;

                -- Recrear índices
                DECLARE @RecreateSql NVARCHAR(MAX) = '';
                SELECT @RecreateSql = @RecreateSql + 
                    'CREATE ' + 
                    CASE WHEN IsUnique = 1 THEN 'UNIQUE ' ELSE '' END +
                    IndexType + ' INDEX ' + QUOTENAME(IndexName) + 
                    ' ON ' + QUOTENAME(TableName) + ' (' + KeyColumns + ')' +
                    CASE WHEN IncludedColumns IS NOT NULL THEN ' INCLUDE (' + IncludedColumns + ')' ELSE '' END +
                    CASE WHEN FilterDefinition IS NOT NULL THEN ' WHERE ' + FilterDefinition ELSE '' END +
                    ';' + CHAR(13)
                FROM #IndexesToRecreate;
                EXEC sp_executesql @RecreateSql;

                DROP TABLE #IndexesToRecreate;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
