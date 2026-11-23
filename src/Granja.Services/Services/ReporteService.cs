using AutoMapper;
using Granja.Domain.Enums;
using Granja.Domain.Interfaces;
using Granja.Domain.Rules;
using Granja.Services.Dtos;
using Granja.Services.Interfaces;

namespace Granja.Services.Services;

public class ReporteService : IReporteService
{
    private readonly IVentaRepository _ventas;
    private readonly IProduccionRepository _producciones;
    private readonly ILoteRepository _lotes;
    private readonly IVacaRepository _vacas;
    private readonly IGranjaRepository _granjas;
    private readonly IMapper _mapper;

    public ReporteService(IVentaRepository ventas, IProduccionRepository producciones, ILoteRepository lotes,
        IVacaRepository vacas, IGranjaRepository granjas, IMapper mapper)
    {
        _ventas = ventas;
        _producciones = producciones;
        _lotes = lotes;
        _vacas = vacas;
        _granjas = granjas;
        _mapper = mapper;
    }

    public async Task<ReporteVentasResponse> VentasAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo, CancellationToken ct = default)
    {
        Validaciones.Rango(desde, hasta);
        var ventas = await _ventas.BuscarAsync(desde, hasta, tipo, ct);

        return new ReporteVentasResponse
        {
            Desde = desde,
            Hasta = hasta,
            Tipo = tipo,
            NumeroVentas = ventas.Count,
            Ventas = _mapper.Map<List<ReporteVentaItem>>(ventas),
            TotalesPorProducto = ventas.GroupBy(v => v.Tipo)
                .Select(g => new ReporteVentaTotalTipo { TipoProducto = g.Key, Cantidad = g.Sum(v => v.Cantidad), Total = g.Sum(v => v.Total) })
                .OrderBy(t => t.TipoProducto).ToList(),
            TotalGeneral = ventas.Sum(v => v.Total)
        };
    }

    public async Task<ReporteLecheDiariaResponse> LecheDiariaAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        Validaciones.Rango(desde, hasta);
        var producciones = await _producciones.ListarAsync(desde, hasta, ct);
        var lotes = await _lotes.BuscarAsync(null, desde, hasta, ct);

        var dias = producciones.GroupBy(p => p.Fecha).OrderBy(g => g.Key).Select(g =>
        {
            var producidos = g.Sum(p => p.Litros);
            var transformados = lotes.Where(l => l.FechaProduccion == g.Key).Sum(l => l.LitrosUsados);
            return new LecheDiariaItem
            {
                Fecha = g.Key,
                NumeroVacas = g.Count(),
                LitrosProducidos = producidos,
                LitrosTransformados = transformados,
                LitrosNoAprovechados = ReglasLeche.NoAprovechada(producidos, transformados)
            };
        }).ToList();

        return new ReporteLecheDiariaResponse
        {
            Dias = dias,
            TotalProducido = dias.Sum(d => d.LitrosProducidos),
            TotalTransformado = dias.Sum(d => d.LitrosTransformados),
            TotalNoAprovechado = dias.Sum(d => d.LitrosNoAprovechados)
        };
    }

    public async Task<IReadOnlyList<ProductoDiarioItem>> ProductoDiarioAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        Validaciones.Rango(desde, hasta);
        var granja = await GranjaService.ObtenerGranjaAsync(_granjas, ct);
        var lotes = await _lotes.BuscarAsync(null, desde, hasta, ct);

        return lotes.GroupBy(l => new { l.FechaProduccion, l.Tipo })
            .OrderBy(g => g.Key.FechaProduccion).ThenBy(g => g.Key.Tipo)
            .Select(g => new ProductoDiarioItem
            {
                Fecha = g.Key.FechaProduccion,
                TipoProducto = g.Key.Tipo,
                Unidad = granja.ParametroDe(g.Key.Tipo).Unidad,
                Lotes = g.Count(),
                LitrosUsados = g.Sum(l => l.LitrosUsados),
                CantidadProducida = g.Sum(l => l.CantidadInicial)
            }).ToList();
    }

    public async Task<IReadOnlyList<ProductoTotalItem>> ProductoTotalAsync(CancellationToken ct = default)
    {
        var granja = await GranjaService.ObtenerGranjaAsync(_granjas, ct);
        var lotes = await _lotes.BuscarAsync(null, null, null, ct);
        var ventas = await _ventas.BuscarAsync(null, null, null, ct);

        return Enum.GetValues<TipoProducto>().Select(tipo =>
        {
            var delTipo = lotes.Where(l => l.Tipo == tipo).ToList();
            var ventasTipo = ventas.Where(v => v.Tipo == tipo).ToList();
            return new ProductoTotalItem
            {
                TipoProducto = tipo,
                Unidad = granja.ParametroDe(tipo).Unidad,
                Producido = delTipo.Sum(l => l.CantidadInicial),
                Vendido = ventasTipo.Sum(v => v.Cantidad),
                Disponible = delTipo.Sum(l => l.CantidadDisponible),
                Danado = delTipo.Sum(l => l.CantidadDanada),
                Ingresos = ventasTipo.Sum(v => v.Total)
            };
        }).ToList();
    }

    public async Task<ReporteProductoDanadoResponse> ProductoDanadoAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        Validaciones.Rango(desde, hasta);
        var danados = (await _lotes.BuscarAsync(null, null, null, ct))
            .Where(l => l.CantidadDanada > 0
                        && (desde == null || l.FechaVencimiento >= desde)
                        && (hasta == null || l.FechaVencimiento <= hasta))
            .OrderBy(l => l.FechaVencimiento)
            .ToList();

        return new ReporteProductoDanadoResponse
        {
            Lotes = _mapper.Map<List<LoteDanadoItem>>(danados),
            TotalPorTipo = danados.GroupBy(l => l.Tipo)
                .Select(g => new TotalPorTipo { TipoProducto = g.Key, Cantidad = g.Sum(l => l.CantidadDanada) })
                .OrderBy(t => t.TipoProducto).ToList()
        };
    }

    /// <summary>Reporte de vacas: promedio, la que más y la que menos produce, y las que están por debajo del promedio.</summary>
    public async Task<ReporteVacasResponse> VacasAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        Validaciones.Rango(desde, hasta);
        var producciones = await _producciones.ListarAsync(desde, hasta, ct);
        var vacas = await _vacas.ListarAsync(ct);

        var items = vacas
            .Where(v => v.Activa || producciones.Any(p => p.VacaId == v.Id))
            .Select(v =>
            {
                var propias = producciones.Where(p => p.VacaId == v.Id).ToList();
                var total = propias.Sum(p => p.Litros);
                return new VacaProduccionItem
                {
                    VacaId = v.Id,
                    Nombre = v.Nombre,
                    LitrosTotales = total,
                    DiasProducidos = propias.Count,
                    PromedioDiario = propias.Count == 0 ? 0 : Math.Round(total / propias.Count, 2)
                };
            })
            .OrderBy(i => i.VacaId)
            .ToList();

        var respuesta = new ReporteVacasResponse { Desde = desde, Hasta = hasta, NumeroVacas = items.Count, Vacas = items };
        if (items.Count == 0) return respuesta;

        respuesta.TotalLitros = items.Sum(i => i.LitrosTotales);
        respuesta.PromedioPorVaca = Math.Round(respuesta.TotalLitros / items.Count, 2);
        var dias = producciones.Count;
        respuesta.PromedioDiarioPorVaca = dias == 0 ? 0 : Math.Round(respuesta.TotalLitros / dias, 2);
        respuesta.MayorProduccion = items.OrderByDescending(i => i.LitrosTotales).ThenBy(i => i.VacaId).First();
        respuesta.MenorProduccion = items.OrderBy(i => i.LitrosTotales).ThenBy(i => i.VacaId).First();
        respuesta.BajoPromedio = items.Where(i => i.LitrosTotales < respuesta.PromedioPorVaca).ToList();
        return respuesta;
    }
}
