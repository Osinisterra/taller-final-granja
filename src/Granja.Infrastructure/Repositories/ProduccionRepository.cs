using Granja.Domain.Entities;
using Granja.Domain.Interfaces;
using Granja.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Granja.Infrastructure.Repositories;

public class ProduccionRepository : IProduccionRepository
{
    private readonly GranjaDbContext _context;

    public ProduccionRepository(GranjaDbContext context) => _context = context;

    public Task<bool> ExisteParaFechaAsync(DateOnly fecha, CancellationToken ct = default) =>
        _context.Producciones.AnyAsync(p => p.Fecha == fecha, ct);

    public async Task<decimal> LitrosProducidosAsync(DateOnly fecha, CancellationToken ct = default)
    {
        var litros = await _context.Producciones.Where(p => p.Fecha == fecha).Select(p => p.Litros).ToListAsync(ct);
        return litros.Sum();
    }

    public async Task<IReadOnlyList<ProduccionLeche>> ListarAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default) =>
        await _context.Producciones
            .Include(p => p.Vaca)
            .Where(p => (desde == null || p.Fecha >= desde) && (hasta == null || p.Fecha <= hasta))
            .OrderBy(p => p.Fecha).ThenBy(p => p.VacaId)
            .ToListAsync(ct);

    public Task AgregarRangoAsync(IEnumerable<ProduccionLeche> producciones, CancellationToken ct = default) =>
        _context.Producciones.AddRangeAsync(producciones, ct);
}
