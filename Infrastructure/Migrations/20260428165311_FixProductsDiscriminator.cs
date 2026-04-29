using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixProductsDiscriminator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ Actualizar todos los registros con discriminador vacío o NULL a "Products"
            migrationBuilder.Sql(@"
                UPDATE Products 
                SET Discriminator = 'Products'
                WHERE Discriminator = '' OR Discriminator IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No es necesario revertir, pero por consistencia:
            // No hacer nada porque no queremos volver a dejar discriminadores vacíos
        }
    }
}
