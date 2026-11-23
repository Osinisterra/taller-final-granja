using Granja.Domain.Entities;
using Granja.Domain.Enums;
using Granja.Domain.Exceptions;

namespace Granja.Tests.Domain;

public class LoteProductoTests
{
    private static readonly DateOnly Hoy = new(2026, 12, 1);
    private static ParametroProducto Queso => new(TipoProducto.Queso, 10, 30, 28000, "kg");
    private static ParametroProducto Yogurt => new(TipoProducto.Yogurt, 1, 15, 6000, "L");

    [Fact]
    public void Crear_CalculaCantidadPorRendimiento_YVencimientoPorVidaUtil()
    {
        var lote = LoteProducto.Crear(Hoy, 100, Queso);

        Assert.Equal(10, lote.CantidadInicial);
        Assert.Equal(10, lote.CantidadDisponible);
        Assert.Equal(Hoy.AddDays(30), lote.FechaVencimiento);
        Assert.Equal(100, lote.LitrosUsados);
    }

    [Fact]
    public void EstaVencido_SoloCuandoLaFechaEsPosteriorAlVencimiento()
    {
        var lote = LoteProducto.Crear(Hoy, 50, Yogurt); // vence el día 16

        Assert.False(lote.EstaVencido(Hoy.AddDays(15)));
        Assert.True(lote.EstaVencido(Hoy.AddDays(16)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    public void Descontar_CantidadInvalida_Lanza(decimal cantidad)
    {
        var lote = LoteProducto.Crear(Hoy, 100, Queso);
        Assert.Throws<DomainException>(() => lote.Descontar(cantidad));
    }

    [Fact]
    public void MarcarComoDanado_PasaElDisponibleADanado()
    {
        var lote = LoteProducto.Crear(Hoy, 50, Yogurt);
        lote.Descontar(20);

        var perdida = lote.MarcarComoDanado();

        Assert.Equal(30, perdida);
        Assert.Equal(0, lote.CantidadDisponible);
        Assert.Equal(30, lote.CantidadDanada);
        Assert.Equal(20, lote.CantidadVendida);
        Assert.Equal(EstadoLote.Vencido, lote.Estado(Hoy.AddDays(16)));
    }

    [Fact]
    public void Venta_NoPermiteAgregarDetalleDeLoteVencido()
    {
        var lote = LoteProducto.Crear(Hoy, 50, Yogurt);
        var venta = new Venta(Hoy.AddDays(16), TipoProducto.Yogurt, 6000);

        Assert.Throws<DomainException>(() => venta.AgregarDetalle(lote, 5));
        Assert.Equal(50, lote.CantidadDisponible);
    }

    [Fact]
    public void Venta_AgregarDetalle_CalculaCantidadYTotal()
    {
        var lote1 = LoteProducto.Crear(Hoy, 100, Queso);
        var lote2 = LoteProducto.Crear(Hoy.AddDays(1), 150, Queso);
        var venta = new Venta(Hoy.AddDays(2), TipoProducto.Queso, 28000);

        venta.AgregarDetalle(lote1, 10);
        venta.AgregarDetalle(lote2, 2);

        Assert.Equal(12, venta.Cantidad);
        Assert.Equal(336000, venta.Total);
        Assert.Equal(2, venta.Detalles.Count);
    }
}
