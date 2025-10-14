using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissionSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Roles",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Resource = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ModuleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Modules",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Customer Relationship Management", true, "CRM" },
                    { 2, "Sales Management", true, "Sales" },
                    { 3, "Product Management", true, "Products" },
                    { 4, "User Management", true, "Users" }
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "IsActive",
                value: true);

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 3, "Acceso a ventas y clientes", true, "Vendedor" },
                    { 4, "Acceso a productos", true, "Almacenista" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(6292));

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Description", "ModuleId", "Name", "Resource" },
                values: new object[,]
                {
                    { 1, "Crear clientes", 1, "Create", "Customer" },
                    { 2, "Ver clientes", 1, "Read", "Customer" },
                    { 3, "Actualizar clientes", 1, "Update", "Customer" },
                    { 4, "Eliminar clientes", 1, "Delete", "Customer" },
                    { 5, "Crear ventas", 2, "Create", "Sale" },
                    { 6, "Ver ventas", 2, "Read", "Sale" },
                    { 7, "Actualizar ventas", 2, "Update", "Sale" },
                    { 8, "Eliminar ventas", 2, "Delete", "Sale" },
                    { 9, "Crear productos", 3, "Create", "Product" },
                    { 10, "Ver productos", 3, "Read", "Product" },
                    { 11, "Actualizar productos", 3, "Update", "Product" },
                    { 12, "Eliminar productos", 3, "Delete", "Product" },
                    { 13, "Crear usuarios", 4, "Create", "User" },
                    { 14, "Ver usuarios", 4, "Read", "User" },
                    { 15, "Actualizar usuarios", 4, "Update", "User" },
                    { 16, "Eliminar usuarios", 4, "Delete", "User" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "CreatedAt", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5732), 1, 1 },
                    { 2, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5734), 2, 1 },
                    { 3, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5735), 3, 1 },
                    { 4, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5735), 4, 1 },
                    { 5, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5735), 5, 1 },
                    { 6, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5736), 6, 1 },
                    { 7, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5736), 7, 1 },
                    { 8, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5737), 8, 1 },
                    { 9, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5737), 9, 1 },
                    { 10, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5737), 10, 1 },
                    { 11, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5738), 11, 1 },
                    { 12, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5738), 12, 1 },
                    { 13, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5738), 13, 1 },
                    { 14, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5738), 14, 1 },
                    { 15, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5738), 15, 1 },
                    { 16, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5739), 16, 1 },
                    { 17, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5755), 2, 2 },
                    { 18, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5756), 6, 2 },
                    { 19, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5757), 10, 2 },
                    { 20, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5764), 1, 3 },
                    { 21, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5764), 2, 3 },
                    { 22, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5765), 3, 3 },
                    { 23, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5765), 5, 3 },
                    { 24, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5766), 6, 3 },
                    { 25, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5766), 7, 3 },
                    { 26, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5767), 10, 3 },
                    { 27, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5775), 9, 4 },
                    { 28, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5775), 10, 4 },
                    { 29, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5776), 11, 4 },
                    { 30, new DateTime(2025, 10, 13, 22, 0, 56, 643, DateTimeKind.Utc).AddTicks(5776), 12, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ModuleId",
                table: "Permissions",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Roles");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Roles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 13, 20, 9, 19, 469, DateTimeKind.Utc).AddTicks(4225));
        }
    }
}
