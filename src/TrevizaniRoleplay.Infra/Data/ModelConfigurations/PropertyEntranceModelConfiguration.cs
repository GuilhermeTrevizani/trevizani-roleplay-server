using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class PropertyEntranceModelConfiguration : IEntityTypeConfiguration<PropertyEntrance>
{
    public void Configure(EntityTypeBuilder<PropertyEntrance> builder)
    {
        builder.ToTable("PropertiesEntrances");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Property).WithMany(x => x.Entrances).HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
    }
}