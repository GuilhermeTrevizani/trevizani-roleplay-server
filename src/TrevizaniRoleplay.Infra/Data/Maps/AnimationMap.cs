using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class AnimationMap : IEntityTypeConfiguration<Animation>
{
    public void Configure(EntityTypeBuilder<Animation> builder)
    {
        builder.ToTable("Animations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Display).HasMaxLength(25);
        builder.Property(x => x.Dictionary).HasMaxLength(100);
        builder.Property(x => x.Name).HasMaxLength(100);
        builder.Property(x => x.Category).HasMaxLength(25);
        builder.Property(x => x.Scenario).HasMaxLength(100);
    }
}