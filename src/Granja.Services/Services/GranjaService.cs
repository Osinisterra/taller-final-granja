using AutoMapper;
using Granja.Domain.Entities;
using Granja.Domain.Exceptions;
using Granja.Domain.Interfaces;
using Granja.Services.Dtos;
using Granja.Services.Interfaces;

namespace Granja.Services.Services;

public class GranjaService : IGranjaService
{
    private readonly IGranjaRepository _granjas;
    private readonly IVacaRepository _vacas;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly IMapper _mapper;

    public GranjaService(IGranjaRepository granjas, IVacaRepository vacas, IUnitOfWork uow, IClock clock, IMapper mapper)
    {
        _granjas = granjas;
        _vacas = vacas;
        _uow = uow;
        _clock = clock;
        _mapper = mapper;
    }

    public async Task<GranjaResponse> ObtenerAsync(CancellationToken ct = default) =>
        _mapper.Map<GranjaResponse>(await ObtenerGranjaAsync(_granjas, ct));

    public async Task<GranjaResponse> ActualizarAsync(ActualizarGranjaRequest request, CancellationToken ct = default)
    {
        var granja = await ObtenerGranjaAsync(_granjas, ct);

        if (request.Parametros.GroupBy(p => p.Tipo).Any(g => g.Count() > 1))
            throw new DomainException("Cada producto solo puede aparecer una vez en los parámetros.");

        granja.ActualizarProduccion(request.NumeroVacas, request.LitrosPorVaca);
        foreach (var p in request.Parametros)
            granja.ParametroDe(p.Tipo).Actualizar(p.Rendimiento, p.VidaUtilDias, p.Precio);

        await SincronizarVacasAsync(granja, _vacas, _clock.Today, ct);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<GranjaResponse>(granja);
    }

    internal static async Task<GranjaLechera> ObtenerGranjaAsync(IGranjaRepository repo, CancellationToken ct) =>
        await repo.ObtenerAsync(ct) ?? throw new EntidadNoEncontradaException("la granja", "configuración");

    /// <summary>
    /// Garantiza que existan exactamente n vacas activas: crea las que falten
    /// (o reactiva inactivas) y desactiva las sobrantes, conservando su historial.
    /// </summary>
    internal static async Task SincronizarVacasAsync(GranjaLechera granja, IVacaRepository vacas, DateOnly hoy, CancellationToken ct)
    {
        var todas = (await vacas.ListarAsync(ct)).OrderBy(v => v.Id).ToList();
        var activas = todas.Where(v => v.Activa).ToList();
        var n = granja.NumeroVacas;

        if (activas.Count > n)
        {
            foreach (var sobrante in activas.Skip(n)) sobrante.Desactivar();
            return;
        }

        var faltantes = n - activas.Count;
        foreach (var inactiva in todas.Where(v => !v.Activa).Take(faltantes))
        {
            inactiva.Activar();
            faltantes--;
        }

        for (var i = 0; i < faltantes; i++)
            await vacas.AgregarAsync(new Vaca($"Vaca {todas.Count + i + 1}", hoy), ct);
    }
}
