using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Services.Dtos;
using Granja.Tests.Support;
using static Granja.Tests.Support.GranjaTestContext;

namespace Granja.Tests.Services;

public class GranjaYVacasTests : IDisposable
{
    private readonly GranjaTestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    [Fact]
    public void Mapeos_DeAutoMapper_SonValidos()
    {
        var config = (AutoMapper.IConfigurationProvider)_ctx.Mapper.ConfigurationProvider;
        config.AssertConfigurationIsValid();
    }

    [Fact]
    public async Task VacaRecienRegistrada_TieneProduccionCero()
    {
        var vacas = await _ctx.Vacas.ListarAsync(false);

        Assert.Equal(10, vacas.Count);
        Assert.All(vacas, v => Assert.Equal(0, v.LitrosProducidos));
    }

    [Fact]
    public async Task ActualizarGranja_CambiaNYL_YAjustaLasVacas()
    {
        var respuesta = await _ctx.Granja.ActualizarAsync(new ActualizarGranjaRequest
        {
            NumeroVacas = 12,
            LitrosPorVaca = 20,
            Parametros = [new() { Tipo = TipoProducto.Queso, Rendimiento = 8, VidaUtilDias = 20, Precio = 30000 }]
        });

        Assert.Equal(240, respuesta.ProduccionDiariaEsperada);
        Assert.Equal(8, respuesta.Parametros.Single(p => p.Tipo == TipoProducto.Queso).Rendimiento);
        Assert.Equal(12, (await _ctx.Vacas.ListarAsync(false)).Count);

        var produccion = await _ctx.Produccion.RegistrarProduccionAsync(new RegistrarProduccionRequest { Fecha = Dia(1) });
        Assert.Equal(240, produccion.LitrosProducidos);

        await _ctx.Granja.ActualizarAsync(new ActualizarGranjaRequest { NumeroVacas = 5, LitrosPorVaca = 20 });
        Assert.Equal(5, (await _ctx.Vacas.ListarAsync(false)).Count);
        Assert.Equal(12, (await _ctx.Vacas.ListarAsync(true)).Count);
    }

    [Fact]
    public async Task ActualizarGranja_ConValoresInvalidos_LanzaDomainException()
    {
        await Assert.ThrowsAsync<DomainException>(() => _ctx.Granja.ActualizarAsync(new ActualizarGranjaRequest
        {
            NumeroVacas = 10,
            LitrosPorVaca = 15,
            Parametros = [new() { Tipo = TipoProducto.Yogurt, Rendimiento = 0, VidaUtilDias = 15, Precio = 1 }]
        }));
    }

    [Fact]
    public async Task ReporteVacas_CalculaPromedioMayorMenorYBajoPromedio()
    {
        await _ctx.Produccion.RegistrarProduccionAsync(new RegistrarProduccionRequest { Fecha = Dia(1) });
        await _ctx.Produccion.RegistrarProduccionAsync(new RegistrarProduccionRequest { Fecha = Dia(2) });

        var reporte = await _ctx.Reportes.VacasAsync(null, null);

        Assert.Equal(10, reporte.NumeroVacas);
        Assert.Equal(300, reporte.TotalLitros);
        Assert.Equal(30, reporte.PromedioPorVaca);
        Assert.Equal(15, reporte.PromedioDiarioPorVaca);
        Assert.NotNull(reporte.MayorProduccion);
        Assert.NotNull(reporte.MenorProduccion);
        Assert.Empty(reporte.BajoPromedio); // en el modelo n × l todas producen lo mismo
    }

    [Fact]
    public async Task ProximosAVencer_DevuelveLotesQueVencenEnLosProximosDias()
    {
        await _ctx.Produccion.RegistrarProduccionAsync(new RegistrarProduccionRequest { Fecha = Dia(1) });
        await _ctx.Produccion.TransformarAsync(new TransformacionRequest { Fecha = Dia(1), TipoProducto = TipoProducto.Yogurt, Litros = 50 });
        await _ctx.Produccion.TransformarAsync(new TransformacionRequest { Fecha = Dia(1), TipoProducto = TipoProducto.Queso, Litros = 100 });

        var alertas = await _ctx.Inventario.ProximosAVencerAsync(3, Dia(14));

        var lote = Assert.Single(alertas);
        Assert.Equal(TipoProducto.Yogurt, lote.TipoProducto);
    }
}
