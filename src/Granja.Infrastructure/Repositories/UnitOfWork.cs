using Granja.Domain.Interfaces;
using Granja.Infrastructure.Data;

namespace Granja.Infrastructure.Repositories;

/// <summary>
/// SaveChangesAsync de EF Core envuelve todos los cambios pendientes del DbContext
/// (venta + detalles + lotes modificados) en una única transacción.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly GranjaDbContext _context;

    public UnitOfWork(GranjaDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
