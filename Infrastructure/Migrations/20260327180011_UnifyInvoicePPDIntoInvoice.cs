using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnifyInvoicePPDIntoInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Eliminar FK e índice de InvoicePPDId → InvoicesPPD
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentApplications_InvoicesPPD_InvoicePPDId",
                table: "PaymentApplications");

            migrationBuilder.DropIndex(
                name: "IX_PaymentApplications_InvoicePPDId",
                table: "PaymentApplications");

            // 2. Eliminar columna InvoicePPDId
            migrationBuilder.DropColumn(
                name: "InvoicePPDId",
                table: "PaymentApplications");

            // 3. Eliminar tabla InvoicesPPD
            migrationBuilder.DropTable(
                name: "InvoicesPPD");

            // 4. Agregar columna InvoiceId y crear FK → Invoices
            migrationBuilder.AddColumn<int>(
                name: "InvoiceId",
                table: "PaymentApplications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 5. Limpiar datos huérfanos (empezando de cero)
            migrationBuilder.Sql("DELETE FROM PaymentApplications WHERE InvoiceId = 0 OR InvoiceId NOT IN (SELECT Id FROM Invoices)");

            // 6. Agregar campos PPD a Invoices

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DaysOverdue",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPaymentDate",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NextPartialityNumber",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Invoices",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalPartialities",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentApplications_InvoiceId",
                table: "PaymentApplications",
                column: "InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentApplications_Invoices_InvoiceId",
                table: "PaymentApplications",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Eliminar FK e índice de InvoiceId
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentApplications_Invoices_InvoiceId",
                table: "PaymentApplications");

            migrationBuilder.DropIndex(
                name: "IX_PaymentApplications_InvoiceId",
                table: "PaymentApplications");

            // 2. Eliminar columna InvoiceId
            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "PaymentApplications");

            // 3. Eliminar campos PPD de Invoices
            migrationBuilder.DropColumn(
                name: "BalanceAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DaysOverdue",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "LastPaymentDate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "NextPartialityNumber",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "TotalPartialities",
                table: "Invoices");

            // NOTA: Para revertir completamente, necesitarías recrear la tabla InvoicesPPD
            // y restaurar la columna InvoicePPDId en PaymentApplications.
            // Esto no está implementado porque es un downgrade complejo.
            throw new NotSupportedException(
                "No se puede revertir esta migración. " +
                "InvoicesPPD fue eliminada. Restaura desde backup si necesitas revertir.");
        }
    }
}
