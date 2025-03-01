using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class FactionEquipmentMap : IEntityTypeConfiguration<FactionEquipment>
{
    public void Configure(EntityTypeBuilder<FactionEquipment> builder)
    {
        builder.ToTable("FactionsEquipmentss");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(25);
        builder.HasOne(x => x.Faction).WithMany().HasForeignKey(x => x.FactionId).OnDelete(DeleteBehavior.Restrict);
    }
}