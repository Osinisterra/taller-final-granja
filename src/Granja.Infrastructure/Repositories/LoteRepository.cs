using Granja.Domain.Entities;
using Granja.Domain.Enums;
using Granja.Domain.Interfaces;
using Granja.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Granja.Infrastructure.Repositories;

public class LoteRepository : Repository<LoteProducto>, ILoteRepository
{
    public LoteRepository(GranjaDbContext context) : base(context) { }

    public async Task<IReadOnlyList<LoteProducto>> ObtenerVendiblesAsync(TipoProducto tipo, DateOnly fecha, CancellationToken ct = default) =>
        await Context.Lotes
            .Where(l => l.Tipo == tipo && l.CantidadDisponible > 0 && l.FechaVencimiento >= fecha)
            .OrderBy(l => l.FechaProduccion).ThenBy(l => l.Id)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<LoteProducto>> ObtenerVencidosConExistenciasAsync(DateOnly fecha, CancellationToken ct = default) =>
        await Context.Lotes
            .Where(l => l.CantidadDisponible > 0 && l.FechaVencimiento < fecha)
            .OrderBy(l => l.FechaProduccion).ThenBy(l => l.Id)
            .ToListAsync(ct);

    public async Task<decimal> LitrosTransformadosAsync(DateOnly fecha, CancellationToken ct = default)
    {
        var litros = await Context.Lotes.Where(l => l.FechaProduccion == fecha).Select(l => l.LitrosUsados).ToListAsync(ct);
        return litros.Sum();
    }

    public async Task<IReadOnlyList<LoteProducto>> BuscarAsync(TipoProducto? tipo, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default) =>
        await Context.Lotes
            .Where(l => (tipo == null || l.Tipo == tipo)
                        && (desde == null || l.FechaProduccion >= desde)
                        && (hasta == null || l.FechaProduccion <= hasta))
            .OrderBy(l => l.FechaProduccion).ThenBy(l => l.Id)
            .ToListAsync(ct);
}
