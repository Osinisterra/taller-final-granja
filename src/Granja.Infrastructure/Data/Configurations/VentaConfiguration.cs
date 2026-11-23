using Granja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Granja.Infrastructure.Data.Configurations;

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> b)
    {
        b.ToTable("Ventas");
        b.HasKey(v => v.Id);
        b.Property(v => v.Tipo).HasConversion<string>().HasMaxLength(20);
        b.HasIndex(v => v.Fecha);
        b.HasMany(v => v.Detalles).WithOne().HasForeignKey(d => d.VentaId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(v => v.Detalles).HasField("_detalles").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class VentaDetalleConfiguration : IEntityTypeConfiguration<VentaDetalle>
{
    public void Configure(EntityTypeBuilder<VentaDetalle> b)
    {
        b.ToTable("VentaDetalles");
        b.HasKey(d => d.Id);
        b.HasOne(d => d.Lote).WithMany().HasForeignKey(d => d.LoteId).OnDelete(DeleteBehavior.Restrict);
    }
}
