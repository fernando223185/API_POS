using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentEmisorReceptorFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmisorNombre",
                table: "Payments",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmisorRegimenFiscal",
                table: "Payments",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmisorRfc",
                table: "Payments",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LugarExpedicion",
                table: "Payments",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceptorDomicilioFiscal",
                table: "Payments",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceptorNombre",
                table: "Payments",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceptorRegimenFiscal",
                table: "Payments",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceptorRfc",
                table: "Payments",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceptorUsoCfdi",
                table: "Payments",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmisorNombre",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "EmisorRegimenFiscal",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "EmisorRfc",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "LugarExpedicion",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReceptorDomicilioFiscal",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReceptorNombre",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReceptorRegimenFiscal",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReceptorRfc",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReceptorUsoCfdi",
                table: "Payments");
        }
    }
}
