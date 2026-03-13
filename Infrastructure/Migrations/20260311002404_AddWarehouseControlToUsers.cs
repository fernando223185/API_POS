using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseControlToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanSellFromMultipleWarehouses",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DefaultWarehouseId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DefaultWarehouseId",
                table: "Users",
                column: "DefaultWarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Warehouses_DefaultWarehouseId",
                table: "Users",
                column: "DefaultWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Warehouses_DefaultWarehouseId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_DefaultWarehouseId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CanSellFromMultipleWarehouses",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DefaultWarehouseId",
                table: "Users");
        }
    }
}
