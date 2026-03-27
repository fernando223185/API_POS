using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CleanPaymentFieldNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethodSAT",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "DefaultPaymentMethodSAT",
                table: "PaymentBatches",
                newName: "PaymentFormSAT");

            migrationBuilder.RenameColumn(
                name: "DefaultBankDestination",
                table: "PaymentBatches",
                newName: "BankDestination");

            migrationBuilder.RenameColumn(
                name: "DefaultAccountDestination",
                table: "PaymentBatches",
                newName: "AccountDestination");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentFormSAT",
                table: "Payments",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(2)",
                oldMaxLength: 2,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentFormSAT",
                table: "PaymentBatches",
                newName: "DefaultPaymentMethodSAT");

            migrationBuilder.RenameColumn(
                name: "BankDestination",
                table: "PaymentBatches",
                newName: "DefaultBankDestination");

            migrationBuilder.RenameColumn(
                name: "AccountDestination",
                table: "PaymentBatches",
                newName: "DefaultAccountDestination");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentFormSAT",
                table: "Payments",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2)",
                oldMaxLength: 2);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethodSAT",
                table: "Payments",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");
        }
    }
}
