using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertsTableIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Alerts_CreatedAt",
                table: "Alerts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_UniqueKey_Status",
                table: "Alerts",
                columns: new[] { "UniqueKey", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_UserId_CompanyId_Status",
                table: "Alerts",
                columns: new[] { "UserId", "CompanyId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Alerts_CreatedAt",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_UniqueKey_Status",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_UserId_CompanyId_Status",
                table: "Alerts");
        }
    }
}
