using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class TruckerLocationMap : IEntityTypeConfiguration<TruckerLocation>
{
    public void Configure(EntityTypeBuilder<TruckerLocation> builder)
    {
        builder.ToTable("TruckerLocations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(50);
    }
}