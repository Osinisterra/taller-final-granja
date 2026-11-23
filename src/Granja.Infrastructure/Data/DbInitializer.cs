using Granja.Domain.Entities;
using Granja.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Granja.Infrastructure.Data;

/// <summary>Aplica migraciones y crea la granja inicial y sus n vacas si la base está vacía.</summary>
public static class DbInitializer
{
    public static async Task InicializarAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GranjaDbContext>();
        var semilla = scope.ServiceProvider.GetRequiredService<IOptions<GranjaSemillaOptions>>().Value;
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        await context.Database.MigrateAsync(ct);
        await SembrarAsync(context, semilla, clock.Today, ct);
    }

    public static async Task SembrarAsync(GranjaDbContext context, GranjaSemillaOptions semilla, DateOnly hoy, CancellationToken ct = default)
    {
        if (await context.Granjas.AnyAsync(ct)) return;

        var parametros = semilla.Productos.Select(p =>
            new ParametroProducto(p.Tipo, p.Rendimiento, p.VidaUtilDias, p.Precio, p.Unidad));
        var granja = new GranjaLechera(semilla.Nombre, semilla.NumeroVacas, semilla.LitrosPorVaca, parametros);
        context.Granjas.Add(granja);

        for (var i = 1; i <= granja.NumeroVacas; i++)
            context.Vacas.Add(new Vaca($"Vaca {i}", hoy));

        await context.SaveChangesAsync(ct);
    }
}
