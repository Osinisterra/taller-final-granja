using Granja.Services.Interfaces;
using Granja.Services.Mapping;
using Granja.Services.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Granja.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<GranjaMappingProfile>());

        services.AddScoped<IGranjaService, GranjaService>();
        services.AddScoped<IVacaService, VacaService>();
        services.AddScoped<IProduccionService, ProduccionService>();
        services.AddScoped<IInventarioService, InventarioService>();
        services.AddScoped<IVentaService, VentaService>();
        services.AddScoped<IReporteService, ReporteService>();

        return services;
    }
}
