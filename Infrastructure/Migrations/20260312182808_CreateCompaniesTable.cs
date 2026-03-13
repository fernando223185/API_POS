using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateCompaniesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Branches",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LegalName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TradeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TaxId = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    SatTaxRegime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FiscalZipCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FiscalAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Website = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    InvoiceSeries = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    InvoiceStartingFolio = table.Column<int>(type: "int", nullable: false),
                    InvoiceCurrentFolio = table.Column<int>(type: "int", nullable: false),
                    SatCertificatePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SatKeyPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SatKeyPassword = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DefaultCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Slogan = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsMainCompany = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Companies_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Companies_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_CompanyId",
                table: "Branches",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_CreatedByUserId",
                table: "Companies",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_UpdatedByUserId",
                table: "Companies",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Companies_CompanyId",
                table: "Branches",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Companies_CompanyId",
                table: "Branches");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Branches_CompanyId",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Branches");
        }
    }
}
