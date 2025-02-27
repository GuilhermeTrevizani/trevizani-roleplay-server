using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class FactionEquipmentItemModelConfiguration : IEntityTypeConfiguration<FactionEquipmentItem>
{
    public void Configure(EntityTypeBuilder<FactionEquipmentItem> builder)
    {
        builder.ToTable("FactionsEquipmentsItems");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.FactionEquipment).WithMany(x => x.Items).HasForeignKey(x => x.FactionEquipmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ItemTemplate).WithMany().HasForeignKey(x => x.ItemTemplateId).OnDelete(DeleteBehavior.Restrict);
    }
}