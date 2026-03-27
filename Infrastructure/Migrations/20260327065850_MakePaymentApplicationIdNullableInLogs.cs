using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakePaymentApplicationIdNullableInLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentComplementLogs_PaymentApplications_PaymentApplicationId",
                table: "PaymentComplementLogs");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentApplicationId",
                table: "PaymentComplementLogs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentComplementLogs_PaymentApplications_PaymentApplicationId",
                table: "PaymentComplementLogs",
                column: "PaymentApplicationId",
                principalTable: "PaymentApplications",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentComplementLogs_PaymentApplications_PaymentApplicationId",
                table: "PaymentComplementLogs");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentApplicationId",
                table: "PaymentComplementLogs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentComplementLogs_PaymentApplications_PaymentApplicationId",
                table: "PaymentComplementLogs",
                column: "PaymentApplicationId",
                principalTable: "PaymentApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
