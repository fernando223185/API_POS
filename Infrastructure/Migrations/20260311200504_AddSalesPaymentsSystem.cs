using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesPaymentsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Customer",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "Importe",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "Impuestos",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "Moneda",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "Mov",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "MovID",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "PrecioTotal",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "Saldo",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "User",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "Warehouse",
                table: "Sales");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Sales",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "UltimoCambio",
                table: "Sales",
                newName: "SaleDate");

            migrationBuilder.RenameColumn(
                name: "FechaEmision",
                table: "Sales",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "Company_ID",
                table: "Sales",
                newName: "WarehouseId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Sales",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Sales",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Sales",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CancelledByUserId",
                table: "Sales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ChangeAmount",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Sales",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Sales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "Sales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "Sales",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "Sales",
                type: "decimal(6,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceId",
                table: "Sales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceUuid",
                table: "Sales",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Sales",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPostedToInventory",
                table: "Sales",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Sales",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PostedToInventoryDate",
                table: "Sales",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PriceListId",
                table: "Sales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresInvoice",
                table: "Sales",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "SubTotal",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "Sales",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Sales",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Sales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SaleId",
                table: "InventoryMovements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SaleDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SaleId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(6,4)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxPercentage = table.Column<decimal>(type: "decimal(6,4)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LotNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleDetails_Sales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Sales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalePayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SaleId = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CardNumber = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    CardType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AuthorizationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransactionReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TerminalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransferReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CheckNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CheckBank = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalePayments_Sales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Sales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sales_Code",
                table: "Sales",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sales_CreatedByUserId",
                table: "Sales",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_CustomerId",
                table: "Sales",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_IsPostedToInventory",
                table: "Sales",
                column: "IsPostedToInventory");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_PriceListId",
                table: "Sales",
                column: "PriceListId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_SaleDate",
                table: "Sales",
                column: "SaleDate");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_Status",
                table: "Sales",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_UserId",
                table: "Sales",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_WarehouseId",
                table: "Sales",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_SaleId",
                table: "InventoryMovements",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleDetails_ProductId",
                table: "SaleDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleDetails_SaleId",
                table: "SaleDetails",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_SalePayments_PaymentDate",
                table: "SalePayments",
                column: "PaymentDate");

            migrationBuilder.CreateIndex(
                name: "IX_SalePayments_PaymentMethod",
                table: "SalePayments",
                column: "PaymentMethod");

            migrationBuilder.CreateIndex(
                name: "IX_SalePayments_SaleId",
                table: "SalePayments",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_SalePayments_Status",
                table: "SalePayments",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryMovements_Sales_SaleId",
                table: "InventoryMovements",
                column: "SaleId",
                principalTable: "Sales",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Customer_CustomerId",
                table: "Sales",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_PriceLists_PriceListId",
                table: "Sales",
                column: "PriceListId",
                principalTable: "PriceLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Users_CreatedByUserId",
                table: "Sales",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Users_UserId",
                table: "Sales",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Warehouses_WarehouseId",
                table: "Sales",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryMovements_Sales_SaleId",
                table: "InventoryMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Customer_CustomerId",
                table: "Sales");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_PriceLists_PriceListId",
                table: "Sales");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Users_CreatedByUserId",
                table: "Sales");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Users_UserId",
                table: "Sales");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Warehouses_WarehouseId",
                table: "Sales");

            migrationBuilder.DropTable(
                name: "SaleDetails");

            migrationBuilder.DropTable(
                name: "SalePayments");

            migrationBuilder.DropIndex(
                name: "IX_Sales_Code",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_CreatedByUserId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_CustomerId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_IsPostedToInventory",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_PriceListId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_SaleDate",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_Status",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_UserId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_WarehouseId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_InventoryMovements_SaleId",
                table: "InventoryMovements");

            migrationBuilder.DropColumn(
                name: "AmountPaid",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CancelledByUserId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "ChangeAmount",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "InvoiceUuid",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "IsPostedToInventory",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "PostedToInventoryDate",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "PriceListId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "RequiresInvoice",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "SubTotal",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "SaleId",
                table: "InventoryMovements");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Sales",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "WarehouseId",
                table: "Sales",
                newName: "Company_ID");

            migrationBuilder.RenameColumn(
                name: "SaleDate",
                table: "Sales",
                newName: "UltimoCambio");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Sales",
                newName: "FechaEmision");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Sales",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "Customer",
                table: "Sales",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "Sales",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Importe",
                table: "Sales",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Impuestos",
                table: "Sales",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Moneda",
                table: "Sales",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mov",
                table: "Sales",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MovID",
                table: "Sales",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioTotal",
                table: "Sales",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Saldo",
                table: "Sales",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "User",
                table: "Sales",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Warehouse",
                table: "Sales",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
