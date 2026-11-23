using Granja.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Granja.Infrastructure.Data;

public class GranjaDbContext : DbContext
{
    public GranjaDbContext(DbContextOptions<GranjaDbContext> options) : base(options) { }

    public DbSet<GranjaLechera> Granjas => Set<GranjaLechera>();
    public DbSet<ParametroProducto> ParametrosProducto => Set<ParametroProducto>();
    public DbSet<Vaca> Vacas => Set<Vaca>();
    public DbSet<ProduccionLeche> Producciones => Set<ProduccionLeche>();
    public DbSet<LoteProducto> Lotes => Set<LoteProducto>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<VentaDetalle> VentaDetalles => Set<VentaDetalle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GranjaDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // SQLite no soporta decimal de forma nativa (no permite comparar ni ordenar en SQL).
        // Se almacena como REAL para poder filtrar CantidadDisponible > 0 en la base de datos.
        configurationBuilder.Properties<decimal>().HaveConversion<double>();
    }
}
