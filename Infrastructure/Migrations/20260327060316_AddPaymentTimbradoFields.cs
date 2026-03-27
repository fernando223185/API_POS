using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTimbradoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CadenaOriginalSat",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoCertificadoCfdi",
                table: "Payments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoCertificadoSat",
                table: "Payments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QrCode",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelloCfdi",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelloSat",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimbradoAt",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Uuid",
                table: "Payments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "XmlCfdi",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CadenaOriginalSat",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "NoCertificadoCfdi",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "NoCertificadoSat",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "QrCode",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SelloCfdi",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SelloSat",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TimbradoAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Uuid",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "XmlCfdi",
                table: "Payments");
        }
    }
}
