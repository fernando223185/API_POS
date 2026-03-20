using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceFieldsToSales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InvoiceDate",
                table: "Sales",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceFolio",
                table: "Sales",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceSeries",
                table: "Sales",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceXml",
                table: "Sales",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SaleId = table.Column<int>(type: "int", nullable: true),
                    Serie = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Folio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FormaPago = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CondicionesDePago = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TipoDeComprobante = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    LugarExpedicion = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    EmisorRfc = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    EmisorNombre = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    EmisorRegimenFiscal = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    ReceptorRfc = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    ReceptorNombre = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ReceptorDomicilioFiscal = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    ReceptorRegimenFiscal = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    ReceptorUsoCfdi = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Uuid = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TimbradoAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    XmlCfdi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CadenaOriginalSat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SelloCfdi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SelloSat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoCertificadoCfdi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NoCertificadoSat = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    QrCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelledByUserId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Invoices_Sales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Sales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Invoices_Users_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    ClaveProdServ = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    NoIdentificacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Cantidad = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ClaveUnidad = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ValorUnitario = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Descuento = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ObjetoImp = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    TieneTraslados = table.Column<bool>(type: "bit", nullable: false),
                    TrasladoBase = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    TrasladoImpuesto = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    TrasladoTipoFactor = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TrasladoTasaOCuota = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    TrasladoImporte = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    TieneRetenciones = table.Column<bool>(type: "bit", nullable: false),
                    RetencionBase = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    RetencionImpuesto = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    RetencionTipoFactor = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    RetencionTasaOCuota = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    RetencionImporte = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    SaleDetailId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceDetails_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sales_InvoiceId",
                table: "Sales",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetails_InvoiceId",
                table: "InvoiceDetails",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetails_ProductId",
                table: "InvoiceDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CancelledByUserId",
                table: "Invoices",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CompanyId",
                table: "Invoices",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CreatedAt",
                table: "Invoices",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CreatedByUserId",
                table: "Invoices",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceDate",
                table: "Invoices",
                column: "InvoiceDate");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_SaleId",
                table: "Invoices",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Serie_Folio",
                table: "Invoices",
                columns: new[] { "Serie", "Folio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Status",
                table: "Invoices",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Uuid",
                table: "Invoices",
                column: "Uuid",
                unique: true,
                filter: "[Uuid] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Invoices_InvoiceId",
                table: "Sales",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Invoices_InvoiceId",
                table: "Sales");

            migrationBuilder.DropTable(
                name: "InvoiceDetails");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Sales_InvoiceId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "InvoiceDate",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "InvoiceFolio",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "InvoiceSeries",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "InvoiceXml",
                table: "Sales");
        }
    }
}
