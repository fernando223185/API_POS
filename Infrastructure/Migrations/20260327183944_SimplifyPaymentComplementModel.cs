using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyPaymentComplementModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Nota: La FK y tabla InvoicesPPD ya fueron eliminadas en la migración UnifyInvoicePPDIntoInvoice
            // Solo eliminamos los campos de complemento de PaymentApplications

            migrationBuilder.DropColumn(
                name: "ComplementError",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "ComplementFolio",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "ComplementSerie",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "ComplementSerieAndFolio",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "ComplementStatus",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "ComplementUUID",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "EmailSent",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "EmailSentAt",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "GeneratedAt",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "LastRetryAt",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "PdfPath",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "SATCertificationDate",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "SATSerialNumber",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "XmlContent",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "XmlPath",
                table: "PaymentApplications");

            migrationBuilder.AddColumn<string>(
                name: "ComplementError",
                table: "Payments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplementFolio",
                table: "Payments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplementSerie",
                table: "Payments",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailSent",
                table: "Payments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailSentAt",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRetryAt",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PdfPath",
                table: "Payments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "XmlPath",
                table: "Payments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComplementError",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ComplementFolio",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ComplementSerie",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "EmailSent",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "EmailSentAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "LastRetryAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PdfPath",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "XmlPath",
                table: "Payments");

            migrationBuilder.AddColumn<string>(
                name: "ComplementError",
                table: "PaymentApplications",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplementFolio",
                table: "PaymentApplications",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplementSerie",
                table: "PaymentApplications",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplementSerieAndFolio",
                table: "PaymentApplications",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplementStatus",
                table: "PaymentApplications",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ComplementUUID",
                table: "PaymentApplications",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailSent",
                table: "PaymentApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailSentAt",
                table: "PaymentApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GeneratedAt",
                table: "PaymentApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoicePPDId",
                table: "PaymentApplications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRetryAt",
                table: "PaymentApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PdfPath",
                table: "PaymentApplications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "PaymentApplications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SATCertificationDate",
                table: "PaymentApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SATSerialNumber",
                table: "PaymentApplications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "XmlContent",
                table: "PaymentApplications",
                type: "ntext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "XmlPath",
                table: "PaymentApplications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InvoicesPPD",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    BalanceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CustomerCfdiUse = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CustomerRFC = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    CustomerTaxRegime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CustomerZipCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DaysOverdue = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Folio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FolioUUID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastPaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextPartialityNumber = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OriginalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Serie = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SerieAndFolio = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPartialities = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoicesPPD", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoicesPPD_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvoicesPPD_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentApplications_InvoicePPDId",
                table: "PaymentApplications",
                column: "InvoicePPDId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesPPD_CompanyId",
                table: "InvoicesPPD",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesPPD_CustomerId",
                table: "InvoicesPPD",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesPPD_DueDate",
                table: "InvoicesPPD",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesPPD_InvoiceId",
                table: "InvoicesPPD",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesPPD_Status",
                table: "InvoicesPPD",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentApplications_InvoicesPPD_InvoicePPDId",
                table: "PaymentApplications",
                column: "InvoicePPDId",
                principalTable: "InvoicesPPD",
                principalColumn: "Id");
        }
    }
}
