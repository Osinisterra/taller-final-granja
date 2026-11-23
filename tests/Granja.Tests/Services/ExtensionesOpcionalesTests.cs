using System.Text;
using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Domain.Interfaces;
using Granja.Services.Dtos;
using Granja.Tests.Support;
using static Granja.Tests.Support.GranjaTestContext;

namespace Granja.Tests.Services;

/// <summary>Extensiones opcionales del §12: paginación, filtros avanzados y exportación a CSV.</summary>
public class ExtensionesOpcionalesTests : IDisposable
{
    private readonly GranjaTestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    /// <summary>Carga el escenario de referencia (§8) hasta el día 17.</summary>
    private async Task CargarEscenarioAsync()
    {
        await _ctx.Produccion.RegistrarProduccionAsync(new RegistrarProduccionRequest { Fecha = Dia(1) });
        await _ctx.Produccion.TransformarAsync(new TransformacionRequest { Fecha = Dia(1), TipoProducto = TipoProducto.Queso, Litros = 100 });
        await _ctx.Produccion.TransformarAsync(new TransformacionRequest { Fecha = Dia(1), TipoProducto = TipoProducto.Yogurt, Litros = 50 });
        await _ctx.Produccion.RegistrarProduccionAsync(new RegistrarProduccionRequest { Fecha = Dia(2) });
        await _ctx.Produccion.TransformarAsync(new TransformacionRequest { Fecha = Dia(2), TipoProducto = TipoProducto.Queso, Litros = 150 });
        await _ctx.Ventas.RegistrarVentaAsync(new VentaRequest { Fecha = Dia(3), TipoProducto = TipoProducto.Queso, Cantidad = 12 });
        await _ctx.Ventas.RegistrarVentaAsync(new VentaRequest { Fecha = Dia(3), TipoProducto = TipoProducto.Yogurt, Cantidad = 20 });
        await _ctx.Inventario.ProcesarVencimientosAsync(new ProcesarVencimientosRequest { Fecha = Dia(17) });
    }

    private async Task<string> CsvAsync(string reporte)
    {
        var archivo = await _ctx.Exportar.ExportarCsvAsync(reporte, null, null, null);
        Assert.Equal("text/csv", archivo.ContentType);
        Assert.Equal(Encoding.UTF8.GetPreamble(), archivo.Contenido.Take(3));
        return Encoding.UTF8.GetString(archivo.Contenido.Skip(3).ToArray());
    }

    [Fact]
    public async Task Ventas_SePaginanEnLaBaseDeDatos()
    {
        await _ctx.Produccion.RegistrarProduccionAsync(new RegistrarProduccionRequest { Fecha = Dia(1) });
        await _ctx.Produccion.TransformarAsync(new TransformacionRequest { Fecha = Dia(1), TipoProducto = TipoProducto.Yogurt, Litros = 150 });
        for (var i = 1; i <= 5; i++)
            await _ctx.Ventas.RegistrarVentaAsync(new VentaRequest { Fecha = Dia(2), TipoProducto = TipoProducto.Yogurt, Cantidad = i });

        var pagina2 = await _ctx.Ventas.ListarAsync(new FiltroVentasRequest { Pagina = 2, TamanoPagina = 2 });

        Assert.Equal(5, pagina2.TotalRegistros);
        Assert.Equal(3, pagina2.TotalPaginas);
        Assert.Equal(new[] { 3m, 4m }, pagina2.Items.Select(v => v.Cantidad));
        Assert.True(pagina2.TieneAnterior);
        Assert.True(pagina2.TieneSiguiente);
    }

