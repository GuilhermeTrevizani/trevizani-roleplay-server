using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class PropertyFurnitureMap : IEntityTypeConfiguration<PropertyFurniture>
{
    public void Configure(EntityTypeBuilder<PropertyFurniture> builder)
    {
        builder.ToTable("PropertiesFurnitures");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Model).HasMaxLength(50);
        builder.HasOne(x => x.Property).WithMany(x => x.Furnitures).HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
    }
}