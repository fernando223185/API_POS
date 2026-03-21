using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSatCatalogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SatFormaPago",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Bancarizado = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    NumeroOperacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RfcEmisorCtaOrdenante = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CtaOrdenante = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RfcEmisorCtaBeneficiario = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CtaBeneficiario = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TipoCadenaPago = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaInicioVigencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinVigencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatFormaPago", x => x.Codigo);
                });

            migrationBuilder.CreateTable(
                name: "SatMetodoPago",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FechaInicioVigencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinVigencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatMetodoPago", x => x.Codigo);
                });

            migrationBuilder.CreateTable(
                name: "SatProductoServicio",
                columns: table => new
                {
                    ClaveProdServ = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IncluyeIva = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IncluyeIeps = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Complemento = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PalabrasSimilares = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FechaInicioVigencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinVigencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatProductoServicio", x => x.ClaveProdServ);
                });

            migrationBuilder.CreateTable(
                name: "SatRegimenFiscal",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    AplicaPersonaFisica = table.Column<bool>(type: "bit", nullable: false),
                    AplicaPersonaMoral = table.Column<bool>(type: "bit", nullable: false),
                    FechaInicioVigencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinVigencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatRegimenFiscal", x => x.Codigo);
                });

            migrationBuilder.CreateTable(
                name: "SatTipoComprobante",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ValorMaximo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaInicioVigencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinVigencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatTipoComprobante", x => x.Codigo);
                });

            migrationBuilder.CreateTable(
                name: "SatUnidadMedida",
                columns: table => new
                {
                    ClaveUnidad = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Nota = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Simbolo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FechaInicioVigencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinVigencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatUnidadMedida", x => x.ClaveUnidad);
                });

            migrationBuilder.CreateTable(
                name: "SatUsoCfdi",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    AplicaPersonaFisica = table.Column<bool>(type: "bit", nullable: false),
                    AplicaPersonaMoral = table.Column<bool>(type: "bit", nullable: false),
                    FechaInicioVigencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinVigencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegimenFiscalReceptor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatUsoCfdi", x => x.Codigo);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SatFormaPago");

            migrationBuilder.DropTable(
                name: "SatMetodoPago");

            migrationBuilder.DropTable(
                name: "SatProductoServicio");

            migrationBuilder.DropTable(
                name: "SatRegimenFiscal");

            migrationBuilder.DropTable(
                name: "SatTipoComprobante");

            migrationBuilder.DropTable(
                name: "SatUnidadMedida");

            migrationBuilder.DropTable(
                name: "SatUsoCfdi");
        }
    }
}
