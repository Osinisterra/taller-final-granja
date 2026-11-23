using Granja.Domain.Entities;
using Granja.Domain.Interfaces;
using Granja.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Granja.Infrastructure.Repositories;

public class VacaRepository : Repository<Vaca>, IVacaRepository
{
    public VacaRepository(GranjaDbContext context) : base(context) { }

    public override async Task<IReadOnlyList<Vaca>> ListarAsync(CancellationToken ct = default) =>
        await Context.Vacas.OrderBy(v => v.Id).ToListAsync(ct);

    public async Task<IReadOnlyList<Vaca>> ListarConProduccionAsync(bool soloActivas, CancellationToken ct = default) =>
        await Context.Vacas
            .Include(v => v.Producciones)
            .Where(v => !soloActivas || v.Activa)
            .OrderBy(v => v.Id)
            .ToListAsync(ct);

    public Task<Vaca?> ObtenerConProduccionAsync(int id, CancellationToken ct = default) =>
        Context.Vacas.Include(v => v.Producciones).FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<IReadOnlyList<Vaca>> ListarActivasAsync(CancellationToken ct = default) =>
        await Context.Vacas.Where(v => v.Activa).OrderBy(v => v.Id).ToListAsync(ct);
}
