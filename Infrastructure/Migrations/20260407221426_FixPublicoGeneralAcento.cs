using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPublicoGeneralAcento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE [Customer]
                SET [Name] = N'PÚBLICO',
                    [CompanyName] = N'PÚBLICO EN GENERAL'
                WHERE [TaxId] = 'XAXX010101000'
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE [Customer]
                SET [Name] = 'PUBLICO',
                    [CompanyName] = 'PUBLICO EN GENERAL'
                WHERE [TaxId] = 'XAXX010101000'
            ");
        }
    }
}
