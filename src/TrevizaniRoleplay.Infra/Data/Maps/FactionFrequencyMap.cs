using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class FactionFrequencyMap : IEntityTypeConfiguration<FactionFrequency>
{
    public void Configure(EntityTypeBuilder<FactionFrequency> builder)
    {
        builder.ToTable("FactionsFrequencies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(10);
        builder.HasOne(x => x.Faction).WithMany().HasForeignKey(x => x.FactionId);
    }
}