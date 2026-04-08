using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedPublicoGeneral : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ─── Público en General ────────────────────────────────────────────
            // Datos conforme a la guía de llenado de CFDI 4.0 del SAT:
            //   RFC:            XAXX010101000  (RFC genérico obligatorio)
            //   Nombre:         PUBLICO EN GENERAL
            //   UsoCFDI:        S01 - Sin efectos fiscales
            //   RegimenFiscal:  616 - Sin obligaciones fiscales
            //   DomicilioFiscal/ZipCode: 00000 (CP genérico cuando no se conoce)
            // ──────────────────────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Customer",
                columns: new[]
                {
                    "Name", "LastName", "Code", "Email", "Phone",
                    "Address", "TaxId", "ZipCode", "Commentary",
                    "CountryId", "StateId",
                    "InteriorNumber", "ExteriorNumber",
                    "StatusId", "IsActive",
                    "CompanyName", "SatTaxRegime", "SatCfdiUse",
                    "Created_at"
                },
                values: new object[]
                {
                    "PUBLICO", "EN GENERAL", "PUBLICO-GENERAL",
                    "publico@general.com", "0000000000",
                    "SIN DIRECCION FISCAL", "XAXX010101000", "00000", "",
                    1, 1,
                    "S/N", "S/N",
                    1, true,
                    "PUBLICO EN GENERAL", "616", "S01",
                    DateTime.UtcNow
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Customer",
                keyColumn: "TaxId",
                keyValue: "XAXX010101000");
        }
    }
}
