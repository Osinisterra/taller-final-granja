using Granja.Domain.Entities;
using Granja.Domain.Enums;

namespace Granja.Domain.Interfaces;

public interface IGranjaRepository
{
    /// <summary>Devuelve la granja (única) con sus parámetros, o null si no existe.</summary>
    Task<GranjaLechera?> ObtenerAsync(CancellationToken ct = default);
    Task AgregarAsync(GranjaLechera granja, CancellationToken ct = default);
}

public interface IVacaRepository : IRepository<Vaca>
{
    Task<IReadOnlyList<Vaca>> ListarConProduccionAsync(bool soloActivas, CancellationToken ct = default);
    Task<Vaca?> ObtenerConProduccionAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Vaca>> ListarActivasAsync(CancellationToken ct = default);
}

public interface IProduccionRepository
{
    Task<bool> ExisteParaFechaAsync(DateOnly fecha, CancellationToken ct = default);
    Task<decimal> LitrosProducidosAsync(DateOnly fecha, CancellationToken ct = default);
    Task<IReadOnlyList<ProduccionLeche>> ListarAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
    Task AgregarRangoAsync(IEnumerable<ProduccionLeche> producciones, CancellationToken ct = default);
}

public interface ILoteRepository : IRepository<LoteProducto>
{
    /// <summary>Lotes del producto con disponible &gt; 0 y no vencidos a la fecha, ordenados por FechaProduccion ascendente (FIFO).</summary>
    Task<IReadOnlyList<LoteProducto>> ObtenerVendiblesAsync(TipoProducto tipo, DateOnly fecha, CancellationToken ct = default);
    /// <summary>Lotes con disponible &gt; 0 cuya fecha de vencimiento ya pasó.</summary>
    Task<IReadOnlyList<LoteProducto>> ObtenerVencidosConExistenciasAsync(DateOnly fecha, CancellationToken ct = default);
    Task<decimal> LitrosTransformadosAsync(DateOnly fecha, CancellationToken ct = default);
    Task<IReadOnlyList<LoteProducto>> BuscarAsync(TipoProducto? tipo, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
}

public interface IVentaRepository : IRepository<Venta>
{
    Task<Venta?> ObtenerConDetalleAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Venta>> BuscarAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo, CancellationToken ct = default);
    /// <summary>Búsqueda con filtros avanzados, orden y paginación ejecutada en la base de datos.</summary>
    Task<(IReadOnlyList<Venta> Items, int Total)> BuscarPaginadoAsync(FiltroVentas filtro, int saltar, int tomar, CancellationToken ct = default);
}
