using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class FurnitureModelConfiguration : IEntityTypeConfiguration<Furniture>
{
    public void Configure(EntityTypeBuilder<Furniture> builder)
    {
        builder.ToTable("Furnitures");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Category).HasMaxLength(50);
        builder.Property(x => x.Name).HasMaxLength(100);
        builder.Property(x => x.Model).HasMaxLength(50);
        builder.Property(x => x.TVTexture).HasMaxLength(50);
        builder.Property(x => x.Subcategory).HasMaxLength(50);
    }
}