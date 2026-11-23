using Granja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Granja.Infrastructure.Data.Configurations;

public class GranjaConfiguration : IEntityTypeConfiguration<GranjaLechera>
{
    public void Configure(EntityTypeBuilder<GranjaLechera> b)
    {
        b.ToTable("Granjas");
        b.HasKey(g => g.Id);
        b.Property(g => g.Nombre).HasMaxLength(100).IsRequired();
        b.Ignore(g => g.ProduccionDiariaEsperada);
        b.HasMany(g => g.Parametros).WithOne().HasForeignKey(p => p.GranjaId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(g => g.Parametros).HasField("_parametros").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class ParametroProductoConfiguration : IEntityTypeConfiguration<ParametroProducto>
{
    public void Configure(EntityTypeBuilder<ParametroProducto> b)
    {
        b.ToTable("ParametrosProducto");
        b.HasKey(p => p.Id);
        b.Property(p => p.Tipo).HasConversion<string>().HasMaxLength(20);
        b.Property(p => p.Unidad).HasMaxLength(20);
        b.HasIndex(p => new { p.GranjaId, p.Tipo }).IsUnique();
    }
}
