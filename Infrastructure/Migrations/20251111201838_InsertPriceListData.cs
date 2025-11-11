using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InsertPriceListData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insertar datos iniciales de PriceList
            migrationBuilder.Sql(@"
                -- Insertar listas de precios básicas
                IF NOT EXISTS (SELECT 1 FROM [PriceLists] WHERE [Code] = 'MENUDEO')
                BEGIN
                    INSERT INTO [PriceLists] ([Name], [Description], [Code], [DefaultDiscountPercentage], [IsDefault], [IsActive], [CreatedAt])
                    VALUES 
                        ('Precio Menudeo', 'Lista de precios para venta al menudeo', 'MENUDEO', 0.0000, 1, 1, GETUTCDATE()),
                        ('Precio Mayoreo', 'Lista de precios para venta al mayoreo con descuento', 'MAYOREO', 10.0000, 0, 1, GETUTCDATE()),
                        ('Precio VIP', 'Lista de precios especiales para clientes VIP', 'VIP', 15.0000, 0, 1, GETUTCDATE()),
                        ('Precio Distribuidor', 'Lista de precios para distribuidores', 'DISTRIBUIDOR', 20.0000, 0, 1, GETUTCDATE());
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: Eliminar las listas de precios
            migrationBuilder.Sql(@"
                DELETE FROM [PriceLists] WHERE [Code] IN ('MENUDEO', 'MAYOREO', 'VIP', 'DISTRIBUIDOR');
            ");
        }
    }
}
