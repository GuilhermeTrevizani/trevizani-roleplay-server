using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class PropertyItemModelConfiguration : IEntityTypeConfiguration<PropertyItem>
{
    public void Configure(EntityTypeBuilder<PropertyItem> builder)
    {
        builder.ToTable("PropertiesItems");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Property).WithMany(x => x.Items).HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ItemTemplate).WithMany().HasForeignKey(x => x.ItemTemplateId).OnDelete(DeleteBehavior.Restrict);
    }
}