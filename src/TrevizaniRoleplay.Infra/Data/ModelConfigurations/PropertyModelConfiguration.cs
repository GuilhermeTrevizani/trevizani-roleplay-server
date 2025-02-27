using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class PropertyModelConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("Properties");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(50);
        builder.HasOne(x => x.Character).WithMany(x => x.Properties).HasForeignKey(x => x.CharacterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Faction).WithMany().HasForeignKey(x => x.FactionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ParentProperty).WithMany().HasForeignKey(x => x.ParentPropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
    }
}