    [Fact]
    public async Task Ventas_FiltrosAvanzadosYOrden()
    {
        await CargarEscenarioAsync();

        var porTotalDesc = await _ctx.Ventas.ListarAsync(new FiltroVentasRequest { OrdenarPor = OrdenVentas.Total, Descendente = true });
        Assert.Equal(new[] { 336000m, 120000m }, porTotalDesc.Items.Select(v => v.Total));

        var grandes = await _ctx.Ventas.ListarAsync(new FiltroVentasRequest { CantidadMinima = 15 });
        Assert.Equal(TipoProducto.Yogurt, Assert.Single(grandes.Items).TipoProducto);

        var queso = await _ctx.Ventas.ListarAsync(new FiltroVentasRequest { Tipo = TipoProducto.Queso, Desde = Dia(3), Hasta = Dia(3) });
        Assert.Equal(2, Assert.Single(queso.Items).Detalle.Count);

        await Assert.ThrowsAsync<DomainException>(() =>
            _ctx.Ventas.ListarAsync(new FiltroVentasRequest { CantidadMinima = 10, CantidadMaxima = 5 }));
    }

    [Fact]
    public async Task Inventario_FiltraPorEstadoYTipo()
    {
        await CargarEscenarioAsync();

        var vencidos = await _ctx.Inventario.ListarLotesAsync(new FiltroLotesRequest { Estado = EstadoLote.Vencido, Fecha = Dia(17) });
        Assert.Equal(TipoProducto.Yogurt, Assert.Single(vencidos.Items).TipoProducto);

        var agotados = await _ctx.Inventario.ListarLotesAsync(new FiltroLotesRequest { Estado = EstadoLote.Agotado, Fecha = Dia(17) });
        Assert.Equal(10, Assert.Single(agotados.Items).CantidadInicial);

        var quesoVendible = await _ctx.Inventario.ListarLotesAsync(new FiltroLotesRequest
        {
            Tipo = TipoProducto.Queso, SoloDisponibles = true, Fecha = Dia(17), Desde = Dia(2), Hasta = Dia(2)
        });
        Assert.Equal(13, Assert.Single(quesoVendible.Items).CantidadDisponible);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task PaginacionInvalida_LanzaDomainException(int pagina, int tamano)
    {
        await Assert.ThrowsAsync<DomainException>(() =>
            _ctx.Vacas.ListarAsync(false, new PaginacionRequest { Pagina = pagina, TamanoPagina = tamano }));
    }

    [Fact]
    public async Task Vacas_PaginaFueraDeRango_DevuelveListaVacia()
    {
        var pagina = await _ctx.Vacas.ListarAsync(false, new PaginacionRequest { Pagina = 5, TamanoPagina = 4 });

        Assert.Empty(pagina.Items);
        Assert.Equal(10, pagina.TotalRegistros);
        Assert.Equal(3, pagina.TotalPaginas);
    }

    [Fact]
    public async Task ExportarProductoTotal_GeneraCsvConLosDatosDelEscenario()
    {
        await CargarEscenarioAsync();

        var lineas = (await CsvAsync("producto-total")).Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("Producto,Unidad,Producido,Vendido,Disponible,Danado,Ingresos", lineas[0]);
        Assert.Equal("Queso,kg,25,12,13,0,336000", lineas[1]);
        Assert.Equal("Yogurt,L,50,20,0,30,120000", lineas[2]);
    }

    [Fact]
    public async Task ExportarVentasYLecheDiaria_IncluyeTotales()
    {
        await CargarEscenarioAsync();

        Assert.Contains("TOTAL GENERAL,,456000", await CsvAsync("ventas"));
        var leche = await CsvAsync("leche-diaria");
        Assert.Contains("2026-12-01,10,150,150,0", leche);
        Assert.Contains("TOTAL,,300,300,0", leche);
    }

    [Theory]
    [InlineData("producto-diario")]
    [InlineData("producto-danado")]
    [InlineData("vacas")]
    public async Task ExportarOtrosReportes_GeneraEncabezado(string reporte)
    {
        await CargarEscenarioAsync();
        Assert.False(string.IsNullOrWhiteSpace(await CsvAsync(reporte)));
    }

    [Fact]
    public async Task ExportarReporteDesconocido_LanzaDomainException()
    {
        await Assert.ThrowsAsync<DomainException>(() => _ctx.Exportar.ExportarCsvAsync("inexistente", null, null, null));
    }
}
