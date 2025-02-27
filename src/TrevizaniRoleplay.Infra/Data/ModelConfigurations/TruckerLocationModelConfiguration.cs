using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class TruckerLocationModelConfiguration : IEntityTypeConfiguration<TruckerLocation>
{
    public void Configure(EntityTypeBuilder<TruckerLocation> builder)
    {
        builder.ToTable("TruckerLocations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(50);
    }
}