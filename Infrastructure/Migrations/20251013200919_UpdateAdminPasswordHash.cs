using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 13, 20, 9, 19, 469, DateTimeKind.Utc).AddTicks(4225), new byte[] { 36, 11, 229, 24, 250, 189, 39, 36, 221, 182, 240, 78, 235, 29, 165, 150, 116, 72, 215, 232, 49, 192, 140, 143, 168, 34, 128, 159, 116, 199, 32, 169 } });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 13, 18, 40, 0, 918, DateTimeKind.Utc).AddTicks(1906), new byte[] { 97, 100, 109, 105, 110, 49, 50, 51 } });
        }
    }
}
