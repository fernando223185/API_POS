using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModulePathIconAndMenuItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Modules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Modules",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "Modules",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MenuItems_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 13, 22, 30, 59, 583, DateTimeKind.Utc).AddTicks(6964));

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_ModuleId",
                table: "MenuItems",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_PermissionId",
                table: "MenuItems",
                column: "PermissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "Modules");

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

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Acceso completo al sistema", true, "Administrador" },
                    { 2, "Acceso básico al sistema", true, "Usuario" },
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
        }
    }
}
