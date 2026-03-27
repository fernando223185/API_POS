using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBranchIdFromCxcTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicesPPD_Branches_BranchId",
                table: "InvoicesPPD");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentBatches_Branches_BranchId",
                table: "PaymentBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Branches_BranchId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_BranchId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_PaymentBatches_BranchId",
                table: "PaymentBatches");

            migrationBuilder.DropIndex(
                name: "IX_InvoicesPPD_BranchId",
                table: "InvoicesPPD");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "PaymentBatches");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "InvoicesPPD");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "PaymentBatches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "InvoicesPPD",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BranchId",
                table: "Payments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentBatches_BranchId",
                table: "PaymentBatches",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesPPD_BranchId",
                table: "InvoicesPPD",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicesPPD_Branches_BranchId",
                table: "InvoicesPPD",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentBatches_Branches_BranchId",
                table: "PaymentBatches",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Branches_BranchId",
                table: "Payments",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");
        }
    }
}
