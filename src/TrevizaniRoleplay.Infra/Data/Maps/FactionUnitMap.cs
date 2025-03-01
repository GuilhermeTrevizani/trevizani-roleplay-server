using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class FactionUnitMap : IEntityTypeConfiguration<FactionUnit>
{
    public void Configure(EntityTypeBuilder<FactionUnit> builder)
    {
        builder.ToTable("FactionsUnits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(25);
        builder.Property(x => x.Status).HasMaxLength(25);
        builder.HasOne(x => x.Faction).WithMany().HasForeignKey(x => x.FactionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Character).WithMany().HasForeignKey(x => x.CharacterId).OnDelete(DeleteBehavior.Restrict);
    }
}