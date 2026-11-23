using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Services.Dtos;
using Granja.Tests.Support;
using static Granja.Tests.Support.GranjaTestContext;

namespace Granja.Tests.Services;

/// <summary>Escenario de prueba de referencia del punto 8 del taller (n = 10, l = 15).</summary>
public class EscenarioReferenciaTests : IDisposable
{
    private readonly GranjaTestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    [Fact]
    public async Task EscenarioCompleto_ProduceLosResultadosEsperados()
    {
        // Día 1: 150 L → 100 L a queso y 50 L a yogurt.
        await _ctx.Produccion.RegistrarProduccionAsync(new RegistrarProduccionRequest { Fecha = Dia(1) });
        var q1 = await _ctx.Produccion.TransformarAsync(new TransformacionRequest { Fecha = Dia(1), TipoProducto = TipoProducto.Queso, Litros = 100 });
        var y1 = await _ctx.Produccion.TransformarAsync(new TransformacionRequest { Fecha = Dia(1), TipoProducto = TipoProducto.Yogurt, Litros = 50 });
        Assert.Equal(10, q1.CantidadInicial);
        Assert.Equal(50, y1.CantidadInicial);
        Assert.Equal(Dia(16), y1.FechaVencimiento);

        // Día 2: 150 L a queso.
        await _ctx.Produccion.RegistrarProduccionAsync(new RegistrarProduccionRequest { Fecha = Dia(2) });
        var q2 = await _ctx.Produccion.TransformarAsync(new TransformacionRequest { Fecha = Dia(2), TipoProducto = TipoProducto.Queso, Litros = 150 });
        Assert.Equal(15, q2.CantidadInicial);

        // Día 3: venta de 12 kg de queso (Q1 10 + Q2 2) y de 20 L de yogurt.
        var ventaQueso = await _ctx.Ventas.RegistrarVentaAsync(new VentaRequest { Fecha = Dia(3), TipoProducto = TipoProducto.Queso, Cantidad = 12 });
        Assert.Equal(28000, ventaQueso.PrecioUnitario);
        Assert.Equal(336000, ventaQueso.Total);
        Assert.Equal(new[] { (q1.Id, 10m), (q2.Id, 2m) }, ventaQueso.Detalle.Select(d => (d.LoteId, d.Cantidad)));
        Assert.Equal(0, (await _ctx.Inventario.ObtenerLoteAsync(q1.Id, Dia(3))).CantidadDisponible);
        Assert.Equal(13, (await _ctx.Inventario.ObtenerLoteAsync(q2.Id, Dia(3))).CantidadDisponible);

        await _ctx.Ventas.RegistrarVentaAsync(new VentaRequest { Fecha = Dia(3), TipoProducto = TipoProducto.Yogurt, Cantidad = 20 });
        Assert.Equal(30, (await _ctx.Inventario.ObtenerLoteAsync(y1.Id, Dia(3))).CantidadDisponible);

        // Día 17: procesar vencimientos y venta de 5 L de yogurt → 409.
        var proceso = await _ctx.Inventario.ProcesarVencimientosAsync(new ProcesarVencimientosRequest { Fecha = Dia(17) });
        Assert.Equal(30, Assert.Single(proceso.Lotes).CantidadDanada);
        await Assert.ThrowsAsync<InventarioInsuficienteException>(() =>
            _ctx.Ventas.RegistrarVentaAsync(new VentaRequest { Fecha = Dia(17), TipoProducto = TipoProducto.Yogurt, Cantidad = 5 }));

        // Reporte de producto total.
        var total = await _ctx.Reportes.ProductoTotalAsync();
        var queso = total.Single(t => t.TipoProducto == TipoProducto.Queso);
        var yogurt = total.Single(t => t.TipoProducto == TipoProducto.Yogurt);
        Assert.Equal((25m, 12m, 13m, 0m), (queso.Producido, queso.Vendido, queso.Disponible, queso.Danado));
        Assert.Equal((50m, 20m, 0m, 30m), (yogurt.Producido, yogurt.Vendido, yogurt.Disponible, yogurt.Danado));

        // Reporte de leche por día: 150 L el día 1 y 150 L el día 2.
        var leche = await _ctx.Reportes.LecheDiariaAsync(null, null);
        Assert.Equal(new[] { (Dia(1), 150m), (Dia(2), 150m) }, leche.Dias.Select(d => (d.Fecha, d.LitrosProducidos)));
        Assert.All(leche.Dias, d => Assert.Equal(0, d.LitrosNoAprovechados));

        // Reporte de ventas.
        var ventas = await _ctx.Reportes.VentasAsync(null, null, null);
        Assert.Equal(2, ventas.NumeroVentas);
        Assert.Equal(336000 + 20 * 6000, ventas.TotalGeneral);

        // Reporte de producto diario.
        var diario = await _ctx.Reportes.ProductoDiarioAsync(null, null);
        Assert.Equal(3, diario.Count);
        Assert.Equal(15, diario.Single(d => d.Fecha == Dia(2)).CantidadProducida);
    }
}
