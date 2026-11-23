using Granja.Domain.Interfaces;
using Granja.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Granja.Infrastructure.Repositories;

/// <summary>Implementación base del repositorio genérico sobre EF Core.</summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly GranjaDbContext Context;

    public Repository(GranjaDbContext context) => Context = context;

    public virtual async Task<T?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        await Context.Set<T>().FindAsync([id], ct);

    public virtual async Task<IReadOnlyList<T>> ListarAsync(CancellationToken ct = default) =>
        await Context.Set<T>().ToListAsync(ct);

    public async Task AgregarAsync(T entidad, CancellationToken ct = default) =>
        await Context.Set<T>().AddAsync(entidad, ct);
}
