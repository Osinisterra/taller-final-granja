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
}
