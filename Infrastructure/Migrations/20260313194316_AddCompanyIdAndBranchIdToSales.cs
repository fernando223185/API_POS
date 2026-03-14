using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyIdAndBranchIdToSales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Sales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Sales",
                type: "int",
                nullable: true);

            // 🆕 POBLAR DATOS EXISTENTES desde Warehouse → Branch → Company
            migrationBuilder.Sql(@"
                UPDATE s
                SET 
                    s.BranchId = w.BranchId,
                    s.CompanyId = b.CompanyId
                FROM Sales s
                INNER JOIN Warehouses w ON w.Id = s.WarehouseId
                INNER JOIN Branches b ON b.Id = w.BranchId
                WHERE s.BranchId IS NULL OR s.CompanyId IS NULL;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_BranchId",
                table: "Sales",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_CompanyId",
                table: "Sales",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Branches_BranchId",
                table: "Sales",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Companies_CompanyId",
                table: "Sales",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Branches_BranchId",
                table: "Sales");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Companies_CompanyId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_BranchId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_CompanyId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Sales");
        }
    }
}
