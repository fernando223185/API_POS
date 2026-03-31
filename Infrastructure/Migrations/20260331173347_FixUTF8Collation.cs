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
            // ════════════════════════════════════════════════════════════════════════════════
            // CAMBIAR COLLATION A UTF-8 PARA SOPORTAR TILDES Y CARACTERES ESPECIALES
            // Primero drop TODOS los índices en las columnas que vamos a modificar
            // ════════════════════════════════════════════════════════════════════════════════
            
            migrationBuilder.Sql(@"
                -- =============== DROP ALL INDEXES ON COLUMNS WE'LL MODIFY ===============
                
                -- Customer indexes
                DROP INDEX IF EXISTS IX_Customer_Code ON Customer;
                DROP INDEX IF EXISTS IX_Customer_TaxId ON Customer;
                DROP INDEX IF EXISTS IX_Customer_Email ON Customer;
                
                -- Products indexes
                DROP INDEX IF EXISTS IX_Products_code ON Products;
                DROP INDEX IF EXISTS IX_Products_barcode ON Products;
                
                -- Sales indexes
                DROP INDEX IF EXISTS IX_Sales_Code ON Sales;
                
                -- Users indexes
                DROP INDEX IF EXISTS IX_Users_Code ON Users;
                DROP INDEX IF EXISTS IX_Users_Email ON Users;
                
                -- Companies indexes
                DROP INDEX IF EXISTS IX_Companies_Code ON Companies;
                
                -- Branches indexes
                DROP INDEX IF EXISTS IX_Branches_Code ON Branches;
                
                -- Warehouses indexes
                DROP INDEX IF EXISTS IX_Warehouses_Code ON Warehouses;
                
                -- Invoices indexes
                DROP INDEX IF EXISTS IX_Invoices_Serie_Folio ON Invoices;
                DROP INDEX IF EXISTS IX_Invoices_Uuid ON Invoices;
                
                -- Suppliers indexes
                DROP INDEX IF EXISTS IX_Suppliers_Code ON Suppliers;
                DROP INDEX IF EXISTS IX_Suppliers_TaxId ON Suppliers;

                -- =============== ALTER COLUMNS TO UTF-8 COLLATION ===============

                -- ===== CUSTOMER =====
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

                -- ===== PRODUCTS =====
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

                -- ===== SALES =====
                ALTER TABLE Sales ALTER COLUMN Code NVARCHAR(20) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Sales ALTER COLUMN CustomerName NVARCHAR(200) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;

                -- ===== USERS =====
                ALTER TABLE Users ALTER COLUMN Code NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Users ALTER COLUMN Name NVARCHAR(255) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Users ALTER COLUMN Email NVARCHAR(255) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Users ALTER COLUMN Phone NVARCHAR(20) COLLATE Latin1_General_100_CI_AS_SC_UTF8;

                -- ===== COMPANIES =====
                ALTER TABLE Companies ALTER COLUMN Code NVARCHAR(20) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Companies ALTER COLUMN LegalName NVARCHAR(300) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Companies ALTER COLUMN TradeName NVARCHAR(200) COLLATE Latin1_General_100_CI_AS_SC_UTF8;

                -- ===== BRANCHES =====
                ALTER TABLE Branches ALTER COLUMN Code NVARCHAR(20) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Branches ALTER COLUMN Name NVARCHAR(200) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Branches ALTER COLUMN Description NVARCHAR(500) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Branches ALTER COLUMN Address NVARCHAR(500) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Branches ALTER COLUMN City NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Branches ALTER COLUMN State NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8;

                -- ===== WAREHOUSES =====
                ALTER TABLE Warehouses ALTER COLUMN Code NVARCHAR(20) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Warehouses ALTER COLUMN Name NVARCHAR(200) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Warehouses ALTER COLUMN Description NVARCHAR(500) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Warehouses ALTER COLUMN WarehouseType NVARCHAR(50) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Warehouses ALTER COLUMN PhysicalLocation NVARCHAR(200) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;

                -- ===== INVOICES =====
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

                -- ===== INVOICE DETAILS =====
                ALTER TABLE InvoiceDetails ALTER COLUMN ClaveProdServ NVARCHAR(8) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE InvoiceDetails ALTER COLUMN NoIdentificacion NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE InvoiceDetails ALTER COLUMN ClaveUnidad NVARCHAR(3) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE InvoiceDetails ALTER COLUMN Unidad NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE InvoiceDetails ALTER COLUMN Descripcion NVARCHAR(1000) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE InvoiceDetails ALTER COLUMN ObjetoImp NVARCHAR(2) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE InvoiceDetails ALTER COLUMN TrasladoImpuesto NVARCHAR(3) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE InvoiceDetails ALTER COLUMN TrasladoTipoFactor NVARCHAR(10) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;

                -- ===== SUPPLIERS =====
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

                -- =============== RECREATE INDEXES ===============
                CREATE UNIQUE NONCLUSTERED INDEX IX_Customer_Code ON Customer (Code);
                CREATE UNIQUE NONCLUSTERED INDEX IX_Customer_TaxId ON Customer (TaxId);
                CREATE UNIQUE NONCLUSTERED INDEX IX_Customer_Email ON Customer (Email);
                
                CREATE UNIQUE NONCLUSTERED INDEX IX_Products_code ON Products (code);
                CREATE UNIQUE NONCLUSTERED INDEX IX_Products_barcode ON Products (barcode) WHERE barcode IS NOT NULL;
                
                CREATE UNIQUE NONCLUSTERED INDEX IX_Sales_Code ON Sales (Code);
                
                CREATE UNIQUE NONCLUSTERED INDEX IX_Users_Code ON Users (Code);
                CREATE UNIQUE NONCLUSTERED INDEX IX_Users_Email ON Users (Email);
                
                CREATE UNIQUE NONCLUSTERED INDEX IX_Companies_Code ON Companies (Code);
                CREATE UNIQUE NONCLUSTERED INDEX IX_Branches_Code ON Branches (Code);
                CREATE UNIQUE NONCLUSTERED INDEX IX_Warehouses_Code ON Warehouses (Code);
                
                CREATE UNIQUE NONCLUSTERED INDEX IX_Invoices_Serie_Folio ON Invoices (Serie, Folio);
                CREATE UNIQUE NONCLUSTERED INDEX IX_Invoices_Uuid ON Invoices (Uuid) WHERE Uuid IS NOT NULL;
                
                CREATE UNIQUE NONCLUSTERED INDEX IX_Suppliers_Code ON Suppliers (Code);
                CREATE UNIQUE NONCLUSTERED INDEX IX_Suppliers_TaxId ON Suppliers (TaxId) WHERE TaxId IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No se revierte - cambiar collation es seguro y no destructivo
            // Los datos se mantienen intactos
        }
    }
}
