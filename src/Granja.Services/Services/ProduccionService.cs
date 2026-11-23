using AutoMapper;
using Granja.Domain.Entities;
using Granja.Domain.Exceptions;
using Granja.Domain.Interfaces;
using Granja.Domain.Rules;
using Granja.Services.Dtos;
using Granja.Services.Interfaces;
using Granja.Services.Mapping;

namespace Granja.Services.Services;

public class ProduccionService : IProduccionService
{
    private readonly IGranjaRepository _granjas;
    private readonly IVacaRepository _vacas;
    private readonly IProduccionRepository _producciones;
    private readonly ILoteRepository _lotes;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly IMapper _mapper;

    public ProduccionService(IGranjaRepository granjas, IVacaRepository vacas, IProduccionRepository producciones,
        ILoteRepository lotes, IUnitOfWork uow, IClock clock, IMapper mapper)
    {
        _granjas = granjas;
        _vacas = vacas;
        _producciones = producciones;
        _lotes = lotes;
        _uow = uow;
        _clock = clock;
        _mapper = mapper;
    }

    /// <summary>RN-01: registra n × l litros (l por cada vaca activa), una sola vez por fecha.</summary>
    public async Task<ProduccionDiaResponse> RegistrarProduccionAsync(RegistrarProduccionRequest request, CancellationToken ct = default)
    {
        var fecha = request.Fecha ?? _clock.Today;

        if (await _producciones.ExisteParaFechaAsync(fecha, ct))
            throw new ProduccionDuplicadaException(fecha);

        var granja = await GranjaService.ObtenerGranjaAsync(_granjas, ct);
        var vacas = await _vacas.ListarActivasAsync(ct);

        if (vacas.Count != granja.NumeroVacas)
        {
            await GranjaService.SincronizarVacasAsync(granja, _vacas, fecha, ct);
            await _uow.SaveChangesAsync(ct);
            vacas = await _vacas.ListarActivasAsync(ct);
        }

        var registros = vacas.Select(v => new ProduccionLeche(v.Id, fecha, granja.LitrosPorVaca)).ToList();
        await _producciones.AgregarRangoAsync(registros, ct);
        await _uow.SaveChangesAsync(ct);

        var producidos = registros.Sum(r => r.Litros);
        return new ProduccionDiaResponse
        {
            Fecha = fecha,
            NumeroVacas = registros.Count,
            LitrosProducidos = producidos,
            LitrosTransformados = 0,
            LitrosDisponibles = producidos
        };
    }

    public async Task<ProduccionDiaResponse> ObtenerDiaAsync(DateOnly fecha, CancellationToken ct = default)
    {
        var dia = (await ResumirDiasAsync(fecha, fecha, ct)).FirstOrDefault();
        return dia ?? throw new EntidadNoEncontradaException("producción de leche para la fecha", fecha.ToString("yyyy-MM-dd"));
    }

    public async Task<PaginaResponse<ProduccionDiaResponse>> ListarAsync(DateOnly? desde, DateOnly? hasta, PaginacionRequest paginacion, CancellationToken ct = default)
    {
        Paginador.Salto(paginacion);
        return Paginador.Paginar(await ResumirDiasAsync(desde, hasta, ct), paginacion);
    }

    private async Task<IReadOnlyList<ProduccionDiaResponse>> ResumirDiasAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct)
    {
        Validaciones.Rango(desde, hasta);
        var producciones = await _producciones.ListarAsync(desde, hasta, ct);
        var lotes = await _lotes.BuscarAsync(null, desde, hasta, ct);

        return producciones
            .GroupBy(p => p.Fecha)
            .Select(g =>
            {
                var producidos = g.Sum(p => p.Litros);
                var transformados = lotes.Where(l => l.FechaProduccion == g.Key).Sum(l => l.LitrosUsados);
                return new ProduccionDiaResponse
                {
                    Fecha = g.Key,
                    NumeroVacas = g.Count(),
                    LitrosProducidos = producidos,
                    LitrosTransformados = transformados,
                    LitrosDisponibles = ReglasLeche.NoAprovechada(producidos, transformados)
                };
            })
            .OrderBy(d => d.Fecha)
            .ToList();
    }

    /// <summary>RN-02, RN-03, RN-04: transforma litros de una fecha en un producto y crea el lote.</summary>
    public async Task<LoteResponse> TransformarAsync(TransformacionRequest request, CancellationToken ct = default)
    {
        var fecha = request.Fecha ?? _clock.Today;

        if (!await _producciones.ExisteParaFechaAsync(fecha, ct))
            throw new EntidadNoEncontradaException("producción de leche para la fecha", fecha.ToString("yyyy-MM-dd"));

        var producidos = await _producciones.LitrosProducidosAsync(fecha, ct);
        var transformados = await _lotes.LitrosTransformadosAsync(fecha, ct);
        ReglasLeche.ValidarTransformacion(fecha, producidos, transformados, request.Litros);

        var granja = await GranjaService.ObtenerGranjaAsync(_granjas, ct);
        var lote = LoteProducto.Crear(fecha, request.Litros, granja.ParametroDe(request.TipoProducto));

        await _lotes.AgregarAsync(lote, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<LoteResponse>(lote, o => o.Items[GranjaMappingProfile.FechaKey] = fecha);
    }
}
