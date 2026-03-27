using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentComplementRequiredFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentCurrency",
                table: "PaymentApplications",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "MXN");

            migrationBuilder.AddColumn<decimal>(
                name: "DocumentExchangeRate",
                table: "PaymentApplications",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 1.0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "PaymentApplications",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxBase",
                table: "PaymentApplications",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TaxCode",
                table: "PaymentApplications",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "002");

            migrationBuilder.AddColumn<string>(
                name: "TaxFactorType",
                table: "PaymentApplications",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Tasa");

            migrationBuilder.AddColumn<string>(
                name: "TaxObject",
                table: "PaymentApplications",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "02");

            migrationBuilder.AddColumn<decimal>(
                name: "TaxRate",
                table: "PaymentApplications",
                type: "decimal(8,6)",
                precision: 8,
                scale: 6,
                nullable: false,
                defaultValue: 0.160000m);

            migrationBuilder.AddColumn<string>(
                name: "CustomerCfdiUse",
                table: "InvoicesPPD",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerTaxRegime",
                table: "InvoicesPPD",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerZipCode",
                table: "InvoicesPPD",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "InvoicesPPD",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "InvoicesPPD",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentCurrency",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "DocumentExchangeRate",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "TaxBase",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "TaxCode",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "TaxFactorType",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "TaxObject",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "TaxRate",
                table: "PaymentApplications");

            migrationBuilder.DropColumn(
                name: "CustomerCfdiUse",
                table: "InvoicesPPD");

            migrationBuilder.DropColumn(
                name: "CustomerTaxRegime",
                table: "InvoicesPPD");

            migrationBuilder.DropColumn(
                name: "CustomerZipCode",
                table: "InvoicesPPD");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "InvoicesPPD");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "InvoicesPPD");
        }
    }
}
