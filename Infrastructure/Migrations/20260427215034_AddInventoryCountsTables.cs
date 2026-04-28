using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryCountsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "InventoryCounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    CountType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedToUserId = table.Column<int>(type: "int", nullable: false),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalProducts = table.Column<int>(type: "int", nullable: false),
                    CountedProducts = table.Column<int>(type: "int", nullable: false),
                    ProductsWithVariance = table.Column<int>(type: "int", nullable: false),
                    TotalVarianceCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryCounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryCounts_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryCounts_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryCounts_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryCounts_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryCounts_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryCounts_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryCountDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryCountId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    SystemQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PhysicalQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Variance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    VariancePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    VarianceCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CountedByUserId = table.Column<int>(type: "int", nullable: true),
                    CountedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecountRequested = table.Column<bool>(type: "bit", nullable: false),
                    RecountedByUserId = table.Column<int>(type: "int", nullable: true),
                    RecountedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryCountDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryCountDetails_InventoryCounts_InventoryCountId",
                        column: x => x.InventoryCountId,
                        principalTable: "InventoryCounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryCountDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryCountDetails_Users_CountedByUserId",
                        column: x => x.CountedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryCountDetails_Users_RecountedByUserId",
                        column: x => x.RecountedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCountDetails_CountedByUserId",
                table: "InventoryCountDetails",
                column: "CountedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCountDetails_InventoryCountId",
                table: "InventoryCountDetails",
                column: "InventoryCountId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCountDetails_ProductId",
                table: "InventoryCountDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCountDetails_RecountedByUserId",
                table: "InventoryCountDetails",
                column: "RecountedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCounts_ApprovedByUserId",
                table: "InventoryCounts",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCounts_AssignedToUserId",
                table: "InventoryCounts",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCounts_CategoryId",
                table: "InventoryCounts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCounts_CompanyId",
                table: "InventoryCounts",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCounts_CreatedByUserId",
                table: "InventoryCounts",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCounts_WarehouseId",
                table: "InventoryCounts",
                column: "WarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryCountDetails");

            migrationBuilder.DropTable(
                name: "InventoryCounts");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Products");
        }
    }
}
