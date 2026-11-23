namespace Granja.Domain.Interfaces;

/// <summary>Repositorio genérico. Los cambios se confirman con <see cref="IUnitOfWork"/>.</summary>
public interface IRepository<T> where T : class
{
    Task<T?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListarAsync(CancellationToken ct = default);
    Task AgregarAsync(T entidad, CancellationToken ct = default);
}
