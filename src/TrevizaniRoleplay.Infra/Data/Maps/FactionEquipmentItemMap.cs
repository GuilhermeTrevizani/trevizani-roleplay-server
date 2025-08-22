using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class FactionEquipmentItemMap : IEntityTypeConfiguration<FactionEquipmentItem>
{
    public void Configure(EntityTypeBuilder<FactionEquipmentItem> builder)
    {
        builder.ToTable("FactionsEquipmentsItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Weapon).HasMaxLength(50);
        builder.HasOne(x => x.FactionEquipment).WithMany(x => x.Items).HasForeignKey(x => x.FactionEquipmentId).OnDelete(DeleteBehavior.Restrict);
    }
}