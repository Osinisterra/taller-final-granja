using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Granja.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Granjas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NumeroVacas = table.Column<int>(type: "INTEGER", nullable: false),
                    LitrosPorVaca = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Granjas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LotesProducto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FechaProduccion = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    FechaVencimiento = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    LitrosUsados = table.Column<double>(type: "REAL", nullable: false),
                    CantidadInicial = table.Column<double>(type: "REAL", nullable: false),
                    CantidadDisponible = table.Column<double>(type: "REAL", nullable: false),
                    CantidadDanada = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotesProducto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vacas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaRegistro = table.Column<DateOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vacas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Fecha = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Cantidad = table.Column<double>(type: "REAL", nullable: false),
                    PrecioUnitario = table.Column<double>(type: "REAL", nullable: false),
                    Total = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParametrosProducto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GranjaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Rendimiento = table.Column<double>(type: "REAL", nullable: false),
                    VidaUtilDias = table.Column<int>(type: "INTEGER", nullable: false),
                    Precio = table.Column<double>(type: "REAL", nullable: false),
                    Unidad = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParametrosProducto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParametrosProducto_Granjas_GranjaId",
                        column: x => x.GranjaId,
                        principalTable: "Granjas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProduccionesLeche",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VacaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Litros = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProduccionesLeche", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProduccionesLeche_Vacas_VacaId",
                        column: x => x.VacaId,
                        principalTable: "Vacas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VentaDetalles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VentaId = table.Column<int>(type: "INTEGER", nullable: false),
                    LoteId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaProduccionLote = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Cantidad = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentaDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentaDetalles_LotesProducto_LoteId",
                        column: x => x.LoteId,
                        principalTable: "LotesProducto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentaDetalles_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LotesProducto_Tipo_FechaProduccion",
                table: "LotesProducto",
                columns: new[] { "Tipo", "FechaProduccion" });

            migrationBuilder.CreateIndex(
                name: "IX_ParametrosProducto_GranjaId_Tipo",
                table: "ParametrosProducto",
                columns: new[] { "GranjaId", "Tipo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProduccionesLeche_Fecha",
                table: "ProduccionesLeche",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_ProduccionesLeche_VacaId_Fecha",
                table: "ProduccionesLeche",
                columns: new[] { "VacaId", "Fecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalles_LoteId",
                table: "VentaDetalles",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalles_VentaId",
                table: "VentaDetalles",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_Fecha",
                table: "Ventas",
                column: "Fecha");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParametrosProducto");

            migrationBuilder.DropTable(
                name: "ProduccionesLeche");

            migrationBuilder.DropTable(
                name: "VentaDetalles");

            migrationBuilder.DropTable(
                name: "Granjas");

            migrationBuilder.DropTable(
                name: "Vacas");

            migrationBuilder.DropTable(
                name: "LotesProducto");

            migrationBuilder.DropTable(
                name: "Ventas");
        }
    }
}
