using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyAndBranchToSales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Agregar columnas CompanyId y BranchId
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Sales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Sales",
                type: "int",
                nullable: true);

            // 2. Poblar datos existentes desde Warehouse -> Branch -> Company
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

            // 3. Crear índices para mejorar performance
            migrationBuilder.CreateIndex(
                name: "IX_Sales_BranchId",
                table: "Sales",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_CompanyId",
                table: "Sales",
                column: "CompanyId");

            // 4. Agregar Foreign Keys
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
            // Eliminar Foreign Keys
            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Branches_BranchId",
                table: "Sales");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Companies_CompanyId",
                table: "Sales");

            // Eliminar índices
            migrationBuilder.DropIndex(
                name: "IX_Sales_BranchId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_CompanyId",
                table: "Sales");

            // Eliminar columnas
            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Sales");
        }
    }
}
