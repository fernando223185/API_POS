using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameToModulesAndSubmodules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppSubmodules_AppModules_ModuleId",
                table: "AppSubmodules");

            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Modules_ModuleId",
                table: "Permissions");

            migrationBuilder.DropTable(
                name: "AppModules");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_ModuleId",
                table: "Permissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppSubmodules",
                table: "AppSubmodules");

            migrationBuilder.DropColumn(
                name: "ModuleId",
                table: "Permissions");

            migrationBuilder.RenameTable(
                name: "AppSubmodules",
                newName: "Submodules");

            migrationBuilder.RenameIndex(
                name: "IX_AppSubmodules_Order",
                table: "Submodules",
                newName: "IX_Submodules_Order");

            migrationBuilder.RenameIndex(
                name: "IX_AppSubmodules_ModuleId",
                table: "Submodules",
                newName: "IX_Submodules_ModuleId");

            migrationBuilder.RenameIndex(
                name: "IX_AppSubmodules_IsActive",
                table: "Submodules",
                newName: "IX_Submodules_IsActive");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Modules",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Modules",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Modules",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Modules",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Modules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "Modules",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Modules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Submodules",
                table: "Submodules",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_IsActive",
                table: "Modules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_Order",
                table: "Modules",
                column: "Order");

            migrationBuilder.AddForeignKey(
                name: "FK_Submodules_Modules_ModuleId",
                table: "Submodules",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Submodules_Modules_ModuleId",
                table: "Submodules");

            migrationBuilder.DropIndex(
                name: "IX_Modules_IsActive",
                table: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_Modules_Order",
                table: "Modules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Submodules",
                table: "Submodules");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Modules");

            migrationBuilder.RenameTable(
                name: "Submodules",
                newName: "AppSubmodules");

            migrationBuilder.RenameIndex(
                name: "IX_Submodules_Order",
                table: "AppSubmodules",
                newName: "IX_AppSubmodules_Order");

            migrationBuilder.RenameIndex(
                name: "IX_Submodules_ModuleId",
                table: "AppSubmodules",
                newName: "IX_AppSubmodules_ModuleId");

            migrationBuilder.RenameIndex(
                name: "IX_Submodules_IsActive",
                table: "AppSubmodules",
                newName: "IX_AppSubmodules_IsActive");

            migrationBuilder.AddColumn<int>(
                name: "ModuleId",
                table: "Permissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Modules",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Modules",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppSubmodules",
                table: "AppSubmodules",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AppModules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppModules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ModuleId",
                table: "Permissions",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_AppModules_IsActive",
                table: "AppModules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AppModules_Order",
                table: "AppModules",
                column: "Order");

            migrationBuilder.AddForeignKey(
                name: "FK_AppSubmodules_AppModules_ModuleId",
                table: "AppSubmodules",
                column: "ModuleId",
                principalTable: "AppModules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Modules_ModuleId",
                table: "Permissions",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
