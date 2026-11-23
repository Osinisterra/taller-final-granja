using Granja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Granja.Infrastructure.Data.Configurations;

public class VacaConfiguration : IEntityTypeConfiguration<Vaca>
{
    public void Configure(EntityTypeBuilder<Vaca> b)
    {
        b.ToTable("Vacas");
        b.HasKey(v => v.Id);
        b.Property(v => v.Nombre).HasMaxLength(100).IsRequired();
        b.Ignore(v => v.LitrosProducidos);
        b.HasMany(v => v.Producciones).WithOne(p => p.Vaca).HasForeignKey(p => p.VacaId);
        b.Navigation(v => v.Producciones).HasField("_producciones").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class ProduccionLecheConfiguration : IEntityTypeConfiguration<ProduccionLeche>
{
    public void Configure(EntityTypeBuilder<ProduccionLeche> b)
    {
        b.ToTable("ProduccionesLeche");
        b.HasKey(p => p.Id);
        // RN-01: una sola producción por fecha y por vaca.
        b.HasIndex(p => new { p.VacaId, p.Fecha }).IsUnique();
        b.HasIndex(p => p.Fecha);
    }
}
