using Granja.Domain.Entities;
using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Domain.Rules;

namespace Granja.Tests.Domain;

public class ReglasLecheTests
{
    private static readonly DateOnly Hoy = new(2026, 12, 1);

    [Fact]
    public void ValidarTransformacion_DentroDelLimite_NoLanza()
    {
        var ex = Record.Exception(() => ReglasLeche.ValidarTransformacion(Hoy, 150, 100, 50));
        Assert.Null(ex);
    }

    [Fact]
    public void ValidarTransformacion_ExcedeLitrosDelDia_LanzaLecheInsuficiente()
    {
        var ex = Assert.Throws<LecheInsuficienteException>(() => ReglasLeche.ValidarTransformacion(Hoy, 150, 100, 51));
        Assert.Equal(50, ex.Disponibles);
    }

    [Fact]
    public void Granja_ProduccionDiariaEsperada_EsNPorL()
    {
        var granja = new GranjaLechera("G", 10, 15,
        [
            new ParametroProducto(TipoProducto.Queso, 10, 30, 1, "kg"),
            new ParametroProducto(TipoProducto.Yogurt, 1, 15, 1, "L")
        ]);

        Assert.Equal(150, granja.ProduccionDiariaEsperada);
    }

    [Fact]
    public void Granja_ValoresInvalidos_Lanza()
    {
        var granja = new GranjaLechera("G", 10, 15,
        [
            new ParametroProducto(TipoProducto.Queso, 10, 30, 1, "kg"),
            new ParametroProducto(TipoProducto.Yogurt, 1, 15, 1, "L")
        ]);

        Assert.Throws<DomainException>(() => granja.ActualizarProduccion(0, 15));
        Assert.Throws<DomainException>(() => granja.ParametroDe(TipoProducto.Queso).Actualizar(0, 30, 1));
    }
}
