using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class BlipModelConfiguration : IEntityTypeConfiguration<Blip>
{
    public void Configure(EntityTypeBuilder<Blip> builder)
    {
        builder.ToTable("Blips");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100);
    }
}