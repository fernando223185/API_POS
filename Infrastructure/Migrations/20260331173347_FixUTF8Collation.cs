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
            // ════════════════════════════════════════════════════════════════════════════════
            
            migrationBuilder.Sql(@"
                -- Customers
                ALTER TABLE Customers ALTER COLUMN Name NVARCHAR(200) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Customers ALTER COLUMN LegalName NVARCHAR(300) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Customers ALTER COLUMN Address NVARCHAR(500) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Customers ALTER COLUMN City NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Customers ALTER COLUMN State NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Customers ALTER COLUMN ZipCode NVARCHAR(10) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Customers ALTER COLUMN Email NVARCHAR(150) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Customers ALTER COLUMN Phone NVARCHAR(20) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                
                -- Products
                ALTER TABLE Products ALTER COLUMN Name NVARCHAR(200) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Products ALTER COLUMN Description NVARCHAR(500) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Products ALTER COLUMN Unit NVARCHAR(50) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                
                -- SalesNew
                ALTER TABLE SalesNew ALTER COLUMN Notes NVARCHAR(MAX) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                
                -- Users
                ALTER TABLE Users ALTER COLUMN Name NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Users ALTER COLUMN Email NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                
                -- Companies
                ALTER TABLE Companies ALTER COLUMN LegalName NVARCHAR(300) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Companies ALTER COLUMN CommercialName NVARCHAR(200) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Companies ALTER COLUMN Address NVARCHAR(500) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Companies ALTER COLUMN City NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Companies ALTER COLUMN State NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                
                -- Branches
                ALTER TABLE Branches ALTER COLUMN Name NVARCHAR(200) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Branches ALTER COLUMN Address NVARCHAR(500) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Branches ALTER COLUMN City NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Branches ALTER COLUMN State NVARCHAR(100) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                
                -- Warehouses
                ALTER TABLE Warehouses ALTER COLUMN Name NVARCHAR(200) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
                ALTER TABLE Warehouses ALTER COLUMN Address NVARCHAR(500) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                
                -- Invoices
                ALTER TABLE Invoices ALTER COLUMN EmisorNombre NVARCHAR(300) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Invoices ALTER COLUMN ReceptorNombre NVARCHAR(300) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                ALTER TABLE Invoices ALTER COLUMN Notes NVARCHAR(MAX) COLLATE Latin1_General_100_CI_AS_SC_UTF8 NULL;
                
                -- InvoiceDetails
                ALTER TABLE InvoiceDetails ALTER COLUMN Descripcion NVARCHAR(1000) COLLATE Latin1_General_100_CI_AS_SC_UTF8;
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
