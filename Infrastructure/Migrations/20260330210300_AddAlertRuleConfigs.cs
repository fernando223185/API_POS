using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertRuleConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TargetRoleId",
                table: "Alerts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AlertRuleConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlertType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    TargetRoleId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertRuleConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertRuleConfigs_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlertRuleConfigs_Roles_TargetRoleId",
                        column: x => x.TargetRoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertRuleConfigs_AlertType_CompanyId",
                table: "AlertRuleConfigs",
                columns: new[] { "AlertType", "CompanyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlertRuleConfigs_CompanyId",
                table: "AlertRuleConfigs",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertRuleConfigs_TargetRoleId",
                table: "AlertRuleConfigs",
                column: "TargetRoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertRuleConfigs");

            migrationBuilder.DropColumn(
                name: "TargetRoleId",
                table: "Alerts");
        }
    }
}
