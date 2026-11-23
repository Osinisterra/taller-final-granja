using Granja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Granja.Infrastructure.Data.Configurations;

public class LoteConfiguration : IEntityTypeConfiguration<LoteProducto>
{
    public void Configure(EntityTypeBuilder<LoteProducto> b)
    {
        b.ToTable("LotesProducto");
        b.HasKey(l => l.Id);
        b.Property(l => l.Tipo).HasConversion<string>().HasMaxLength(20);
        b.Ignore(l => l.CantidadVendida);
        b.HasIndex(l => new { l.Tipo, l.FechaProduccion });
    }
}
