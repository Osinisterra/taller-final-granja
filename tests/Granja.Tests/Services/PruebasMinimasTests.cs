using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Services.Dtos;
using Granja.Tests.Support;
using Microsoft.EntityFrameworkCore;
using static Granja.Tests.Support.GranjaTestContext;

namespace Granja.Tests.Services;

/// <summary>Pruebas unitarias mínimas del punto 9 del taller.</summary>
public class PruebasMinimasTests : IDisposable
{
    private readonly GranjaTestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    private Task Producir(int dia) =>
        _ctx.Produccion.RegistrarProduccionAsync(new RegistrarProduccionRequest { Fecha = Dia(dia) });

    private Task<LoteResponse> Transformar(int dia, TipoProducto tipo, decimal litros) =>
        _ctx.Produccion.TransformarAsync(new TransformacionRequest { Fecha = Dia(dia), TipoProducto = tipo, Litros = litros });

    private Task<VentaResponse> Vender(int dia, TipoProducto tipo, decimal cantidad) =>
        _ctx.Ventas.RegistrarVentaAsync(new VentaRequest { Fecha = Dia(dia), TipoProducto = tipo, Cantidad = cantidad });

    [Fact]
    public async Task ProduccionDiaria_EsNPorL_YNoSeRegistraDosVecesLaMismaFecha()
    {
        var resultado = await _ctx.Produccion.RegistrarProduccionAsync(new RegistrarProduccionRequest { Fecha = Dia(1) });

        Assert.Equal(150, resultado.LitrosProducidos);
        Assert.Equal(10, resultado.NumeroVacas);
        await Assert.ThrowsAsync<ProduccionDuplicadaException>(() => Producir(1));

        await using var db = _ctx.NuevoContexto();
        Assert.Equal(10, await db.Producciones.CountAsync(p => p.Fecha == Dia(1)));
    }

    [Fact]
    public async Task ProduccionSinFecha_UsaLaFechaDelReloj()
    {
        _ctx.Clock.Today = Dia(5);
        var resultado = await _ctx.Produccion.RegistrarProduccionAsync(new RegistrarProduccionRequest());
        Assert.Equal(Dia(5), resultado.Fecha);
    }

    [Fact]
    public async Task NoSePuedenTransformarMasLitrosQueLosDisponiblesDelDia()
    {
        await Producir(1);
        await Transformar(1, TipoProducto.Queso, 100);

        await Assert.ThrowsAsync<LecheInsuficienteException>(() => Transformar(1, TipoProducto.Yogurt, 51));

        var dia = await _ctx.Produccion.ObtenerDiaAsync(Dia(1));
        Assert.Equal(100, dia.LitrosTransformados);
        Assert.Equal(50, dia.LitrosDisponibles);
    }

    [Fact]
    public async Task TransformarSinProduccionEnLaFecha_LanzaNoEncontrado()
    {
        await Assert.ThrowsAsync<EntidadNoEncontradaException>(() => Transformar(3, TipoProducto.Queso, 10));
    }

    [Fact]
    public async Task Venta_TomaDelLoteMasAntiguoAntesQueDelMasNuevo()
    {
        await Producir(1);
        await Producir(2);
        var nuevo = await Transformar(2, TipoProducto.Queso, 150); // se crea primero, pero es más reciente
        var antiguo = await Transformar(1, TipoProducto.Queso, 100);

        var venta = await Vender(3, TipoProducto.Queso, 5);

        var detalle = Assert.Single(venta.Detalle);
        Assert.Equal(antiguo.Id, detalle.LoteId);
        Assert.Equal(5, (await _ctx.Inventario.ObtenerLoteAsync(antiguo.Id, Dia(3))).CantidadDisponible);
        Assert.Equal(15, (await _ctx.Inventario.ObtenerLoteAsync(nuevo.Id, Dia(3))).CantidadDisponible);
    }

    [Fact]
    public async Task Venta_QueExcedeUnLote_ConsumeElSiguienteYGeneraVariosDetalles()
    {
        await Producir(1);
        await Producir(2);
        var q1 = await Transformar(1, TipoProducto.Queso, 100);
        var q2 = await Transformar(2, TipoProducto.Queso, 150);

        var venta = await Vender(3, TipoProducto.Queso, 12);

        Assert.Collection(venta.Detalle,
            d => { Assert.Equal(q1.Id, d.LoteId); Assert.Equal(10, d.Cantidad); },
            d => { Assert.Equal(q2.Id, d.LoteId); Assert.Equal(2, d.Cantidad); });
        Assert.Equal(336000, venta.Total);
    }

    [Fact]
    public async Task LoteVencido_NuncaSeVende_AunqueSeaElMasAntiguo()
    {
        await Producir(1);
        await Producir(10);
        var viejo = await Transformar(1, TipoProducto.Yogurt, 50);  // vence el día 16
        var nuevo = await Transformar(10, TipoProducto.Yogurt, 40); // vence el día 25

        var venta = await Vender(17, TipoProducto.Yogurt, 5);

        var detalle = Assert.Single(venta.Detalle);
        Assert.Equal(nuevo.Id, detalle.LoteId);
        Assert.Equal(50, (await _ctx.Inventario.ObtenerLoteAsync(viejo.Id, Dia(17))).CantidadDisponible);
    }

    [Fact]
    public async Task VentaConExistenciasInsuficientes_NoModificaElInventario()
    {
        await Producir(1);
        await Producir(2);
        await Transformar(1, TipoProducto.Queso, 100);
        await Transformar(2, TipoProducto.Queso, 150);

        var ex = await Assert.ThrowsAsync<InventarioInsuficienteException>(() => Vender(3, TipoProducto.Queso, 26));
        Assert.Equal(25, ex.Disponible);

        await using var db = _ctx.NuevoContexto();
        Assert.Equal(25, (await db.Lotes.ToListAsync()).Sum(l => l.CantidadDisponible));
        Assert.Equal(0, await db.Ventas.CountAsync());
        Assert.Equal(0, await db.VentaDetalles.CountAsync());
    }

    [Fact]
    public async Task ProcesarVencimientos_PasaDisponibleADanado_YElReporteLoRefleja()
    {
        await Producir(1);
        await Transformar(1, TipoProducto.Yogurt, 50);
        await Vender(3, TipoProducto.Yogurt, 20);

        var proceso = await _ctx.Inventario.ProcesarVencimientosAsync(new ProcesarVencimientosRequest { Fecha = Dia(17) });

        Assert.Equal(1, proceso.LotesAfectados);
        Assert.Equal(30, proceso.Lotes[0].CantidadDanada);

        var danado = await _ctx.Reportes.ProductoDanadoAsync(null, null);
        Assert.Equal(30, Assert.Single(danado.TotalPorTipo).Cantidad);

        var total = (await _ctx.Reportes.ProductoTotalAsync()).Single(p => p.TipoProducto == TipoProducto.Yogurt);
        Assert.Equal(0, total.Disponible);
        Assert.Equal(30, total.Danado);

        // Procesar de nuevo no vuelve a contar el daño.
        var segundo = await _ctx.Inventario.ProcesarVencimientosAsync(new ProcesarVencimientosRequest { Fecha = Dia(17) });
        Assert.Equal(0, segundo.LotesAfectados);
    }
}
