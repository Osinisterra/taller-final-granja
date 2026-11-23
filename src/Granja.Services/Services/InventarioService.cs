using AutoMapper;
using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Domain.Interfaces;
using Granja.Services.Dtos;
using Granja.Services.Interfaces;
using Granja.Services.Mapping;

namespace Granja.Services.Services;

public class InventarioService : IInventarioService
{
    private readonly ILoteRepository _lotes;
    private readonly IGranjaRepository _granjas;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly IMapper _mapper;

    public InventarioService(ILoteRepository lotes, IGranjaRepository granjas, IUnitOfWork uow, IClock clock, IMapper mapper)
    {
        _lotes = lotes;
        _granjas = granjas;
        _uow = uow;
        _clock = clock;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<LoteResponse>> ListarLotesAsync(TipoProducto? tipo, bool soloDisponibles, DateOnly? fecha, CancellationToken ct = default)
    {
        var dia = fecha ?? _clock.Today;
        var lotes = await _lotes.BuscarAsync(tipo, null, null, ct);
        if (soloDisponibles) lotes = lotes.Where(l => l.EsVendible(dia)).ToList();
        return Mapear(lotes, dia);
    }

    public async Task<LoteResponse> ObtenerLoteAsync(int id, DateOnly? fecha, CancellationToken ct = default)
    {
        var lote = await _lotes.ObtenerPorIdAsync(id, ct) ?? throw new EntidadNoEncontradaException("el lote", id);
        return _mapper.Map<LoteResponse>(lote, o => o.Items[GranjaMappingProfile.FechaKey] = fecha ?? _clock.Today);
    }

    public async Task<InventarioResumenResponse> ResumenAsync(DateOnly? fecha, CancellationToken ct = default)
    {
        var dia = fecha ?? _clock.Today;
        var granja = await GranjaService.ObtenerGranjaAsync(_granjas, ct);
        var lotes = await _lotes.BuscarAsync(null, null, null, ct);

        return new InventarioResumenResponse
        {
            Fecha = dia,
            Productos = Enum.GetValues<TipoProducto>().Select(tipo =>
            {
                var delTipo = lotes.Where(l => l.Tipo == tipo).ToList();
                return new InventarioResumenItem
                {
                    TipoProducto = tipo,
                    Unidad = granja.ParametroDe(tipo).Unidad,
                    DisponibleVendible = delTipo.Where(l => l.EsVendible(dia)).Sum(l => l.CantidadDisponible),
                    VencidoSinProcesar = delTipo.Where(l => l.EstaVencido(dia)).Sum(l => l.CantidadDisponible),
                    Danado = delTipo.Sum(l => l.CantidadDanada),
                    LotesVigentes = delTipo.Count(l => l.EsVendible(dia))
                };
            }).ToList()
        };
    }

    /// <summary>Extensión opcional: lotes vendibles que vencen dentro de los próximos <paramref name="dias"/> días.</summary>
    public async Task<IReadOnlyList<LoteResponse>> ProximosAVencerAsync(int dias, DateOnly? fecha, CancellationToken ct = default)
    {
        if (dias < 0) throw new DomainException("Los días deben ser cero o más.");
        var dia = fecha ?? _clock.Today;
        var limite = dia.AddDays(dias);
        var lotes = (await _lotes.BuscarAsync(null, null, null, ct))
            .Where(l => l.EsVendible(dia) && l.FechaVencimiento <= limite)
            .OrderBy(l => l.FechaVencimiento)
            .ToList();
        return Mapear(lotes, dia);
    }

    /// <summary>RN-08: los lotes vencidos a la fecha pasan su disponible a dañado.</summary>
    public async Task<ProcesarVencimientosResponse> ProcesarVencimientosAsync(ProcesarVencimientosRequest request, CancellationToken ct = default)
    {
        var fecha = request.Fecha ?? _clock.Today;
        var vencidos = await _lotes.ObtenerVencidosConExistenciasAsync(fecha, ct);

        var items = vencidos.Select(l =>
        {
            var perdida = l.MarcarComoDanado();
            return new LoteDanadoItem
            {
                LoteId = l.Id,
                TipoProducto = l.Tipo,
                FechaProduccion = l.FechaProduccion,
                FechaVencimiento = l.FechaVencimiento,
                CantidadDanada = perdida
            };
        }).ToList();

        await _uow.SaveChangesAsync(ct);

        return new ProcesarVencimientosResponse
        {
            Fecha = fecha,
            LotesAfectados = items.Count,
            Lotes = items,
            TotalPorTipo = items.GroupBy(i => i.TipoProducto)
                .Select(g => new TotalPorTipo { TipoProducto = g.Key, Cantidad = g.Sum(i => i.CantidadDanada) })
                .ToList()
        };
    }

    private List<LoteResponse> Mapear(IEnumerable<Domain.Entities.LoteProducto> lotes, DateOnly dia) =>
        _mapper.Map<List<LoteResponse>>(lotes, o => o.Items[GranjaMappingProfile.FechaKey] = dia);
}
