namespace Granja.Domain.Interfaces;

/// <summary>Confirma todos los cambios pendientes en una sola transacción.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
