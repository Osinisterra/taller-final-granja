using Granja.Domain.Entities;
using Granja.Domain.Interfaces;
using Granja.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Granja.Infrastructure.Repositories;

public class GranjaRepository : IGranjaRepository
{
    private readonly GranjaDbContext _context;

    public GranjaRepository(GranjaDbContext context) => _context = context;

    public Task<GranjaLechera?> ObtenerAsync(CancellationToken ct = default) =>
        _context.Granjas.Include(g => g.Parametros).OrderBy(g => g.Id).FirstOrDefaultAsync(ct);

    public async Task AgregarAsync(GranjaLechera granja, CancellationToken ct = default) =>
        await _context.Granjas.AddAsync(granja, ct);
}
