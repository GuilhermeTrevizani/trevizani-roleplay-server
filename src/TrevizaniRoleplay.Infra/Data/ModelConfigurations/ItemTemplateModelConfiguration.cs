using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class ItemTemplateModelConfiguration : IEntityTypeConfiguration<ItemTemplate>
{
    public void Configure(EntityTypeBuilder<ItemTemplate> builder)
    {
        builder.ToTable("ItemsTemplates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(50);
        builder.Property(x => x.ObjectModel).HasMaxLength(50);
        builder.Property(x => x.Image).HasMaxLength(50);
    }
}