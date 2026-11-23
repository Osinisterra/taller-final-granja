using AutoMapper;
using Granja.Domain.Enums;
using Granja.Infrastructure.Data;
using Granja.Infrastructure.Repositories;
using Granja.Services.Mapping;
using Granja.Services.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Granja.Tests.Support;

/// <summary>
/// Arma los servicios reales sobre SQLite en memoria (con migraciones) y un reloj falso,
/// para simular varios días como pide RN-10. Parámetros = escenario de referencia §8.
/// </summary>
public sealed class GranjaTestContext : IDisposable
{
    public static readonly DateOnly Dia1 = new(2026, 12, 1);

    private readonly SqliteConnection _connection;

    public GranjaDbContext Db { get; }
    public FakeClock Clock { get; } = new(Dia1);
    public IMapper Mapper { get; }

    public GranjaService Granja { get; }
    public VacaService Vacas { get; }
    public ProduccionService Produccion { get; }
    public InventarioService Inventario { get; }
    public VentaService Ventas { get; }
    public ReporteService Reportes { get; }

    public GranjaTestContext(int n = 10, decimal l = 15)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        Db = NuevoContexto();
        Db.Database.Migrate();

        DbInitializer.SembrarAsync(Db, Semilla(n, l), Dia1).GetAwaiter().GetResult();

        Mapper = CrearMapper();
        var uow = new UnitOfWork(Db);
        var granjas = new GranjaRepository(Db);
        var vacas = new VacaRepository(Db);
        var producciones = new ProduccionRepository(Db);
        var lotes = new LoteRepository(Db);
        var ventas = new VentaRepository(Db);

        Granja = new GranjaService(granjas, vacas, uow, Clock, Mapper);
        Vacas = new VacaService(vacas, Mapper);
        Produccion = new ProduccionService(granjas, vacas, producciones, lotes, uow, Clock, Mapper);
        Inventario = new InventarioService(lotes, granjas, uow, Clock, Mapper);
        Ventas = new VentaService(lotes, ventas, granjas, uow, Clock, Mapper);
        Reportes = new ReporteService(ventas, producciones, lotes, vacas, granjas, Mapper);
    }

    /// <summary>Contexto nuevo sobre la misma base: sirve para comprobar lo que realmente quedó guardado.</summary>
    public GranjaDbContext NuevoContexto() =>
        new(new DbContextOptionsBuilder<GranjaDbContext>().UseSqlite(_connection).Options);

    public static IMapper CrearMapper() =>
        new MapperConfiguration(cfg => cfg.AddProfile<GranjaMappingProfile>(), NullLoggerFactory.Instance).CreateMapper();

    public static DateOnly Dia(int numero) => Dia1.AddDays(numero - 1);

    private static GranjaSemillaOptions Semilla(int n, decimal l) => new()
    {
        Nombre = "Granja de pruebas",
        NumeroVacas = n,
        LitrosPorVaca = l,
        Productos =
        [
            new() { Tipo = TipoProducto.Queso, Rendimiento = 10, VidaUtilDias = 30, Precio = 28000, Unidad = "kg" },
            new() { Tipo = TipoProducto.Yogurt, Rendimiento = 1, VidaUtilDias = 15, Precio = 6000, Unidad = "L" }
        ]
    };

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}
