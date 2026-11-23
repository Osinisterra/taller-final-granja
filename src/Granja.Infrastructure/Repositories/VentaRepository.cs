using Granja.Domain.Entities;
using Granja.Domain.Enums;
using Granja.Domain.Interfaces;
using Granja.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Granja.Infrastructure.Repositories;

public class VentaRepository : Repository<Venta>, IVentaRepository
{
    public VentaRepository(GranjaDbContext context) : base(context) { }

    public Task<Venta?> ObtenerConDetalleAsync(int id, CancellationToken ct = default) =>
        Context.Ventas.Include(v => v.Detalles).FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<IReadOnlyList<Venta>> BuscarAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo, CancellationToken ct = default) =>
        await Context.Ventas
            .Include(v => v.Detalles)
            .Where(v => (desde == null || v.Fecha >= desde)
                        && (hasta == null || v.Fecha <= hasta)
                        && (tipo == null || v.Tipo == tipo))
            .OrderBy(v => v.Fecha).ThenBy(v => v.Id)
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<Venta> Items, int Total)> BuscarPaginadoAsync(FiltroVentas f, int saltar, int tomar, CancellationToken ct = default)
    {
        var query = Context.Ventas.AsQueryable();

        if (f.Desde is { } desde) query = query.Where(v => v.Fecha >= desde);
        if (f.Hasta is { } hasta) query = query.Where(v => v.Fecha <= hasta);
        if (f.Tipo is { } tipo) query = query.Where(v => v.Tipo == tipo);
        if (f.CantidadMinima is { } min) query = query.Where(v => v.Cantidad >= min);
        if (f.CantidadMaxima is { } max) query = query.Where(v => v.Cantidad <= max);

        var total = await query.CountAsync(ct);

        query = (f.OrdenarPor, f.Descendente) switch
        {
            (OrdenVentas.Cantidad, false) => query.OrderBy(v => v.Cantidad).ThenBy(v => v.Id),
            (OrdenVentas.Cantidad, true) => query.OrderByDescending(v => v.Cantidad).ThenByDescending(v => v.Id),
            (OrdenVentas.Total, false) => query.OrderBy(v => v.Total).ThenBy(v => v.Id),
            (OrdenVentas.Total, true) => query.OrderByDescending(v => v.Total).ThenByDescending(v => v.Id),
            (_, true) => query.OrderByDescending(v => v.Fecha).ThenByDescending(v => v.Id),
            _ => query.OrderBy(v => v.Fecha).ThenBy(v => v.Id)
        };

        var items = await query.Skip(saltar).Take(tomar).Include(v => v.Detalles).ToListAsync(ct);
        return (items, total);
    }
}
