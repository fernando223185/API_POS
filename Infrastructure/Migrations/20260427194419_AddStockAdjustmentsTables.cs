using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStockAdjustmentsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockAdjustments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    AdjustmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockAdjustments_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StockAdjustments_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockAdjustments_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockAdjustmentDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockAdjustmentId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    SystemQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PhysicalQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdjustmentQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAdjustmentDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockAdjustmentDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockAdjustmentDetails_StockAdjustments_StockAdjustmentId",
                        column: x => x.StockAdjustmentId,
                        principalTable: "StockAdjustments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentDetails_ProductId",
                table: "StockAdjustmentDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentDetails_StockAdjustmentId",
                table: "StockAdjustmentDetails",
                column: "StockAdjustmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustments_CompanyId",
                table: "StockAdjustments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustments_CreatedByUserId",
                table: "StockAdjustments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustments_WarehouseId",
                table: "StockAdjustments",
                column: "WarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockAdjustmentDetails");

            migrationBuilder.DropTable(
                name: "StockAdjustments");
        }
    }
}
