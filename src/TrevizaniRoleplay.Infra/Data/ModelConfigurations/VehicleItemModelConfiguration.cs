using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class VehicleItemModelConfiguration : IEntityTypeConfiguration<VehicleItem>
{
    public void Configure(EntityTypeBuilder<VehicleItem> builder)
    {
        builder.ToTable("VehiclesItems");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Vehicle).WithMany(x => x.Items).HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ItemTemplate).WithMany().HasForeignKey(x => x.ItemTemplateId).OnDelete(DeleteBehavior.Restrict);
    }
}