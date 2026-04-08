using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RevertPublicoGeneralAcento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Revertir el UPDATE incorrecto que afectó a todos los clientes
            // con TaxId = 'XAXX010101000'. Solo el cliente con Code = 'PUBLICO-GENERAL'
            // debe tener Name = N'PÚBLICO' y CompanyName = N'PÚBLICO EN GENERAL'.
            // Los demás clientes con ese RFC tienen CompanyName NULL (valor original).
            migrationBuilder.Sql(@"
                UPDATE [Customer]
                SET [CompanyName] = NULL
                WHERE [TaxId] = 'XAXX010101000'
                  AND [Code] != 'PUBLICO-GENERAL'
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
